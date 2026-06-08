import { describe, expect, it } from "vitest";
import { evaluateReadiness, formatReadinessReport } from "./private-demo-readiness.mjs";

const passingCommands = [
  { label: "npm run typecheck", status: 0 },
  { label: "npm run test", status: 0 },
  { label: "npm run build", status: 0 },
  { label: "npm run capture:ride-video", status: 0 }
];

const passingSummary = {
  canvasHealth: {
    avgLuma: 120,
    height: 900,
    maxLuma: 230,
    minLuma: 20,
    nonBlank: true,
    width: 1440
  },
  canvasNonBlank: true,
  consoleMessages: [],
  contactSheetPath: new URL("private-demo-readiness.test.mjs", import.meta.url).pathname,
  durationMs: 60_000,
  frameCount: 12,
  httpStatus: 200,
  mp4Path: new URL("private-demo-readiness.test.mjs", import.meta.url).pathname,
  pageErrors: [],
  runDir: "/tmp/capture",
  screenshotPath: "/tmp/capture/final-frame.png",
  summaryPath: new URL("private-demo-readiness.test.mjs", import.meta.url).pathname,
  url: "http://127.0.0.1:5174/?captureHud=hidden",
  webmPath: "/tmp/capture/video.webm"
};

describe("private demo readiness", () => {
  it("returns the ready verdict only when all command and browser checks pass", () => {
    const readiness = evaluateReadiness({
      captureSummary: passingSummary,
      commandResults: passingCommands
    });

    expect(readiness.ready).toBe(true);
    expect(readiness.verdict).toBe("ready-for-private-web-demo");
  });

  it("keeps the report non-ready when the canvas is blank", () => {
    const readiness = evaluateReadiness({
      captureSummary: { ...passingSummary, canvasNonBlank: false },
      commandResults: passingCommands
    });

    expect(readiness.ready).toBe(false);
    expect(readiness.verdict).toBe("not-ready-for-private-web-demo");
  });

  it("formats a launch checklist and capture evidence", () => {
    const readiness = evaluateReadiness({
      captureSummary: passingSummary,
      commandResults: passingCommands
    });
    const report = formatReadinessReport({
      captureSummary: passingSummary,
      commandResults: passingCommands,
      currentCommit: "abc123",
      generatedAt: "2026-06-08T00:00:00.000Z",
      projectRoot: "/repo",
      readiness,
      reportPath: "/repo/_bmad-output/web-test-results/myb-19-private-demo-readiness.txt"
    });

    expect(report).toContain("Launch Checklist");
    expect(report).toContain("Verdict: ready-for-private-web-demo");
    expect(report).toContain("canvasNonBlank: true");
  });
});
