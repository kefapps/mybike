using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MYB89;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MYB112
{
    public sealed class MYB112RuntimeFpsProbe : MonoBehaviour
    {
        public string label = "after";
        public string reportPath = "";
        public float warmupSeconds = 2f;
        public float measurementSeconds = 12f;
        public float startMeters = 18f;
        public float endMeters = 72f;
        public int width = 1280;
        public int height = 720;
        public int targetFps = 60;
        public int warningBelowFps = 45;
        public int redBelowFps = 30;

        private readonly List<float> frameTimes = new();
        private MYB89ProbeRide ride;
        private float elapsedSeconds;
        private bool firstMeasurementFrameLogged;
        private bool reportWritten;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigurePlayerLoopForProbe()
        {
            if (HasCommandLineFlag("-myb112FpsReport"))
            {
                Application.runInBackground = true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateFromCommandLine()
        {
            if (!HasCommandLineFlag("-myb112FpsReport"))
            {
                return;
            }

            if (FindAnyObjectByType<MYB112RuntimeFpsProbe>() != null)
            {
                return;
            }

            var probe = new GameObject("MYB112_RuntimeFpsProbe");
            DontDestroyOnLoad(probe);
            probe.AddComponent<MYB112RuntimeFpsProbe>();
        }

        private void Start()
        {
            ApplyCommandLineArgs();
            Application.runInBackground = true;
            Time.timeScale = 1f;
            DisableLegacyInputModules();
            Screen.SetResolution(width, height, false);
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = targetFps;

            ride = FindAnyObjectByType<MYB89ProbeRide>();
            if (ride != null)
            {
                ride.autoplay = false;
                ride.RebuildRouteCache();
                ride.SetPreviewProgress(startMeters);
            }

            Debug.Log("MYB-112 runtime FPS probe started for " + label + " -> " + reportPath);
        }

        private void Update()
        {
            if (reportWritten)
            {
                return;
            }

            elapsedSeconds += Time.unscaledDeltaTime;
            var sequenceSeconds = Mathf.Max(0.001f, measurementSeconds);
            var measureElapsed = Mathf.Max(0f, elapsedSeconds - warmupSeconds);
            var t = Mathf.Clamp01(measureElapsed / sequenceSeconds);

            if (ride != null)
            {
                ride.SetPreviewProgress(Mathf.Lerp(startMeters, endMeters, t));
            }

            if (elapsedSeconds >= warmupSeconds)
            {
                frameTimes.Add(Mathf.Max(0.0001f, Time.unscaledDeltaTime));
                if (!firstMeasurementFrameLogged)
                {
                    firstMeasurementFrameLogged = true;
                    Debug.Log("MYB-112 runtime FPS probe measuring " + label);
                }
            }

            if (measureElapsed >= sequenceSeconds)
            {
                WriteReportAndQuit();
            }
        }

        private static bool HasCommandLineFlag(string flag)
        {
            return Environment.GetCommandLineArgs().Any(argument => argument == flag);
        }

        private static void DisableLegacyInputModules()
        {
            foreach (var inputModule in FindObjectsByType<StandaloneInputModule>(FindObjectsInactive.Exclude))
            {
                inputModule.enabled = false;
            }
        }

        private void ApplyCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-myb112FpsReport":
                        reportPath = args[i + 1];
                        break;
                    case "-myb112FpsLabel":
                        label = args[i + 1];
                        break;
                }
            }
        }

        private void WriteReportAndQuit()
        {
            reportWritten = true;
            var absoluteReportPath = string.IsNullOrEmpty(reportPath)
                ? Path.Combine(Application.persistentDataPath, "myb-112-runtime-fps-" + label + ".txt")
                : reportPath;
            Directory.CreateDirectory(Path.GetDirectoryName(absoluteReportPath) ?? Application.persistentDataPath);

            var frameCount = frameTimes.Count;
            var totalSeconds = frameTimes.Sum();
            var averageFps = frameCount <= 0 || totalSeconds <= 0f ? 0f : frameCount / totalSeconds;
            var sortedFrameFps = frameTimes
                .Select(delta => 1f / Mathf.Max(0.0001f, delta))
                .OrderBy(value => value)
                .ToArray();
            var onePercentLowFps = sortedFrameFps.Length == 0
                ? 0f
                : sortedFrameFps[Mathf.Clamp(Mathf.FloorToInt(sortedFrameFps.Length * 0.01f), 0, sortedFrameFps.Length - 1)];
            var minFps = sortedFrameFps.Length == 0 ? 0f : sortedFrameFps[0];
            var status = StatusFor(Mathf.Min(averageFps, onePercentLowFps));

            File.WriteAllLines(absoluteReportPath, new[]
            {
                "MYB-112 runtime FPS probe",
                "Generated UTC: " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                "Label: " + label,
                "Status: " + status,
                "Resolution: " + width.ToString(CultureInfo.InvariantCulture) + "x" + height.ToString(CultureInfo.InvariantCulture),
                "Quality: " + (QualitySettings.names.Length == 0 ? QualitySettings.GetQualityLevel().ToString(CultureInfo.InvariantCulture) : QualitySettings.names[QualitySettings.GetQualityLevel()]),
                "Target FPS: " + targetFps.ToString(CultureInfo.InvariantCulture),
                "Warning below FPS: " + warningBelowFps.ToString(CultureInfo.InvariantCulture),
                "Red below FPS: " + redBelowFps.ToString(CultureInfo.InvariantCulture),
                "Warmup seconds: " + warmupSeconds.ToString("0.00", CultureInfo.InvariantCulture),
                "Measurement seconds: " + totalSeconds.ToString("0.00", CultureInfo.InvariantCulture),
                "Frames measured: " + frameCount.ToString(CultureInfo.InvariantCulture),
                "Average FPS: " + averageFps.ToString("0.0", CultureInfo.InvariantCulture),
                "1 percent low FPS: " + onePercentLowFps.ToString("0.0", CultureInfo.InvariantCulture),
                "Minimum FPS: " + minFps.ToString("0.0", CultureInfo.InvariantCulture),
                "Sequence meters: " + startMeters.ToString("0.0", CultureInfo.InvariantCulture) + " -> " + endMeters.ToString("0.0", CultureInfo.InvariantCulture),
                "Video FPS: not measured here; video proof is separate."
            });

            Debug.Log("MYB-112 runtime FPS probe wrote " + absoluteReportPath + " with status " + status);
            Application.Quit(status == "red" ? 30 : 0);
        }

        private string StatusFor(float fps)
        {
            if (fps < redBelowFps)
            {
                return "red";
            }

            if (fps < warningBelowFps)
            {
                return "warning";
            }

            return "target";
        }
    }
}
