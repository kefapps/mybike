import { spawnSync } from "node:child_process";
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, relative, resolve } from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

export const DEFAULT_REPORT_PATH = "_bmad-output/unity-test-results/myb-83-local-ci.txt";
export const UNITY_PROJECT_PATH = "unity/Echapee4D";

const repoRoot = resolve(fileURLToPath(new URL("..", import.meta.url)));
const DEFAULT_COMMAND_TIMEOUT_MS = 120_000;

const REQUIRED_PATHS = [
  "unity/Echapee4D/Assets",
  "unity/Echapee4D/Packages/manifest.json",
  "unity/Echapee4D/Packages/packages-lock.json",
  "unity/Echapee4D/ProjectSettings/ProjectVersion.txt",
  "unity/Echapee4D/Assets/MYB91/Editor/MYB91CanonicalBaselineValidator.cs"
];

const LEGACY_FORBIDDEN_PATHS = ["unity/Echappee3D"];

const GENERATED_UNITY_PATHS = [
  "unity/Echapee4D/Library",
  "unity/Echapee4D/Temp",
  "unity/Echapee4D/Logs",
  "unity/Echapee4D/UserSettings",
  "unity/Echapee4D/Obj",
  "unity/Echapee4D/Build",
  "unity/Echapee4D/Builds",
  "unity/Echapee4D/MemoryCaptures",
  "unity/Echapee4D/Recordings"
];

const CRITICAL_UNITY_STATUS_PATHS = [
  "unity/Echapee4D/Packages",
  "unity/Echapee4D/ProjectSettings"
];

const GENERATED_STATUS_PATHS = GENERATED_UNITY_PATHS;

export function parseArgs(argv = process.argv.slice(2)) {
  const options = {
    base: "main",
    reportPath: DEFAULT_REPORT_PATH,
    skipUnity: false
  };

  for (let index = 0; index < argv.length; index += 1) {
    const arg = argv[index];
    if (arg === "--skip-unity") {
      options.skipUnity = true;
    } else if (arg === "--base") {
      const next = argv[index + 1];
      if (!next || next.startsWith("-")) {
        throw new Error("--base requires a git revision");
      }
      options.base = next;
      index += 1;
    } else if (arg === "--report") {
      const next = argv[index + 1];
      if (!next || next.startsWith("-")) {
        throw new Error("--report requires a path");
      }
      options.reportPath = next;
      index += 1;
    } else if (arg === "--help" || arg === "-h") {
      options.help = true;
    } else {
      throw new Error(`Unknown argument: ${arg}`);
    }
  }

  return options;
}

export function evaluateLocalCi(checks) {
  const required = checks.filter((check) => check.required !== false);
  const failed = required.filter((check) => check.status === "fail");
  const skipped = required.filter((check) => check.status === "skip");

  return {
    passed: failed.length === 0 && skipped.length === 0,
    failedCount: failed.length,
    skippedRequiredCount: skipped.length,
    totalChecks: checks.length,
    verdict: failed.length === 0 && skipped.length === 0 ? "pass" : "fail"
  };
}

export function formatLocalCiReport({ checks, generatedAt, projectRoot, reportPath, verdict }) {
  const relativeReportPath = relative(projectRoot, reportPath);
  const lines = [
    "MYB-83 Local CI Validation",
    "",
    `Generated: ${generatedAt}`,
    `Project root: ${projectRoot}`,
    `Unity project: ${UNITY_PROJECT_PATH}`,
    `Command: npm run validate:local-ci`,
    `Report path: ${relativeReportPath}`,
    `Verdict: ${verdict.verdict}`,
    "",
    "Checks"
  ];

  for (const check of checks) {
    const required = check.required === false ? "optional" : "required";
    lines.push(`- ${check.status.toUpperCase()} [${required}] ${check.label}`);
    if (check.summary) {
      lines.push(`  ${check.summary}`);
    }
    if (check.command) {
      lines.push(`  command: ${check.command}`);
    }
  }

  lines.push(
    "",
    "PR Checklist Snippet",
    `- Local CI: ${verdict.verdict === "pass" ? "PASS" : "FAIL"} (${relativeReportPath})`,
    "- Scope: local Unity macOS-first validation only; no GitHub Actions or hosted runner.",
    "- WebGL: not run; secondary proof only when a ticket explicitly asks for it.",
    "",
    "Local Limits",
    "- This command is intentionally local and does not create `.github/workflows`.",
    "- Unity checks require the local Unity Editor and Unity-MCP server.",
    "- Full Unity builds remain out of scope for MYB-83 unless a later ticket asks for them."
  );

  if (verdict.verdict !== "pass") {
    lines.push("Failure Output");
    for (const check of checks.filter((item) => item.status === "fail" || item.status === "skip")) {
      lines.push(`- ${check.label}: ${check.summary ?? check.status}`);
      const output = trimOutput(check.output);
      if (output) {
        lines.push("  output:");
        for (const line of output.split("\n")) {
          lines.push(`  ${line}`);
        }
      }
    }
  }

  return `${lines.join("\n")}\n`;
}

function trimOutput(output, maxLines = 12) {
  if (!output) {
    return "";
  }
  const lines = output.trim().split(/\r?\n/).filter(Boolean);
  return lines.slice(-maxLines).join("\n");
}

function passCheck(label, summary) {
  return { label, status: "pass", summary };
}

function failCheck(label, summary, output = "") {
  return { label, status: "fail", summary, output };
}

function skipCheck(label, summary) {
  return { label, status: "skip", summary };
}

function runCommand(
  label,
  command,
  args,
  { required = true, timeoutMs = DEFAULT_COMMAND_TIMEOUT_MS, timeoutSummary } = {}
) {
  const startedAt = Date.now();
  const result = spawnSync(command, args, {
    cwd: repoRoot,
    encoding: "utf8",
    maxBuffer: 10 * 1024 * 1024,
    timeout: timeoutMs
  });
  const durationMs = Date.now() - startedAt;
  const output = [result.stdout, result.stderr].filter(Boolean).join("\n");
  const displayCommand = [command, ...args].join(" ");

  if (result.error) {
    return {
      command: displayCommand,
      durationMs,
      label,
      output,
      required,
      status: "fail",
      summary:
        result.error.code === "ETIMEDOUT"
          ? timeoutSummary ?? `timed out after ${timeoutMs}ms`
          : result.error.message
    };
  }

  if (result.status === 0) {
    return {
      command: displayCommand,
      durationMs,
      label,
      output,
      required,
      status: "pass",
      summary: `completed in ${durationMs}ms`
    };
  }

  return {
    command: displayCommand,
    durationMs,
    label,
    output,
    required,
    status: "fail",
    summary: timeoutSummary ?? `exit ${result.status ?? 1}`
  };
}

function checkRequiredPaths() {
  return REQUIRED_PATHS.map((path) => {
    const absolutePath = join(repoRoot, path);
    return existsSync(absolutePath)
      ? passCheck(`required path exists: ${path}`)
      : failCheck(`required path exists: ${path}`, "missing");
  });
}

function checkLegacyForbiddenPaths() {
  return LEGACY_FORBIDDEN_PATHS.map((path) => {
    const absolutePath = join(repoRoot, path);
    return existsSync(absolutePath)
      ? failCheck(`legacy path absent: ${path}`, "path exists")
      : passCheck(`legacy path absent: ${path}`);
  });
}

function checkJsonFile(path) {
  const absolutePath = join(repoRoot, path);
  try {
    JSON.parse(readFileSync(absolutePath, "utf8"));
    return passCheck(`JSON parses: ${path}`);
  } catch (error) {
    return failCheck(`JSON parses: ${path}`, error.message);
  }
}

function runBranchWhitespaceCheck(base) {
  const verifyBase = runCommand("branch diff base resolves", "git", ["rev-parse", "--verify", base]);
  if (verifyBase.status !== "pass") {
    return {
      ...verifyBase,
      label: `branch whitespace check vs ${base}`,
      summary: `base revision does not resolve: ${base}`
    };
  }

  const mergeBase = runCommand("branch diff merge-base resolves", "git", ["merge-base", "HEAD", base]);
  if (mergeBase.status !== "pass") {
    return {
      ...mergeBase,
      label: `branch whitespace check vs ${base}`,
      summary: `could not resolve merge-base with ${base}`
    };
  }

  const mergeBaseSha = mergeBase.output.trim();
  return runCommand(`branch/worktree whitespace check vs ${base}`, "git", ["diff", "--check", mergeBaseSha]);
}

function runGitStatusCheck(label, paths) {
  const result = runCommand(label, "git", ["status", "--porcelain", "--", ...paths]);
  if (result.status !== "pass") {
    return result;
  }

  const output = result.output.trim();
  return output.length === 0
    ? { ...result, summary: "no tracked or untracked drift" }
    : {
        ...result,
        status: "fail",
        summary: "unexpected dirty paths",
        output
      };
}

function runGitTrackedGeneratedFoldersCheck() {
  const result = runCommand("Unity generated folders tracked-file check", "git", [
    "ls-files",
    "--",
    ...GENERATED_UNITY_PATHS
  ]);
  if (result.status !== "pass") {
    return result;
  }

  const output = result.output.trim();
  return output.length === 0
    ? { ...result, summary: "no generated Unity folders are tracked" }
    : {
        ...result,
        status: "fail",
        summary: "generated Unity folders contain tracked files",
        output
      };
}

function unityBaselineInput() {
  return JSON.stringify({
    className: "MYB83RunBaselineValidator",
    methodName: "Main",
    csharpCode:
      "using MYB91.Editor; public static class MYB83RunBaselineValidator { public static void Main() { MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli(); } }"
  });
}

function unityMyb59Input() {
  return JSON.stringify({
    className: "MYB83RunMyb59Validator",
    methodName: "Main",
    csharpCode:
      "using MYB59.Editor; public static class MYB83RunMyb59Validator { public static void Main() { MYB59ResistanceControllerValidator.ValidateResistanceControllerCli(); } }"
  });
}

function unityMyb60Input() {
  return JSON.stringify({
    className: "MYB83RunMyb60Validator",
    methodName: "Main",
    csharpCode:
      "using MYB60.Editor; public static class MYB83RunMyb60Validator { public static void Main() { MYB60ResistanceMapperValidator.ValidateResistanceMapperCli(); } }"
  });
}

function unityMyb64Input() {
  return JSON.stringify({
    className: "MYB83RunMyb64Validator",
    methodName: "Main",
    csharpCode:
      "using MYB64.Editor; public static class MYB83RunMyb64Validator { public static void Main() { MYB64NoTrainerFallbackValidator.ValidateNoTrainerFallbackCli(); } }"
  });
}

function unityMyb73Input() {
  return JSON.stringify({
    className: "MYB83RunMyb73Validator",
    methodName: "Main",
    csharpCode:
      "using MYB73.Editor; public static class MYB83RunMyb73Validator { public static void Main() { MYB73RoutePreviewValidator.ValidateRoutePreviewCli(); } }"
  });
}

function unityMyb79Input() {
  return JSON.stringify({
    className: "MYB83RunMyb79Validator",
    methodName: "Main",
    csharpCode:
      "using MYB79.Editor; public static class MYB83RunMyb79Validator { public static void Main() { MYB79WelcomeScreenValidator.ValidateWelcomeScreenCli(); } }"
  });
}

function unityMyb98Input() {
  return JSON.stringify({
    className: "MYB83RunMyb98Validator",
    methodName: "Main",
    csharpCode:
      "using MYB98.Editor; public static class MYB83RunMyb98Validator { public static void Main() { MYB98RideTrajectoryValidator.ValidateRideTrajectoryCli(); } }"
  });
}

async function runLocalCi(options) {
  const checks = [
    ...checkRequiredPaths(),
    ...checkLegacyForbiddenPaths(),
    checkJsonFile("unity/Echapee4D/Packages/manifest.json"),
    checkJsonFile("unity/Echapee4D/Packages/packages-lock.json"),
    runCommand("working tree whitespace check", "git", ["diff", "--check"]),
    runCommand("staged whitespace check", "git", ["diff", "--cached", "--check"]),
    runBranchWhitespaceCheck(options.base),
    runGitStatusCheck("Unity Packages/ProjectSettings drift check", CRITICAL_UNITY_STATUS_PATHS),
    runGitStatusCheck("Unity generated folders untracked drift check", GENERATED_STATUS_PATHS),
    runGitTrackedGeneratedFoldersCheck()
  ];

  if (options.skipUnity) {
    checks.push(
      skipCheck("Unity-MCP status", "--skip-unity was provided"),
      skipCheck("MYB-91 canonical baseline validator", "--skip-unity was provided"),
      skipCheck("MYB-59 resistance controller validator", "--skip-unity was provided"),
      skipCheck("MYB-60 resistance mapper validator", "--skip-unity was provided"),
      skipCheck("MYB-64 no-trainer fallback validator", "--skip-unity was provided"),
      skipCheck("MYB-73 route preview validator", "--skip-unity was provided"),
      skipCheck("MYB-79 welcome screen validator", "--skip-unity was provided"),
      skipCheck("MYB-98 ride trajectory unit validator", "--skip-unity was provided")
    );
  } else {
    checks.push(
      runCommand("Unity-MCP status", "unity-mcp-cli", ["status", UNITY_PROJECT_PATH, "--timeout", "10000"], {
        timeoutMs: 30_000
      }),
      runCommand("MYB-91 canonical baseline validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityBaselineInput(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-59 resistance controller validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb59Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-60 resistance mapper validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb60Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-64 no-trainer fallback validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb64Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-73 route preview validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb73Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-79 welcome screen validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb79Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runCommand("MYB-98 ride trajectory unit validator", "unity-mcp-cli", [
        "run-tool",
        "script-execute",
        UNITY_PROJECT_PATH,
        "--input",
        unityMyb98Input(),
        "--timeout",
        "180000"
      ], {
        timeoutMs: 240_000
      }),
      runGitStatusCheck("post-Unity Packages/ProjectSettings drift check", CRITICAL_UNITY_STATUS_PATHS),
      runGitStatusCheck("post-Unity generated folders untracked drift check", GENERATED_STATUS_PATHS)
    );
  }

  const reportPath = resolve(repoRoot, options.reportPath);
  const verdict = evaluateLocalCi(checks);
  const report = formatLocalCiReport({
    checks,
    generatedAt: new Date().toISOString(),
    projectRoot: repoRoot,
    reportPath,
    verdict
  });

  mkdirSync(dirname(reportPath), { recursive: true });
  writeFileSync(reportPath, report);

  console.log(`[MYB-83] local CI report: ${relative(repoRoot, reportPath)}`);
  console.log(`[MYB-83] verdict: ${verdict.verdict}`);
  for (const check of checks) {
    console.log(`[MYB-83] ${check.status.toUpperCase()} ${check.label}`);
  }

  if (!verdict.passed) {
    process.exitCode = 1;
  }

  return { checks, reportPath, verdict };
}

function printHelp() {
  console.log(`Usage: npm run validate:local-ci -- [--base <revision>] [--skip-unity] [--report <path>]

Runs the MYB-83 local Unity macOS-first validation.

Options:
  --base <revision>  Git revision for the branch diff whitespace check. Defaults to main.
  --skip-unity       Run repo hygiene checks only and mark Unity checks as skipped.
  --report <path>    Write the report to a custom path.
`);
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
  try {
    const options = parseArgs();
    if (options.help) {
      printHelp();
    } else {
      await runLocalCi(options);
    }
  } catch (error) {
    console.error(`[MYB-83] ${error.message}`);
    process.exitCode = 1;
  }
}
