import { chromium } from "@playwright/test";
import { spawn } from "node:child_process";
import { createReadStream, existsSync, mkdirSync } from "node:fs";
import { readdir, rename, rm, stat, writeFile } from "node:fs/promises";
import { createServer as createHttpServer } from "node:http";
import { createServer as createProbeServer } from "node:net";
import { extname, join, normalize, resolve, sep } from "node:path";

const repoRoot = resolve(new URL("..", import.meta.url).pathname);
const buildDir = resolve(repoRoot, "_bmad-output/unity-webgl-builds/myb-90-unity-mcp-probe");
const outputRoot = resolve(repoRoot, "_bmad-output/video-captures");
const runId = new Date().toISOString().replace(/[:.]/g, "-");
const runDir = join(outputRoot, `myb-90-unity-webgl-readiness-${runId}`);
const videoDir = join(runDir, "raw");
const framesDir = join(runDir, "frames");
const preferredPort = Number(process.env.UNITY_WEBGL_PORT ?? 8090);
const port = await findAvailablePort(preferredPort);
const url = `http://127.0.0.1:${port}/`;
const viewport = { width: 1280, height: 720 };
const chromePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";

mkdirSync(videoDir, { recursive: true });
mkdirSync(framesDir, { recursive: true });

const mimeTypes = new Map([
  [".html", "text/html; charset=utf-8"],
  [".js", "text/javascript; charset=utf-8"],
  [".wasm", "application/wasm"],
  [".data", "application/octet-stream"],
  [".css", "text/css; charset=utf-8"],
  [".png", "image/png"],
  [".ico", "image/x-icon"]
]);

async function findAvailablePort(preferred) {
  for (let candidate = preferred; candidate < preferred + 30; candidate += 1) {
    if (await canUsePort(candidate)) {
      return candidate;
    }
  }

  throw new Error(`No available port from ${preferred} to ${preferred + 29}`);
}

function canUsePort(candidate) {
  return new Promise((resolveCanUse) => {
    const server = createProbeServer();
    server.once("error", () => resolveCanUse(false));
    server.once("listening", () => {
      server.close(() => resolveCanUse(true));
    });
    server.listen(candidate, "127.0.0.1");
  });
}

function createStaticServer(rootDir) {
  return createHttpServer(async (request, response) => {
    try {
      const requestUrl = new URL(request.url ?? "/", url);
      const pathname = decodeURIComponent(requestUrl.pathname);
      const requestedPath = pathname === "/" ? "/index.html" : pathname;
      const filePath = resolve(rootDir, `.${requestedPath}`);
      const safeRoot = `${resolve(rootDir)}${sep}`;
      const safeFile = normalize(filePath);

      if (!safeFile.startsWith(safeRoot) && safeFile !== resolve(rootDir)) {
        response.writeHead(403);
        response.end("Forbidden");
        return;
      }

      const fileStats = await stat(filePath);
      if (!fileStats.isFile()) {
        response.writeHead(404);
        response.end("Not found");
        return;
      }

      response.writeHead(200, {
        "Content-Length": fileStats.size,
        "Content-Type": mimeTypes.get(extname(filePath)) ?? "application/octet-stream",
        "Cross-Origin-Embedder-Policy": "require-corp",
        "Cross-Origin-Opener-Policy": "same-origin"
      });
      createReadStream(filePath).pipe(response);
    } catch {
      response.writeHead(404);
      response.end("Not found");
    }
  });
}

function listen(server, candidatePort) {
  return new Promise((resolveListen) => {
    server.listen(candidatePort, "127.0.0.1", resolveListen);
  });
}

function closeServer(server) {
  return new Promise((resolveClose) => server.close(resolveClose));
}

async function waitForHttp200(targetUrl, timeoutMs = 30_000) {
  const started = Date.now();
  while (Date.now() - started < timeoutMs) {
    try {
      const response = await fetch(targetUrl);
      if (response.ok) {
        return response.status;
      }
    } catch {
    }

    await new Promise((resolveDelay) => setTimeout(resolveDelay, 500));
  }

  throw new Error(`Timed out waiting for ${targetUrl}`);
}

async function waitForCanvasHealth(page, timeoutMs = 120_000) {
  const started = Date.now();
  let lastHealth = null;
  while (Date.now() - started < timeoutMs) {
    const canvas = page.locator("canvas").first();
    if (await canvas.count()) {
      lastHealth = await readCanvasHealth(page);
      if (lastHealth.nonBlank) {
        return {
          health: lastHealth,
          loadMs: Date.now() - started
        };
      }
    }

    await page.waitForTimeout(1000);
  }

  return {
    health: lastHealth,
    loadMs: Date.now() - started
  };
}

async function readCanvasHealth(page) {
  return page.locator("canvas").first().evaluate((canvas) => {
    const rect = canvas.getBoundingClientRect();
    const sampleWidth = 64;
    const sampleHeight = 64;
    const gl = canvas.getContext("webgl2") ?? canvas.getContext("webgl");
    let data;

    if (gl) {
      data = new Uint8Array(sampleWidth * sampleHeight * 4);
      const x = Math.max(0, Math.floor(gl.drawingBufferWidth / 2 - sampleWidth / 2));
      const y = Math.max(0, Math.floor(gl.drawingBufferHeight / 2 - sampleHeight / 2));
      gl.readPixels(x, y, sampleWidth, sampleHeight, gl.RGBA, gl.UNSIGNED_BYTE, data);
    } else {
      const probe = document.createElement("canvas");
      probe.width = sampleWidth;
      probe.height = sampleHeight;
      const context = probe.getContext("2d");
      if (!context) {
        return {
          avgLuma: 0,
          cssHeight: rect.height,
          cssWidth: rect.width,
          height: canvas.height,
          maxLuma: 0,
          minLuma: 0,
          nonBlank: false,
          width: canvas.width
        };
      }

      context.drawImage(canvas, 0, 0, sampleWidth, sampleHeight);
      data = context.getImageData(0, 0, sampleWidth, sampleHeight).data;
    }

    let minLuma = Number.POSITIVE_INFINITY;
    let maxLuma = Number.NEGATIVE_INFINITY;
    let totalLuma = 0;
    let visiblePixels = 0;

    for (let index = 0; index < data.length; index += 4) {
      const alpha = data[index + 3];
      if (alpha === 0) {
        continue;
      }

      const luma = 0.2126 * data[index] + 0.7152 * data[index + 1] + 0.0722 * data[index + 2];
      minLuma = Math.min(minLuma, luma);
      maxLuma = Math.max(maxLuma, luma);
      totalLuma += luma;
      visiblePixels += 1;
    }

    const finiteMin = Number.isFinite(minLuma) ? minLuma : 0;
    const finiteMax = Number.isFinite(maxLuma) ? maxLuma : 0;
    const avgLuma = visiblePixels > 0 ? totalLuma / visiblePixels : 0;

    return {
      avgLuma: Number(avgLuma.toFixed(3)),
      cssHeight: Number(rect.height.toFixed(1)),
      cssWidth: Number(rect.width.toFixed(1)),
      height: canvas.height,
      maxLuma: Number(finiteMax.toFixed(3)),
      minLuma: Number(finiteMin.toFixed(3)),
      nonBlank: visiblePixels > 0 && finiteMax - finiteMin > 5,
      width: canvas.width
    };
  });
}

async function readImageLumaStats(imagePath) {
  const output = await runFfprobe([
    "-v",
    "error",
    "-f",
    "lavfi",
    "-i",
    `movie=${imagePath},signalstats`,
    "-show_entries",
    "frame_tags=lavfi.signalstats.YMIN,lavfi.signalstats.YAVG,lavfi.signalstats.YMAX",
    "-of",
    "json"
  ]);
  const parsed = JSON.parse(output);
  const tags = parsed.frames?.[0]?.tags ?? {};
  const minLuma = Number(tags["lavfi.signalstats.YMIN"] ?? 0);
  const avgLuma = Number(tags["lavfi.signalstats.YAVG"] ?? 0);
  const maxLuma = Number(tags["lavfi.signalstats.YMAX"] ?? 0);
  return {
    avgLuma,
    maxLuma,
    minLuma,
    nonBlank: maxLuma - minLuma > 5
  };
}

async function runFfmpeg(args) {
  return new Promise((resolveRun, rejectRun) => {
    const process = spawn("ffmpeg", args, { stdio: ["ignore", "pipe", "pipe"] });
    let stderr = "";
    process.stderr.on("data", (chunk) => {
      stderr += chunk.toString();
    });
    process.on("close", (code) => {
      if (code === 0) {
        resolveRun();
        return;
      }

      rejectRun(new Error(`ffmpeg failed with code ${code}: ${stderr}`));
    });
  });
}

async function runFfprobe(args) {
  return new Promise((resolveRun, rejectRun) => {
    const process = spawn("ffprobe", args, { stdio: ["ignore", "pipe", "pipe"] });
    let stdout = "";
    let stderr = "";
    process.stdout.on("data", (chunk) => {
      stdout += chunk.toString();
    });
    process.stderr.on("data", (chunk) => {
      stderr += chunk.toString();
    });
    process.on("close", (code) => {
      if (code === 0) {
        resolveRun(stdout);
        return;
      }

      rejectRun(new Error(`ffprobe failed with code ${code}: ${stderr}`));
    });
  });
}

async function videoMetadata(videoPath) {
  const output = await runFfprobe([
    "-v",
    "error",
    "-show_entries",
    "format=duration,size",
    "-show_entries",
    "stream=codec_name,width,height,avg_frame_rate",
    "-of",
    "json",
    videoPath
  ]);
  return JSON.parse(output);
}

async function main() {
  if (!existsSync(join(buildDir, "index.html"))) {
    throw new Error(`Unity WebGL build not found at ${buildDir}`);
  }

  const server = createStaticServer(buildDir);
  await listen(server, port);

  let browser;
  try {
    const httpStatus = await waitForHttp200(url);
    browser = await chromium.launch({
      executablePath: existsSync(chromePath) ? chromePath : undefined,
      headless: true
    });
    const context = await browser.newContext({
      deviceScaleFactor: 1,
      recordVideo: {
        dir: videoDir,
        size: viewport
      },
      viewport
    });
    const page = await context.newPage();
    const consoleMessages = [];
    const consoleCounts = { error: 0, warning: 0 };
    const pageErrors = [];
    const failedRequests = [];
    const responseCounts = { total: 0, failedHttp: 0 };

    page.on("console", (message) => {
      if (["error", "warning"].includes(message.type())) {
        consoleCounts[message.type()] += 1;
        if (consoleMessages.length < 80) {
          consoleMessages.push(`${message.type()}: ${message.text()}`);
        }
      }
    });
    page.on("pageerror", (error) => pageErrors.push(error.message));
    page.on("requestfailed", (request) => {
      failedRequests.push(`${request.method()} ${request.url()} :: ${request.failure()?.errorText ?? "unknown"}`);
    });
    page.on("response", (response) => {
      responseCounts.total += 1;
      if (response.status() >= 400) {
        responseCounts.failedHttp += 1;
      }
    });

    const started = Date.now();
    await page.goto(url, { waitUntil: "domcontentloaded", timeout: 30_000 });
    await page.locator("canvas").first().waitFor({ state: "visible", timeout: 30_000 });
    await page.waitForTimeout(32_000);
    const finalCanvasHealth = await readCanvasHealth(page).catch((error) => ({
      error: error.message,
      nonBlank: false
    }));

    const canvasScreenshotPath = join(runDir, "canvas-frame.png");
    await page.locator("canvas").first().screenshot({ path: canvasScreenshotPath });
    const canvasScreenshotHealth = await readImageLumaStats(canvasScreenshotPath);
    const screenshotPath = join(runDir, "page-frame.png");
    await page.screenshot({ fullPage: true, path: screenshotPath });

    const video = page.video();
    await context.close();
    const rawVideoPath = await video.path();
    await browser.close();
    browser = null;

    const webmPath = join(runDir, "unity-webgl-readiness.webm");
    await rm(webmPath, { force: true });
    await rename(rawVideoPath, webmPath);

    const mp4Path = join(runDir, "unity-webgl-readiness.mp4");
    const contactSheetPath = join(runDir, "unity-webgl-readiness-contact-sheet.jpg");

    await runFfmpeg([
      "-y",
      "-i",
      webmPath,
      "-c:v",
      "libx264",
      "-pix_fmt",
      "yuv420p",
      "-movflags",
      "+faststart",
      mp4Path
    ]);
    await runFfmpeg([
      "-y",
      "-i",
      mp4Path,
      "-vf",
      "fps=1/4,scale=320:-1,tile=5x2",
      "-frames:v",
      "1",
      contactSheetPath
    ]);
    await runFfmpeg([
      "-y",
      "-i",
      mp4Path,
      "-vf",
      "fps=1/4,scale=640:-1",
      join(framesDir, "frame-%02d.jpg")
    ]);

    const frameFiles = await readdir(framesDir);
    const summary = {
      browserElapsedMs: Date.now() - started,
      buildDir,
      canvasNonBlank: canvasScreenshotHealth.nonBlank,
      canvasScreenshotHealth,
      canvasScreenshotPath,
      consoleCounts,
      consoleMessages,
      contactSheetPath,
      failedRequests,
      finalCanvasHealth,
      frameCount: frameFiles.filter((file) => file.endsWith(".jpg")).length,
      httpStatus,
      mp4Path,
      pageErrors,
      responseCounts,
      runDir,
      screenshotPath,
      url,
      videoMetadata: await videoMetadata(mp4Path),
      viewport,
      webmPath
    };
    summary.summaryPath = join(runDir, "capture-summary.json");
    await writeFile(summary.summaryPath, JSON.stringify(summary, null, 2));
    console.log(JSON.stringify({
      canvasNonBlank: summary.canvasNonBlank,
      consoleCounts: summary.consoleCounts,
      failedRequests: summary.failedRequests.length,
      httpStatus: summary.httpStatus,
      mp4Path: summary.mp4Path,
      pageErrors: summary.pageErrors.length,
      runDir: summary.runDir,
      summaryPath: summary.summaryPath
    }, null, 2));
  } finally {
    if (browser) {
      await browser.close();
    }

    await closeServer(server);
  }
}

await main();
