import { chromium } from "@playwright/test";
import { spawn } from "node:child_process";
import { existsSync, mkdirSync } from "node:fs";
import { readdir, rename, rm, writeFile } from "node:fs/promises";
import { createServer } from "node:net";
import { join, resolve } from "node:path";

const repoRoot = resolve(new URL("..", import.meta.url).pathname);
const outputRoot = resolve(repoRoot, "_bmad-output/video-captures");
const runId = new Date().toISOString().replace(/[:.]/g, "-");
const runDir = join(outputRoot, `ride-visual-audit-${runId}`);
const videoDir = join(runDir, "raw");
const framesDir = join(runDir, "frames");
const preferredPort = Number(process.env.CAPTURE_PORT ?? 5174);
const capturePort = process.env.CAPTURE_URL
  ? Number(new URL(process.env.CAPTURE_URL).port || preferredPort)
  : await findAvailablePort(preferredPort);
const captureHud = process.env.CAPTURE_HIDE_HUD === "0" ? "visible" : "hidden";
const url = withCaptureHudParam(
  process.env.CAPTURE_URL ?? `http://127.0.0.1:${capturePort}/`,
  captureHud
);
const captureMs = Number(process.env.CAPTURE_MS ?? 60_000);
const captureSeconds = Math.max(1, Math.round(captureMs / 1000));
const viewport = { width: 1440, height: 900 };
const chromePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";

mkdirSync(videoDir, { recursive: true });
mkdirSync(framesDir, { recursive: true });

function withCaptureHudParam(targetUrl, mode) {
  if (mode !== "hidden") {
    return targetUrl;
  }

  const nextUrl = new URL(targetUrl);
  nextUrl.searchParams.set("captureHud", "hidden");

  return nextUrl.toString();
}

async function findAvailablePort(preferred) {
  for (let port = preferred; port < preferred + 20; port += 1) {
    if (await canUsePort(port)) {
      return port;
    }
  }

  throw new Error(`No available capture port from ${preferred} to ${preferred + 19}`);
}

function canUsePort(port) {
  return new Promise((resolveCanUse) => {
    const probe = createServer();

    probe.once("error", () => resolveCanUse(false));
    probe.once("listening", () => {
      probe.close(() => resolveCanUse(true));
    });
    probe.listen(port, "127.0.0.1");
  });
}

const server = spawn(
  "npm",
  [
    "run",
    "dev",
    "--",
    "--host",
    "127.0.0.1",
    "--port",
    String(capturePort),
    "--strictPort"
  ],
  {
    cwd: repoRoot,
    stdio: ["ignore", "pipe", "pipe"]
  }
);

const serverLogs = [];
server.stdout.on("data", (chunk) => serverLogs.push(chunk.toString()));
server.stderr.on("data", (chunk) => serverLogs.push(chunk.toString()));

async function waitForHttp200(targetUrl, timeoutMs = 30_000) {
  const started = Date.now();

  while (Date.now() - started < timeoutMs) {
    try {
      const response = await fetch(targetUrl);
      if (response.ok) {
        return response.status;
      }
    } catch {
      // Vite is still booting.
    }

    await new Promise((resolveDelay) => setTimeout(resolveDelay, 500));
  }

  throw new Error(`Timed out waiting for ${targetUrl}`);
}

async function setSlider(page, value) {
  const slider = page.locator('input[type="range"]').first();

  await slider.evaluate(
    (element, nextValue) => {
      element.value = String(nextValue);
      element.dispatchEvent(new Event("input", { bubbles: true }));
      element.dispatchEvent(new Event("change", { bubbles: true }));
    },
    String(value)
  );
}

async function readCanvasHealth(page) {
  return page.locator("canvas").evaluate((canvas) => {
    const sampleWidth = 64;
    const sampleHeight = 64;
    let data;
    const gl = canvas.getContext("webgl2") ?? canvas.getContext("webgl");

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

    const avgLuma = visiblePixels > 0 ? totalLuma / visiblePixels : 0;
    const finiteMin = Number.isFinite(minLuma) ? minLuma : 0;
    const finiteMax = Number.isFinite(maxLuma) ? maxLuma : 0;

    return {
      avgLuma: Number(avgLuma.toFixed(3)),
      height: canvas.height,
      maxLuma: Number(finiteMax.toFixed(3)),
      minLuma: Number(finiteMin.toFixed(3)),
      nonBlank: visiblePixels > 0 && finiteMax - finiteMin > 5,
      width: canvas.width
    };
  });
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

async function main() {
  const httpStatus = await waitForHttp200(url);
  const browser = await chromium.launch({
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
  const pageErrors = [];

  page.on("console", (message) => {
    if (["error", "warning"].includes(message.type())) {
      consoleMessages.push(`${message.type()}: ${message.text()}`);
    }
  });
  page.on("pageerror", (error) => pageErrors.push(error.message));

  await page.goto(url, { waitUntil: "networkidle" });
  await page.getByRole("button", { name: /lancer la balade mock/i }).click();
  await page.locator("canvas").waitFor({ state: "visible", timeout: 10_000 });

  await setSlider(page, 45);
  await page.waitForTimeout(7_500);
  await setSlider(page, 72);
  await page.waitForTimeout(7_500);
  await setSlider(page, 92);
  await page.waitForTimeout(Math.max(0, captureMs - 15_000));

  const framebufferHealth = await readCanvasHealth(page);
  const canvasScreenshotPath = join(runDir, "canvas-frame.png");
  await page.locator("canvas").screenshot({ path: canvasScreenshotPath });
  const screenshotPath = join(runDir, "final-frame.png");
  await page.screenshot({ fullPage: true, path: screenshotPath });

  const video = page.video();
  await context.close();
  await browser.close();

  const rawVideoPath = await video.path();
  const webmPath = join(runDir, `ride-visual-audit-${captureSeconds}s.webm`);
  await rm(webmPath, { force: true });
  await rename(rawVideoPath, webmPath);

  const mp4Path = join(runDir, `ride-visual-audit-${captureSeconds}s.mp4`);
  const contactSheetPath = join(runDir, "ride-visual-audit-contact-sheet.jpg");
  const canvasHealth = await readImageLumaStats(canvasScreenshotPath);

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
    "fps=1/5,scale=320:-1,tile=6x2",
    "-frames:v",
    "1",
    contactSheetPath
  ]);
  await runFfmpeg([
    "-y",
    "-i",
    mp4Path,
    "-vf",
    "fps=1/5,scale=640:-1",
    join(framesDir, "frame-%02d.jpg")
  ]);

  const frameFiles = await readdir(framesDir);
  const summary = {
    consoleMessages,
    contactSheetPath,
    durationMs: captureMs,
    frameCount: frameFiles.filter((file) => file.endsWith(".jpg")).length,
    httpStatus,
    mp4Path,
    pageErrors,
    canvasHealth,
    canvasScreenshotPath,
    framebufferHealth,
    canvasNonBlank: canvasHealth.nonBlank,
    captureHud,
    capturePort,
    runDir,
    screenshotPath,
    serverLogs: serverLogs.join(""),
    url,
    viewport,
    webmPath
  };

  await writeFile(join(runDir, "capture-summary.json"), JSON.stringify(summary, null, 2));
  console.log(JSON.stringify(summary, null, 2));
}

try {
  await main();
} finally {
  server.kill("SIGTERM");
}
