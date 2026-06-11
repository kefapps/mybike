# Echapee4D Unity Baseline

`unity/Echapee4D` is the canonical Unity project for active MyBike work. As of
`MYB-94`, the priority runtime target is macOS. WebGL remains a secondary
validation/demo path. `unity/Echappee3D` was removed by `MYB-92`. `src/**`
remains a historical React/Vite/Three.js reference unless a Linear issue
explicitly targets it.

## Current Baseline

- Unity editor: `6000.4.10f1`
- Unity-MCP package: `com.ivanmurzak.unity.mcp` `0.80.0`
- Canonical scene: `Assets/Scenes/MYB89UnityMcpProbe.unity`
- WebGL build helper, secondary proof: `Tools/MYB-90/Build WebGL Readiness Probe`
- Baseline validator: `Tools/MYB-91/Validate Canonical Unity Baseline`

The scene keeps the `MYB89` name because it is the proven Unity-MCP/WebGL probe
that became the baseline on 2026-06-11. Rename it only through an explicit
follow-up ticket, after updating builders, captures, reports, and Linear sync.

## Platform Priority

- Primary target: Unity macOS.
- Secondary target: Unity WebGL for browser proof, private demo validation or
  regression evidence when a Linear issue explicitly asks for it.
- Candidate later target: Android, after a dedicated platform ticket and module
  installation check.
- Connected-bike direction: macOS/CoreBluetooth/FTMS, only after a real-device
  proof ticket. Mock mode remains required at all times.

## Source Control Boundary

Version these Unity roots:

- `Assets/`
- `Packages/`
- `ProjectSettings/`

Do not version generated Unity folders such as `Library/`, `Temp/`, `Logs/`,
`UserSettings/`, `Obj/`, `Build/`, `Builds/`, `MemoryCaptures/`, or
`Recordings/`.

## Validation

From the repository root:

```bash
unity-mcp-cli status unity/Echapee4D --timeout 10000
```

From Unity, run:

```text
Tools/MYB-91/Validate Canonical Unity Baseline
```

Batchmode equivalent:

```bash
/Applications/Unity/Hub/Editor/6000.4.10f1/Unity.app/Contents/MacOS/Unity \
  -projectPath unity/Echapee4D \
  -batchmode \
  -quit \
  -executeMethod MYB91.Editor.MYB91CanonicalBaselineValidator.ValidateCanonicalBaselineCli \
  -logFile _bmad-output/unity-test-results/myb-91-canonical-baseline-unity.log
```

The validator writes:

```text
_bmad-output/unity-test-results/myb-91-canonical-baseline.txt
```

## Secondary WebGL Rebuild And Capture

When a ticket asks for WebGL proof, rebuild the current WebGL probe through
Unity:

```text
Tools/MYB-90/Build WebGL Readiness Probe
```

Then capture the local browser proof:

```bash
node scripts/capture-unity-webgl-readiness.mjs
```

The builder writes the WebGL output under
`_bmad-output/unity-webgl-builds/myb-90-unity-mcp-probe/`; the capture script
starts its own local HTTP server and writes video evidence under
`_bmad-output/video-captures/`.
