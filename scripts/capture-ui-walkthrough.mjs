import { chromium } from "@playwright/test";
import { spawn } from "node:child_process";
import { existsSync, mkdirSync } from "node:fs";
import { rename, rm, writeFile } from "node:fs/promises";
import { createServer } from "node:net";
import { join, resolve } from "node:path";

const repoRoot = resolve(new URL("..", import.meta.url).pathname);
const outputRoot = resolve(repoRoot, "_bmad-output/video-captures");
const runId = new Date().toISOString().replace(/[:.]/g, "-");
const runDir = join(outputRoot, `ui-walkthrough-${runId}`);
const videoDir = join(runDir, "raw");
const preferredPort = 5180;
const capturePort = await findAvailablePort(preferredPort);
const url = `http://127.0.0.1:${capturePort}/`;
const viewport = { width: 1440, height: 900 };
const chromePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome";

mkdirSync(videoDir, { recursive: true });

async function findAvailablePort(preferred) {
  for (let port = preferred; port < preferred + 20; port += 1) {
    if (await canUsePort(port)) return port;
  }
  throw new Error(`No available port from ${preferred}`);
}

function canUsePort(port) {
  return new Promise((resolveCanUse) => {
    const probe = createServer();
    probe.once("error", () => resolveCanUse(false));
    probe.once("listening", () => { probe.close(() => resolveCanUse(true)); });
    probe.listen(port, "127.0.0.1");
  });
}

const server = spawn("npm", ["run","dev","--","--host","127.0.0.1","--port",String(capturePort),"--strictPort"], { cwd: repoRoot, stdio: ["ignore", "pipe", "pipe"] });

let serverOutput = "";
server.stdout.on("data", (chunk) => { serverOutput += chunk; });
server.stderr.on("data", (chunk) => { serverOutput += chunk; });

await new Promise((resolveStart) => {
  const check = () => {
    if (serverOutput.includes("Local:")) resolveStart();
    else setTimeout(check, 200);
  };
  setTimeout(check, 200);
});

const browser = await chromium.launch({ headless: true, executablePath: chromePath });
const context = await browser.newContext({ viewport, recordVideo: { dir: videoDir, size: viewport } });
const page = await context.newPage();

console.log("🎬 UI walkthrough...");

// 1. StartScreen - multi-route + microcopy
await page.goto(url, { waitUntil: "domcontentloaded" });
await page.waitForTimeout(2000);

// Click through routes to show the selector
const radios = await page.$$("input[type=radio]");
for (const r of radios.slice(0, 3)) {
  try { await r.click(); await page.waitForTimeout(1500); } catch {}
}

// 2. Start ride
const startBtn = await page.$("button.primary-action");
if (startBtn) {
  await startBtn.click();
  await page.waitForTimeout(5000);

  // Move slider to progress
  const slider = await page.$("input[type=range]");
  if (slider) {
    for (const v of ["0.3","0.6","0.9"]) {
      try { await slider.fill(v); await page.waitForTimeout(3000); } catch {}
    }
  }

  // Click terminate
  const finishBtn = await page.$("button");
  // Find the "Terminer" button
  const buttons = await page.$$("button");
  for (const b of buttons) {
    const t = await b.textContent().catch(() => "");
    if (t && /termin/i.test(t)) { await b.click(); break; }
  }
  await page.waitForTimeout(4000);
}

// 3. Summary screen visible
await page.waitForTimeout(2000);

const video = page.video();
await context.close();
await browser.close();
server.kill();

if (video) {
  const vpath = await video.path();
  const dest = join(runDir, "ui-walkthrough.mp4");
  await rename(vpath, dest);
  console.log(`✅ ${dest}`);
}

const summary = { url, capturePort, runDir };
await writeFile(join(runDir, "capture-summary.json"), JSON.stringify(summary, null, 2));
console.log(JSON.stringify(summary));
