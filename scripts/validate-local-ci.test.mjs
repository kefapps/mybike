import { describe, expect, it } from "vitest";

import { evaluateLocalCi, formatLocalCiReport, parseArgs } from "./validate-local-ci.mjs";

describe("validate-local-ci", () => {
  it("fails when a required check fails or is skipped", () => {
    const verdict = evaluateLocalCi([
      { label: "repo hygiene", status: "pass" },
      { label: "unity status", status: "skip" },
      { label: "optional proof", required: false, status: "skip" }
    ]);

    expect(verdict).toMatchObject({
      failedCount: 0,
      passed: false,
      skippedRequiredCount: 1,
      verdict: "fail"
    });
  });

  it("passes when required checks pass and optional checks are skipped", () => {
    const verdict = evaluateLocalCi([
      { label: "repo hygiene", status: "pass" },
      { label: "unity status", status: "pass" },
      { label: "optional proof", required: false, status: "skip" }
    ]);

    expect(verdict).toMatchObject({
      failedCount: 0,
      passed: true,
      skippedRequiredCount: 0,
      verdict: "pass"
    });
  });

  it("formats the reusable PR checklist snippet", () => {
    const report = formatLocalCiReport({
      checks: [{ label: "repo hygiene", status: "pass", summary: "ok" }],
      generatedAt: "2026-06-12T09:40:00.000Z",
      projectRoot: "/repo",
      reportPath: "/repo/_bmad-output/unity-test-results/myb-83-local-ci.txt",
      verdict: { verdict: "pass" }
    });

    expect(report).toContain("MYB-83 Local CI Validation");
    expect(report).toContain("Local CI: PASS");
    expect(report).toContain("no GitHub Actions or hosted runner");
  });

  it("parses report and skip-unity arguments", () => {
    expect(parseArgs(["--skip-unity", "--report", "tmp/report.txt"])).toEqual({
      reportPath: "tmp/report.txt",
      skipUnity: true
    });
  });
});
