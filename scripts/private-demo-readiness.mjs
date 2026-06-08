import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from "node:fs";
import { readdir } from "node:fs/promises";
import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

export const DEFAULT_BASELINE_COMMIT = "0611461c3a230394abb976e17681105fac99289f";
export const DEFAULT_REPORT_PATH = "_bmad-output/web-test-results/myb-19-private-demo-readiness.txt";

const repoRoot = resolve(new URL("..", import.meta.url).pathname);

export function evaluateReadiness({ captureSummary, commandResults }) {
  const commandChecks = commandResults.map((result) => ({
    label: result.label,
    pass: result.status === 0
  }));
  const captureChecks = [
    {
      label: "HTTP 200",
      pass: captureSummary?.httpStatus === 200
    },
    {
      label: "canvas nonblank",
      pass: captureSummary?.canvasNonBlank === true
    },
    {
      label: "no page errors",
      pass: Array.isArray(captureSummary?.pageErrors) && captureSummary.pageErrors.length === 0
    },
    {
      label: "no console errors",
      pass: Array.isArray(captureSummary?.consoleMessages) && captureSummary.consoleMessages.length === 0
    },
    {
      label: "60s capture",
      pass: Number(captureSummary?.durationMs) >= 60_000
    },
    {
      label: "video artifact",
      pass: Boolean(captureSummary?.mp4Path) && existsSync(captureSummary.mp4Path)
    },
    {
      label: "contact sheet",
      pass: Boolean(captureSummary?.contactSheetPath) && existsSync(captureSummary.contactSheetPath)
    },
    {
      label: "capture summary",
      pass: Boolean(captureSummary?.summaryPath) && existsSync(captureSummary.summaryPath)
    }
  ];

  const checks = [...commandChecks, ...captureChecks];
  const ready = checks.every((check) => check.pass);

  return {
    checks,
    ready,
    verdict: ready ? "ready-for-private-web-demo" : "not-ready-for-private-web-demo"
  };
}

export function formatReadinessReport({
  baselineCommit = DEFAULT_BASELINE_COMMIT,
  captureSummary,
  commandResults,
  currentCommit,
  generatedAt,
  projectRoot,
  readiness,
  reportPath
}) {
  const targetUrl = captureSummary?.url ?? "unknown";
  const demoUrl = visibleDemoUrl(targetUrl);
  const capturePort = captureSummary?.capturePort ?? 5174;
  const launchCommand = `npm run dev -- --host 127.0.0.1 --port ${capturePort} --strictPort`;
  const relativeReportPath = relative(projectRoot, reportPath);
  const lines = [
    "MYB-19 Private Web Demo Readiness",
    "",
    `Generated: ${generatedAt}`,
    `Project root: ${projectRoot}`,
    `Baseline commit: ${baselineCommit}`,
    `Current commit: ${currentCommit}`,
    `Target URL: ${targetUrl}`,
    "Build mode: local Vite development server with production build validation",
    `Local launch command: ${launchCommand}`,
    `Report path: ${relativeReportPath}`,
    "",
    "Checks"
  ];

  for (const result of commandResults) {
    lines.push(`- ${result.label}: ${result.status === 0 ? "pass" : "fail"}`);
  }

  for (const check of readiness.checks.filter((item) => !commandResults.some((result) => result.label === item.label))) {
    lines.push(`- ${check.label}: ${check.pass ? "pass" : "fail"}`);
  }

  lines.push(
    "",
    "Capture Evidence",
    `- directory: ${captureSummary?.runDir ?? "missing"}`,
    `- video: ${captureSummary?.mp4Path ?? "missing"}`,
    `- webm: ${captureSummary?.webmPath ?? "missing"}`,
    `- contact sheet: ${captureSummary?.contactSheetPath ?? "missing"}`,
    `- final frame: ${captureSummary?.screenshotPath ?? "missing"}`,
    `- summary: ${captureSummary?.summaryPath ?? "missing"}`,
    `- durationMs: ${captureSummary?.durationMs ?? "missing"}`,
    `- frameCount: ${captureSummary?.frameCount ?? "missing"}`,
    `- canvasNonBlank: ${captureSummary?.canvasNonBlank === true ? "true" : "false"}`,
    `- canvasHealth: ${JSON.stringify(captureSummary?.canvasHealth ?? null)}`,
    `- pageErrors: ${JSON.stringify(captureSummary?.pageErrors ?? [])}`,
    `- consoleMessages: ${JSON.stringify(captureSummary?.consoleMessages ?? [])}`,
    "",
    "Launch Checklist",
    "1. From the repo root, run `npm install` if dependencies are not already installed.",
    `2. Run \`${launchCommand}\` for the local demo server.`,
    `3. Open \`${demoUrl}\` in the browser.`,
    "4. Click `Lancer la balade mock`.",
    "5. Adjust the mock effort slider and confirm speed, distance and progress update.",
    "6. Use pause/resume, then finish the ride and confirm the summary appears.",
    "7. Keep this report with the latest 60s capture as the private-demo proof.",
    "",
    "If Not Ready",
    "- Do not show the demo as ready; fix the failing check named above and regenerate this report.",
    "",
    `Verdict: ${readiness.verdict}`
  );

  return `${lines.join("\n")}\n`;
}

function visibleDemoUrl(targetUrl) {
  try {
    const parsedUrl = new URL(targetUrl);
    parsedUrl.searchParams.delete("captureHud");
    return parsedUrl.toString();
  } catch {
    return targetUrl;
  }
}

function runCommand(label, command, args) {
  const startedAt = Date.now();
  console.log(`\n[MYB-19] ${label}`);
  const result = spawnSync(command, args, {
    cwd: repoRoot,
    stdio: "inherit"
  });

  return {
    durationMs: Date.now() - startedAt,
    label,
    status: result.status ?? 1
  };
}

function currentCommit() {
  const result = spawnSync("git", ["rev-parse", "HEAD"], {
    cwd: repoRoot,
    encoding: "utf8"
  });

  return result.status === 0 ? result.stdout.trim() : "unknown";
}

async function latestCaptureSummary() {
  const captureRoot = join(repoRoot, "_bmad-output/video-captures");
  const entries = await readdir(captureRoot, { withFileTypes: true });
  const summaries = entries
    .filter((entry) => entry.isDirectory() && entry.name.startsWith("ride-visual-audit-"))
    .map((entry) => join(captureRoot, entry.name, "capture-summary.json"))
    .filter((path) => existsSync(path))
    .sort((left, right) => statSync(right).mtimeMs - statSync(left).mtimeMs);

  if (summaries.length === 0) {
    throw new Error("No capture-summary.json found under _bmad-output/video-captures");
  }

  const summaryPath = summaries[0];
  return {
    ...JSON.parse(readFileSync(summaryPath, "utf8")),
    summaryPath
  };
}

async function main() {
  const commandResults = [
    runCommand("npm run typecheck", "npm", ["run", "typecheck"]),
    runCommand("npm run test", "npm", ["run", "test"]),
    runCommand("npm run build", "npm", ["run", "build"]),
    runCommand("npm run capture:ride-video", "npm", ["run", "capture:ride-video"])
  ];
  const captureSummary = await latestCaptureSummary();
  const reportPath = join(repoRoot, DEFAULT_REPORT_PATH);
  const readiness = evaluateReadiness({ captureSummary, commandResults });
  const report = formatReadinessReport({
    captureSummary,
    commandResults,
    currentCommit: currentCommit(),
    generatedAt: new Date().toISOString(),
    projectRoot: repoRoot,
    readiness,
    reportPath
  });

  mkdirSync(dirname(reportPath), { recursive: true });
  writeFileSync(reportPath, report);
  console.log(`\n[MYB-19] readiness report: ${DEFAULT_REPORT_PATH}`);
  console.log(`[MYB-19] verdict: ${readiness.verdict}`);

  if (!readiness.ready) {
    process.exitCode = 1;
  }
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  await main();
}
