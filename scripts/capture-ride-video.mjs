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
const captureMs = Number(process.env.CAPTURE_MS ?? 30_000);
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

  const screenshotPath = join(runDir, "final-frame.png");
  await page.screenshot({ fullPage: true, path: screenshotPath });

  const video = page.video();
  await context.close();
  await browser.close();

  const rawVideoPath = await video.path();
  const webmPath = join(runDir, "ride-visual-audit-30s.webm");
  await rm(webmPath, { force: true });
  await rename(rawVideoPath, webmPath);

  const mp4Path = join(runDir, "ride-visual-audit-30s.mp4");
  const contactSheetPath = join(runDir, "ride-visual-audit-contact-sheet.jpg");

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
    "fps=1/5,scale=320:-1,tile=6x1",
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
