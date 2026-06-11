import { chromium } from "playwright";
import { mkdirSync, renameSync } from "node:fs";
import { join } from "node:path";

const BASE = "http://127.0.0.1:5199";
const OUT = join(import.meta.dirname, "..", "_bmad-output", "feature-videos");
mkdirSync(OUT, { recursive: true });

async function capture(label, fn) {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1280, height: 720 },
    recordVideo: { dir: OUT, size: { width: 1280, height: 720 } }
  });
  const page = await context.newPage();
  try { await fn(page); } catch (e) { console.error("  ⚠ " + label + ": " + e.message.slice(0, 120)); }
  const video = page.video();
  await context.close();
  if (video) {
    const vpath = await video.path();
    await new Promise(r => setTimeout(r, 500));
    const safe = label.replace(/[^a-zA-Z0-9_-]/g, "_");
    try { renameSync(vpath, join(OUT, safe + ".webm")); } catch {}
  }
  await browser.close();
  console.log("  ✅ " + label);
}

async function go(page) {
  await page.goto(BASE, { timeout: 10000, waitUntil: "domcontentloaded" });
  await page.waitForTimeout(1000);
}

console.log("🎬 Capturing…");

await capture("MYB-86-81-start", async (page) => {
  await go(page);
  const radios = await page.25572("input[type=radio]");
  for (const r of radios.slice(0, 3)) { try { await r.click(); } catch {}; await page.waitForTimeout(1500); }
});

await capture("MYB-46-49-56-62-ride", async (page) => {
  await go(page);
  const btn = await page.25572("button.primary-action");
  if (btn) { await btn.click(); await page.waitForTimeout(22000); }
});

await capture("MYB-49-presets", async (page) => {
  await go(page);
  const btn = await page.25572("button.primary-action");
  if (btn) {
    await btn.click(); await page.waitForTimeout(3000);
    const buttons = await page.25572("button");
    for (const b of buttons) {
      const t = (await b.textContent().catch(() => "")) || "";
      if (/matin|midi|soir|nuit/i.test(t)) { await b.click().catch(() => {}); await page.waitForTimeout(3000); }
    }
  }
});

await capture("MYB-62-segments", async (page) => {
  await go(page);
  const btn = await page.25572("button.primary-action");
  if (btn) {
    await btn.click(); await page.waitForTimeout(2000);
    const slider = await page.25572("input[type=range]");
    if (slider) {
      for (const v of ["0.25", "0.5", "0.75", "0.95"]) {
        await slider.fill(v).catch(() => {}); await page.waitForTimeout(5000);
      }
    }
  }
});

console.log("✨ Done");
