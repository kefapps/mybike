using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using MYB73;
using MYB89;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MYB165UnityRecorderVideoCapture
{
    private const string TicketId = "MYB-165";
    private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string GeneratedRootName = "MYB165_FirstTrueRouteRoot";
    private const string VideoRootRelative = "_bmad-output/video-captures/MYB-165";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-165";
    private const string RecorderReportRelativePath = ImplementationRootRelative + "/myb-165-video-capture-recorder-report.md";
    private const float NormalSpeedMetersPerSecond = 12.5f;
    private const int VideoFrameRate = 30;
    private const int VideoWidth = 1280;
    private const int VideoHeight = 720;
    private const float CompletionGraceSeconds = 4f;
    private const string SessionPrefix = "MYB165.UnityRecorder.";
    private const string PendingKey = SessionPrefix + "Pending";
    private const string OutputRelativeKey = SessionPrefix + "OutputRelative";
    private const string OutputDirectoryKey = SessionPrefix + "OutputDirectory";
    private const string OutputFileNoExtensionKey = SessionPrefix + "OutputFileNoExtension";
    private const string DurationSecondsKey = SessionPrefix + "DurationSeconds";
    private const string RouteLengthMetersKey = SessionPrefix + "RouteLengthMeters";
    private const string FrameCountKey = SessionPrefix + "FrameCount";
    private const string ErrorKey = SessionPrefix + "Error";

    private static RecorderController recorderController;
    private static MYB89ProbeRide activeRide;
    private static readonly List<Tuple<GameObject, bool>> PreviewStates = new List<Tuple<GameObject, bool>>();
    private static bool callbacksRegistered;
    private static bool recordingStarted;
    private static double recordingStartTime;
    private static int previousCaptureFramerate;
    private static int previousTargetFrameRate;
    private static bool previousHudVisible;
    private static bool previousUseEffortSimulator;
    private static bool previousAutoplay;
    private static bool previousWaitForRoutePreview;
    private static float previousSpeedMetersPerSecond;
    private static float previousProgressMeters;

    static MYB165UnityRecorderVideoCapture()
    {
        ResumePendingCapture();
    }

    [MenuItem("Tools/MyBike/MYB-165/Capture Full Route Video (Unity Recorder)")]
    public static void RunBatchCaptureVideoOnly()
    {
        Directory.CreateDirectory(ToRepoPath(VideoRootRelative));
        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));

        EnsureRouteSceneReady();

        var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
        var camera = Camera.main;
        if (ride == null || camera == null)
        {
            throw new InvalidOperationException("MYB-165 Unity Recorder capture requires MYB89ProbeRide and MainCamera.");
        }

        ride.RebuildRouteCache();
        var routeLength = ride.RouteLength;
        var durationSeconds = routeLength / NormalSpeedMetersPerSecond;
        var frameCount = Mathf.CeilToInt(durationSeconds * VideoFrameRate);
        var stamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        var outputRelative = VideoRootRelative + "/myb-165-first-true-route-recorder-" + stamp;
        var outputDirectory = ToRepoPath(outputRelative);
        var outputFileNoExtension = Path.Combine(outputDirectory, "myb-165-first-true-route-bike-pov-3min-720p-30fps");
        Directory.CreateDirectory(outputDirectory);

        SessionState.SetBool(PendingKey, true);
        SessionState.SetString(OutputRelativeKey, outputRelative);
        SessionState.SetString(OutputDirectoryKey, outputDirectory);
        SessionState.SetString(OutputFileNoExtensionKey, outputFileNoExtension);
        SessionState.SetString(DurationSecondsKey, durationSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        SessionState.SetString(RouteLengthMetersKey, routeLength.ToString("0.###", CultureInfo.InvariantCulture));
        SessionState.SetInt(FrameCountKey, frameCount);
        SessionState.SetString(ErrorKey, string.Empty);

        WritePendingSummary("pending");
        RegisterCallbacks();
        Debug.Log("MYB-165 Unity Recorder capture prepared at " + outputRelative + ". Entering Play Mode.");
        EditorApplication.EnterPlaymode();
    }

    private static void EnsureRouteSceneReady()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (GameObject.Find(GeneratedRootName) != null
            && UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>() != null
            && Camera.main != null)
        {
            return;
        }

        var result = MYB165FirstTrueRouteBuilder.BuildFirstTrueRoute(captureVideoFrames: false);
        if (result.BlockingErrors.Count > 0)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }
    }

    private static void ResumePendingCapture()
    {
        if (SessionState.GetBool(PendingKey, false))
        {
            RegisterCallbacks();
        }
    }

    private static void RegisterCallbacks()
    {
        if (callbacksRegistered)
        {
            return;
        }

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        callbacksRegistered = true;
    }

    private static void UnregisterCallbacks()
    {
        if (!callbacksRegistered)
        {
            return;
        }

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.update -= MonitorRecording;
        callbacksRegistered = false;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(PendingKey, false))
        {
            return;
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            StartRecorderInPlayMode();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            StopRecorderIfNeeded();
            RestorePlayModeOverrides();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            CompleteCaptureAfterPlayMode();
        }
    }

    private static void StartRecorderInPlayMode()
    {
        try
        {
            activeRide = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (activeRide == null || camera == null)
            {
                FailAndExitPlayMode("Missing MYB89ProbeRide or MainCamera after entering Play Mode.");
                return;
            }

            activeRide.RebuildRouteCache();
            previousCaptureFramerate = Time.captureFramerate;
            previousTargetFrameRate = Application.targetFrameRate;
            previousUseEffortSimulator = activeRide.useEffortSimulator;
            previousAutoplay = activeRide.autoplay;
            previousWaitForRoutePreview = activeRide.waitForRoutePreview;
            previousSpeedMetersPerSecond = activeRide.speedMetersPerSecond;
            previousProgressMeters = activeRide.progressMeters;
            previousHudVisible = GetHudVisible(activeRide);

            HidePreviewPanels();
            SetHudVisible(activeRide, false);
            activeRide.useEffortSimulator = false;
            activeRide.speedMetersPerSecond = NormalSpeedMetersPerSecond;
            activeRide.autoplay = true;
            activeRide.waitForRoutePreview = false;
            activeRide.SetPreviewProgress(0f);

            Time.captureFramerate = VideoFrameRate;
            Application.targetFrameRate = VideoFrameRate;

            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            controllerSettings.FrameRatePlayback = FrameRatePlayback.Constant;
            controllerSettings.FrameRate = VideoFrameRate;
            controllerSettings.CapFrameRate = true;
            controllerSettings.ExitPlayMode = false;
            controllerSettings.SetRecordModeToFrameInterval(0, SessionState.GetInt(FrameCountKey, 1) - 1);

            var movieSettings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            movieSettings.name = "MYB-165 Bike POV Unity Recorder";
            movieSettings.Enabled = true;
            movieSettings.CaptureAudio = false;
            movieSettings.CaptureAlpha = false;
            movieSettings.EncoderSettings = new CoreEncoderSettings
            {
                EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
                Codec = CoreEncoderSettings.OutputCodec.MP4,
                TargetBitRate = 10f,
                GopSize = VideoFrameRate
            };
            movieSettings.ImageInputSettings = new CameraInputSettings
            {
                Source = ImageSource.MainCamera,
                OutputWidth = VideoWidth,
                OutputHeight = VideoHeight,
                CaptureUI = false
            };
            movieSettings.OutputFile = SessionState.GetString(OutputFileNoExtensionKey, string.Empty);

            controllerSettings.AddRecorderSettings(movieSettings);
            RecorderOptions.VerboseMode = false;
            recorderController = new RecorderController(controllerSettings);
            recorderController.PrepareRecording();
            if (!recorderController.StartRecording())
            {
                FailAndExitPlayMode("RecorderController.StartRecording returned false.");
                return;
            }

            recordingStarted = true;
            recordingStartTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += MonitorRecording;
            WritePendingSummary("recording");
            Debug.Log("MYB-165 Unity Recorder capture started.");
        }
        catch (Exception exception)
        {
            FailAndExitPlayMode(exception.Message);
        }
    }

    private static void MonitorRecording()
    {
        if (!SessionState.GetBool(PendingKey, false) || !recordingStarted)
        {
            return;
        }

        if (recorderController == null || !recorderController.IsRecording())
        {
            EditorApplication.ExitPlaymode();
            return;
        }

        if (activeRide != null)
        {
            var routeLength = ParseSessionFloat(RouteLengthMetersKey);
            if (routeLength > 0f && activeRide.progressMeters >= routeLength - 0.1f)
            {
                StopRecorderIfNeeded();
                EditorApplication.ExitPlaymode();
                return;
            }
        }

        var durationSeconds = ParseSessionFloat(DurationSecondsKey);
        if (durationSeconds > 0f
            && EditorApplication.timeSinceStartup - recordingStartTime > durationSeconds + CompletionGraceSeconds)
        {
            StopRecorderIfNeeded();
            EditorApplication.ExitPlaymode();
        }
    }

    private static void StopRecorderIfNeeded()
    {
        EditorApplication.update -= MonitorRecording;
        if (recorderController != null)
        {
            recorderController.StopRecording();
            recorderController = null;
        }
    }

    private static void CompleteCaptureAfterPlayMode()
    {
        var error = SessionState.GetString(ErrorKey, string.Empty);
        var expectedMp4Path = SessionState.GetString(OutputFileNoExtensionKey, string.Empty) + ".mp4";
        var mp4Path = File.Exists(expectedMp4Path)
            ? expectedMp4Path
            : FindFirstMp4(SessionState.GetString(OutputDirectoryKey, string.Empty));
        var status = string.IsNullOrEmpty(error) && File.Exists(mp4Path) ? "complete" : "failed";
        if (string.IsNullOrEmpty(error) && !File.Exists(mp4Path))
        {
            error = "Unity Recorder finished but no MP4 was found.";
            SessionState.SetString(ErrorKey, error);
        }

        WriteCaptureSummary(status, mp4Path, error);
        WriteRecorderReport(status, mp4Path, error);
        ClearSession();
        UnregisterCallbacks();

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(status == "complete" ? 0 : 1);
        }
    }

    private static void FailAndExitPlayMode(string message)
    {
        SessionState.SetString(ErrorKey, message);
        Debug.LogError("MYB-165 Unity Recorder capture failed: " + message);
        StopRecorderIfNeeded();
        EditorApplication.ExitPlaymode();
    }

    private static void RestorePlayModeOverrides()
    {
        Time.captureFramerate = previousCaptureFramerate;
        Application.targetFrameRate = previousTargetFrameRate;
        if (activeRide != null)
        {
            activeRide.useEffortSimulator = previousUseEffortSimulator;
            activeRide.speedMetersPerSecond = previousSpeedMetersPerSecond;
            activeRide.autoplay = previousAutoplay;
            activeRide.waitForRoutePreview = previousWaitForRoutePreview;
            activeRide.SetPreviewProgress(previousProgressMeters);
            SetHudVisible(activeRide, previousHudVisible);
        }

        foreach (var state in PreviewStates)
        {
            if (state.Item1 != null)
            {
                state.Item1.SetActive(state.Item2);
            }
        }

        PreviewStates.Clear();
    }

    private static void HidePreviewPanels()
    {
        PreviewStates.Clear();
        var previewPanels = UnityEngine.Object.FindObjectsByType<MYB73RoutePreviewPanel>(FindObjectsInactive.Include);
        foreach (var preview in previewPanels)
        {
            var root = preview.panelRoot == null ? preview.gameObject : preview.panelRoot;
            if (root != null)
            {
                PreviewStates.Add(new Tuple<GameObject, bool>(root, root.activeSelf));
                root.SetActive(false);
            }
        }

        var namedPreview = GameObject.Find("MYB73_RoutePreview");
        if (namedPreview != null && PreviewStates.TrueForAll(state => state.Item1 != namedPreview))
        {
            PreviewStates.Add(new Tuple<GameObject, bool>(namedPreview, namedPreview.activeSelf));
            namedPreview.SetActive(false);
        }
    }

    private static bool GetHudVisible(MYB89ProbeRide ride)
    {
        return ride.distanceLabel == null || ride.distanceLabel.gameObject.activeSelf;
    }

    private static void SetHudVisible(MYB89ProbeRide ride, bool visible)
    {
        SetTextVisible(ride.distanceLabel, visible);
        SetTextVisible(ride.speedLabel, visible);
        SetTextVisible(ride.difficultyLabel, visible);
        SetTextVisible(ride.gradeLabel, visible);
        SetTextVisible(ride.segmentLabel, visible);
        SetTextVisible(ride.verdictLabel, visible);
    }

    private static void SetTextVisible(Text text, bool visible)
    {
        if (text != null)
        {
            text.gameObject.SetActive(visible);
        }
    }

    private static void WritePendingSummary(string status)
    {
        WriteCaptureSummary(status, string.Empty, string.Empty);
    }

    private static void WriteCaptureSummary(string status, string absoluteMp4Path, string error)
    {
        var outputDirectory = SessionState.GetString(OutputDirectoryKey, string.Empty);
        if (string.IsNullOrEmpty(outputDirectory))
        {
            return;
        }

        Directory.CreateDirectory(outputDirectory);
        var relativeMp4Path = ToRepoRelativePath(absoluteMp4Path);
        File.WriteAllText(
            Path.Combine(outputDirectory, "capture-summary.json"),
            "{\n"
            + "  \"ticket\": \"MYB-165\",\n"
            + "  \"captureMethod\": \"Unity Recorder\",\n"
            + "  \"status\": \"" + EscapeJson(status) + "\",\n"
            + "  \"outputScene\": \"" + ScenePath + "\",\n"
            + "  \"generatedRoot\": \"" + GeneratedRootName + "\",\n"
            + "  \"routeLengthMeters\": " + FormatFloat(ParseSessionFloat(RouteLengthMetersKey)) + ",\n"
            + "  \"normalSpeedMetersPerSecond\": " + FormatFloat(NormalSpeedMetersPerSecond) + ",\n"
            + "  \"durationSeconds\": " + FormatFloat(ParseSessionFloat(DurationSecondsKey)) + ",\n"
            + "  \"frameRate\": " + VideoFrameRate.ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"frameCount\": " + SessionState.GetInt(FrameCountKey, 0).ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"width\": " + VideoWidth.ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"height\": " + VideoHeight.ToString(CultureInfo.InvariantCulture) + ",\n"
            + "  \"mp4Path\": \"" + EscapeJson(relativeMp4Path) + "\",\n"
            + "  \"error\": \"" + EscapeJson(error) + "\"\n"
            + "}\n");
    }

    private static void WriteRecorderReport(string status, string absoluteMp4Path, string error)
    {
        var relativeMp4Path = ToRepoRelativePath(absoluteMp4Path);
        var summaryRelativePath = SessionState.GetString(OutputRelativeKey, string.Empty) + "/capture-summary.json";
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("# MYB-165 Unity Recorder Video Capture Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine("MYB-165 now has a Unity Recorder based capture path for the full first-person route video.");
        builder.AppendLine();
        builder.AppendLine("## Capture");
        builder.AppendLine("- method: `Unity Recorder com.unity.recorder`");
        builder.AppendLine("- status: `" + status + "`");
        builder.AppendLine("- output scene: `" + ScenePath + "`");
        builder.AppendLine("- camera source: `MainCamera`");
        builder.AppendLine("- resolution: `" + VideoWidth + "x" + VideoHeight + "`");
        builder.AppendLine("- frame rate: `" + VideoFrameRate + "fps`");
        builder.AppendLine("- route speed: `" + FormatFloat(NormalSpeedMetersPerSecond) + "m/s`");
        builder.AppendLine("- duration: `" + FormatFloat(ParseSessionFloat(DurationSecondsKey)) + "s`");
        builder.AppendLine("- frame count: `" + SessionState.GetInt(FrameCountKey, 0) + "`");
        builder.AppendLine();
        builder.AppendLine("## Output");
        builder.AppendLine("- MP4: `" + relativeMp4Path + "`");
        builder.AppendLine("- summary: `" + summaryRelativePath + "`");
        builder.AppendLine();
        builder.AppendLine("## Fallback");
        builder.AppendLine("The previous RenderTexture JPG sequence path remains available as `Tools/MyBike/MYB-165/Capture Full Route Video Frames Fallback`.");
        builder.AppendLine();
        builder.AppendLine("## Errors");
        builder.AppendLine(string.IsNullOrEmpty(error) ? "- None recorded." : "- " + error);
        File.WriteAllText(ToRepoPath(RecorderReportRelativePath), builder.ToString());
    }

    private static void ClearSession()
    {
        SessionState.SetBool(PendingKey, false);
        SessionState.EraseString(OutputRelativeKey);
        SessionState.EraseString(OutputDirectoryKey);
        SessionState.EraseString(OutputFileNoExtensionKey);
        SessionState.EraseString(DurationSecondsKey);
        SessionState.EraseString(RouteLengthMetersKey);
        SessionState.EraseInt(FrameCountKey);
        SessionState.EraseString(ErrorKey);
        recordingStarted = false;
        activeRide = null;
    }

    private static string FindFirstMp4(string outputDirectory)
    {
        if (string.IsNullOrEmpty(outputDirectory) || !Directory.Exists(outputDirectory))
        {
            return string.Empty;
        }

        var files = Directory.GetFiles(outputDirectory, "*.mp4", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.Ordinal);
        return files.Length == 0 ? string.Empty : files[0];
    }

    private static float ParseSessionFloat(string key)
    {
        return float.TryParse(SessionState.GetString(key, "0"), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0f;
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../..", relativePath));
    }

    private static string ToRepoRelativePath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath))
        {
            return string.Empty;
        }

        var repoRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
        var fullPath = Path.GetFullPath(absolutePath);
        if (!fullPath.StartsWith(repoRoot, StringComparison.Ordinal))
        {
            return absolutePath;
        }

        return fullPath.Substring(repoRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Replace('\\', '/');
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
