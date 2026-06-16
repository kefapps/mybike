using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MYB145CaptureRigHelper
{
    private const string SetupMenuPath = "Tools/MyBike/Capture/MYB-145 Setup Capture Cameras";
    private const string ValidateMenuPath = "Tools/MyBike/Capture/MYB-145 Validate Capture Cameras";
    private const string CaptureMenuPath = "Tools/MyBike/Capture/MYB-145 Capture Route + Overview";
    private const string RouteCameraName = "RouteCamera";
    private const string OverviewCameraName = "OverviewCamera";
    private const string OutputRootRelativePath = "_bmad-output/visual-checkpoints";
    private const int CaptureWidth = 1600;
    private const int CaptureHeight = 900;
    private const int MetadataSchemaVersion = 1;

    private static readonly Vector3 DefaultRoutePosition = new Vector3(-0.85f, 1.55f, 7.5f);
    private static readonly Vector3 DefaultRouteLookTarget = new Vector3(0f, 1.15f, 42f);
    private static readonly Vector3 DefaultOverviewPosition = new Vector3(0f, 86f, 66f);
    private static readonly Vector3 DefaultOverviewRotation = new Vector3(90f, 0f, 0f);

    [MenuItem(SetupMenuPath)]
    public static void SetupCaptureCamerasFromMenu()
    {
        var result = SetupCaptureCameras("Menu");
        LogResult(result);
    }

    [MenuItem(ValidateMenuPath)]
    public static void ValidateCaptureCamerasFromMenu()
    {
        var result = ValidateCaptureCameras("Menu");
        LogResult(result);
    }

    [MenuItem(CaptureMenuPath)]
    public static void CaptureRouteAndOverviewFromMenu()
    {
        var result = CaptureRouteAndOverview("Menu", CaptureOptions.FromCommandLine());
        LogResult(result);
    }

    public static void RunBatchValidate()
    {
        var options = CaptureOptions.FromCommandLine();
        OpenSceneIfProvided(options);
        var result = ValidateCaptureCameras("Batch");
        LogResult(result);
        EditorApplication.Exit(result.ErrorCount > 0 ? 1 : 0);
    }

    public static void RunBatchCapture()
    {
        var options = CaptureOptions.FromCommandLine();
        OpenSceneIfProvided(options);
        var result = CaptureRouteAndOverview("Batch", options);
        LogResult(result);
        EditorApplication.Exit(result.ErrorCount > 0 ? 1 : 0);
    }

    public static void RunBatchSetup()
    {
        var options = CaptureOptions.FromCommandLine();
        OpenSceneIfProvided(options);
        var result = SetupCaptureCameras("Batch");
        LogResult(result);
        EditorApplication.Exit(result.ErrorCount > 0 ? 1 : 0);
    }

    public static void RunBatchExample()
    {
        var options = CaptureOptions.FromCommandLine();
        OpenSceneIfProvided(options);
        var setup = SetupCaptureCameras("BatchExample");
        LogResult(setup);

        if (setup.ErrorCount > 0)
        {
            EditorApplication.Exit(1);
            return;
        }

        var capture = CaptureRouteAndOverview("BatchExample", options);
        LogResult(capture);
        EditorApplication.Exit(capture.ErrorCount > 0 ? 1 : 0);
    }

    public static CaptureResult SetupCaptureCameras()
    {
        return SetupCaptureCameras("Unknown");
    }

    public static CaptureResult SetupCaptureCameras(string executionMode)
    {
        var result = CreateResult(executionMode, "setup");

        try
        {
            var routeCameras = FindNamedCameras(RouteCameraName);
            var overviewCameras = FindNamedCameras(OverviewCameraName);

            if (routeCameras.Count > 1)
            {
                result.AddError("MULTIPLE_ROUTE_CAMERAS", "Multiple RouteCamera objects exist. Resolve before setup can normalize safely.");
            }

            if (overviewCameras.Count > 1)
            {
                result.AddError("MULTIPLE_OVERVIEW_CAMERAS", "Multiple OverviewCamera objects exist. Resolve before setup can normalize safely.");
            }

            if (result.ErrorCount == 0)
            {
                var routeCamera = routeCameras.Count == 1 ? routeCameras[0] : CreateCamera(RouteCameraName, result);
                var overviewCamera = overviewCameras.Count == 1 ? overviewCameras[0] : CreateCamera(OverviewCameraName, result);

                NormalizeRouteCamera(routeCamera, result);
                NormalizeOverviewCamera(overviewCamera, result);
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                result.AddWarning("SCENE_DIRTY_NOT_SAVED", "Setup changed capture cameras and marked the active scene dirty. Save explicitly if this checkpoint rig should persist.");
            }

            AddCameraRecords(result);
            WriteReports(result);
        }
        catch (Exception exception)
        {
            result.AddError("SETUP_EXCEPTION", exception.GetType().FullName + ": " + exception.Message);
            result.AddInfo("SETUP_EXCEPTION_DETAIL", exception.ToString());
            TryWriteReports(result);
        }

        return result;
    }

    public static CaptureResult ValidateCaptureCameras()
    {
        return ValidateCaptureCameras("Unknown");
    }

    public static CaptureResult ValidateCaptureCameras(string executionMode)
    {
        var result = CreateResult(executionMode, "validate");

        try
        {
            ValidateCameraRig(result, captureRequired: false);
            AddCameraRecords(result);
            WriteReports(result);
        }
        catch (Exception exception)
        {
            result.AddError("VALIDATE_EXCEPTION", exception.GetType().FullName + ": " + exception.Message);
            result.AddInfo("VALIDATE_EXCEPTION_DETAIL", exception.ToString());
            TryWriteReports(result);
        }

        return result;
    }

    public static CaptureResult CaptureRouteAndOverview()
    {
        return CaptureRouteAndOverview("Unknown", CaptureOptions.Default());
    }

    public static CaptureResult CaptureRouteAndOverview(string executionMode, CaptureOptions options)
    {
        options = options ?? CaptureOptions.Default();
        var result = CreateResult(executionMode, "capture");
        result.TicketId = options.TicketId;
        result.State = options.State;
        result.OutputDirectory = Path.Combine(GetRepoRoot(), OutputRootRelativePath, options.TicketId);
        result.OutputDirectoryRelative = OutputRootRelativePath + "/" + options.TicketId;

        try
        {
            ValidateCaptureState(options.State, result);
            ValidateCameraRig(result, captureRequired: true);
            AddCameraRecords(result);

            if (result.ErrorCount == 0)
            {
                Directory.CreateDirectory(result.OutputDirectory);
                var routeCamera = FindSingleNamedCamera(RouteCameraName);
                var overviewCamera = FindSingleNamedCamera(OverviewCameraName);
                CaptureCamera(routeCamera, "route", options.State, result);
                CaptureCamera(overviewCamera, "overview", options.State, result);
                GenerateExplicitComparisons(options, result);
            }

            WriteReports(result);
        }
        catch (Exception exception)
        {
            result.AddError("CAPTURE_EXCEPTION", exception.GetType().FullName + ": " + exception.Message);
            result.AddInfo("CAPTURE_EXCEPTION_DETAIL", exception.ToString());
            TryWriteReports(result);
        }

        return result;
    }

    private static CaptureResult CreateResult(string executionMode, string mode)
    {
        var ticketId = ReadCommandLineValue("-myb145Ticket", "MYB-145");
        var state = ReadCommandLineValue("-myb145State", "current");
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH-mm-ssZ", CultureInfo.InvariantCulture);
        var outputDirectoryRelative = OutputRootRelativePath + "/" + ticketId;

        return new CaptureResult
        {
            ExecutionMode = string.IsNullOrWhiteSpace(executionMode) ? "Unknown" : executionMode,
            Mode = mode,
            TicketId = ticketId,
            State = state,
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            Timestamp = timestamp,
            RepoRoot = GetRepoRoot(),
            ScenePath = SceneManager.GetActiveScene().path,
            SceneName = SceneManager.GetActiveScene().name,
            Branch = GetGitValue("rev-parse --abbrev-ref HEAD"),
            Commit = GetGitValue("rev-parse --short HEAD"),
            OutputDirectoryRelative = outputDirectoryRelative,
            OutputDirectory = Path.Combine(GetRepoRoot(), outputDirectoryRelative)
        };
    }

    private static void ValidateCameraRig(CaptureResult result, bool captureRequired)
    {
        var routeCameras = FindNamedCameras(RouteCameraName);
        var overviewCameras = FindNamedCameras(OverviewCameraName);

        if (routeCameras.Count == 0)
        {
            result.AddError("ROUTE_CAMERA_MISSING", "RouteCamera is missing. Run setup explicitly before capture.");
        }
        else if (routeCameras.Count > 1)
        {
            result.AddError("MULTIPLE_ROUTE_CAMERAS", "Multiple RouteCamera objects exist. MYB-145 cannot choose one safely.");
        }

        if (overviewCameras.Count == 0)
        {
            result.AddError("OVERVIEW_CAMERA_MISSING", "OverviewCamera is missing. Run setup explicitly before capture.");
        }
        else if (overviewCameras.Count > 1)
        {
            result.AddError("MULTIPLE_OVERVIEW_CAMERAS", "Multiple OverviewCamera objects exist. MYB-145 cannot choose one safely.");
        }

        if (routeCameras.Count == 1)
        {
            ValidateRouteCameraSettings(routeCameras[0], result);
        }

        if (overviewCameras.Count == 1)
        {
            ValidateOverviewCameraSettings(overviewCameras[0], result);
        }

        if (!captureRequired && result.ErrorCount == 0)
        {
            result.AddInfo("CAPTURE_RIG_VALID", "RouteCamera and OverviewCamera are present and usable.");
        }
    }

    private static void ValidateRouteCameraSettings(Camera camera, CaptureResult result)
    {
        if (Mathf.Abs(camera.fieldOfView - 50f) > 0.01f)
        {
            result.AddWarning("ROUTE_CAMERA_FOV_NON_CANONICAL", "RouteCamera FOV is " + FormatFloat(camera.fieldOfView) + "; V1 default is 50.");
        }

        if (camera.orthographic)
        {
            result.AddWarning("ROUTE_CAMERA_ORTHOGRAPHIC", "RouteCamera should be perspective.");
        }

        if (camera.nearClipPlane > 0.1f || camera.farClipPlane < 150f)
        {
            result.AddWarning("ROUTE_CAMERA_CLIPPING_NON_CANONICAL", "RouteCamera clipping differs from MYB-145 V1 defaults.");
        }
    }

    private static void ValidateOverviewCameraSettings(Camera camera, CaptureResult result)
    {
        if (!camera.orthographic)
        {
            result.AddWarning("OVERVIEW_CAMERA_NOT_ORTHOGRAPHIC", "OverviewCamera should be orthographic for stable density/context review.");
        }

        if (camera.orthographic && Mathf.Abs(camera.orthographicSize - 42f) > 0.01f)
        {
            result.AddWarning("OVERVIEW_CAMERA_SIZE_NON_CANONICAL", "OverviewCamera orthographic size is " + FormatFloat(camera.orthographicSize) + "; V1 default is 42.");
        }
    }

    private static Camera CreateCamera(string name, CaptureResult result)
    {
        var cameraObject = new GameObject(name);
        var camera = cameraObject.AddComponent<Camera>();
        result.AddInfo("CAMERA_CREATED", name + " created by explicit setup mode.");
        return camera;
    }

    private static void NormalizeRouteCamera(Camera camera, CaptureResult result)
    {
        camera.name = RouteCameraName;
        camera.transform.position = DefaultRoutePosition;
        camera.transform.rotation = Quaternion.LookRotation((DefaultRouteLookTarget - DefaultRoutePosition).normalized, Vector3.up);
        camera.fieldOfView = 50f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 180f;
        camera.orthographic = false;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.35f, 0.40f, 0.37f);
        result.AddInfo("ROUTE_CAMERA_NORMALIZED", "RouteCamera transform and capture parameters normalized.");
    }

    private static void NormalizeOverviewCamera(Camera camera, CaptureResult result)
    {
        camera.name = OverviewCameraName;
        camera.transform.position = DefaultOverviewPosition;
        camera.transform.rotation = Quaternion.Euler(DefaultOverviewRotation);
        camera.fieldOfView = 50f;
        camera.orthographic = true;
        camera.orthographicSize = 42f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 220f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.35f, 0.40f, 0.37f);
        result.AddInfo("OVERVIEW_CAMERA_NORMALIZED", "OverviewCamera transform and capture parameters normalized.");
    }

    private static void CaptureCamera(Camera camera, string captureType, string state, CaptureResult result)
    {
        var fileName = result.Timestamp + "-" + state + "-" + captureType + ".png";
        var path = Path.Combine(result.OutputDirectory, fileName);
        RenderCameraToPng(camera, path, CaptureWidth, CaptureHeight);

        var relativePath = result.OutputDirectoryRelative + "/" + fileName;
        result.Captures.Add(new CaptureRecord
        {
            State = state,
            Type = captureType,
            Path = relativePath,
            Scene = result.ScenePath,
            Camera = camera.name,
            Position = FormatVector(camera.transform.position),
            Rotation = FormatVector(camera.transform.eulerAngles),
            Fov = camera.orthographic ? camera.orthographicSize : camera.fieldOfView,
            Resolution = CaptureWidth + "x" + CaptureHeight
        });
        result.AddInfo("CAPTURE_WRITTEN", captureType + " " + state + " capture written to `" + relativePath + "`.");
    }

    private static void RenderCameraToPng(Camera camera, string path, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetRepoRoot());

        var renderTexture = new RenderTexture(width, height, 24);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;

        try
        {
            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();

            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(renderTexture);
        }
    }

    private static void GenerateExplicitComparisons(CaptureOptions options, CaptureResult result)
    {
        GenerateComparisonIfProvided(options.BeforeRoutePath, options.AfterRoutePath, "route", options, result);
        GenerateComparisonIfProvided(options.BeforeOverviewPath, options.AfterOverviewPath, "overview", options, result);
    }

    private static void GenerateComparisonIfProvided(string beforePath, string afterPath, string type, CaptureOptions options, CaptureResult result)
    {
        var hasBefore = !string.IsNullOrWhiteSpace(beforePath);
        var hasAfter = !string.IsNullOrWhiteSpace(afterPath);

        if (!hasBefore && !hasAfter)
        {
            result.AddInfo("COMPARISON_NOT_REQUESTED", type + " before/after comparison was not requested.");
            return;
        }

        if (!hasBefore || !hasAfter)
        {
            result.AddError("COMPARISON_EXPLICIT_PATH_MISSING", type + " comparison requires explicit before and after paths.");
            return;
        }

        if (string.IsNullOrWhiteSpace(options.BaselineReason))
        {
            result.AddError("EXPLICIT_BASELINE_REASON_MISSING", type + " comparison requires `-myb145BaselineReason`.");
            return;
        }

        var beforeFullPath = ToFullPath(beforePath);
        var afterFullPath = ToFullPath(afterPath);
        if (!File.Exists(beforeFullPath))
        {
            result.AddError("COMPARISON_BEFORE_NOT_FOUND", "Before " + type + " capture not found: `" + beforePath + "`.");
            return;
        }

        if (!File.Exists(afterFullPath))
        {
            result.AddError("COMPARISON_AFTER_NOT_FOUND", "After " + type + " capture not found: `" + afterPath + "`.");
            return;
        }

        var fileName = result.Timestamp + "-" + type + "-before-after.png";
        var outputPath = Path.Combine(result.OutputDirectory, fileName);
        CreateSideBySidePng(beforeFullPath, afterFullPath, outputPath, result);
        var relativePath = result.OutputDirectoryRelative + "/" + fileName;

        result.Comparisons.Add(new ComparisonRecord
        {
            Type = type,
            BeforePath = ToProjectRelativePath(beforeFullPath),
            AfterPath = ToProjectRelativePath(afterFullPath),
            SheetPath = relativePath
        });
        result.ExplicitBaselineReason = options.BaselineReason;
        result.ExplicitBaselineSource = options.BaselineSource;
        result.ExplicitBaselineSelectedBy = options.BaselineSelectedBy;
        result.AddInfo("COMPARISON_WRITTEN", type + " before/after sheet written to `" + relativePath + "`.");
    }

    private static void CreateSideBySidePng(string beforePath, string afterPath, string outputPath, CaptureResult result)
    {
        var beforeBytes = File.ReadAllBytes(beforePath);
        var afterBytes = File.ReadAllBytes(afterPath);
        var beforeTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        var afterTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        beforeTexture.LoadImage(beforeBytes);
        afterTexture.LoadImage(afterBytes);

        if (beforeTexture.width != afterTexture.width || beforeTexture.height != afterTexture.height)
        {
            result.AddWarning("COMPARISON_RESOLUTION_MISMATCH", "Before/after captures have different resolutions.");
        }

        var width = beforeTexture.width + afterTexture.width;
        var height = Math.Max(beforeTexture.height, afterTexture.height);
        var sheet = new Texture2D(width, height, TextureFormat.RGB24, false);
        FillTexture(sheet, Color.black);
        CopyTexture(beforeTexture, sheet, 0, 0);
        CopyTexture(afterTexture, sheet, beforeTexture.width, 0);
        File.WriteAllBytes(outputPath, sheet.EncodeToPNG());

        UnityEngine.Object.DestroyImmediate(beforeTexture);
        UnityEngine.Object.DestroyImmediate(afterTexture);
        UnityEngine.Object.DestroyImmediate(sheet);
    }

    private static void FillTexture(Texture2D texture, Color color)
    {
        var pixels = Enumerable.Repeat(color, texture.width * texture.height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
    }

    private static void CopyTexture(Texture2D source, Texture2D target, int offsetX, int offsetY)
    {
        var pixels = source.GetPixels();
        target.SetPixels(offsetX, offsetY, source.width, source.height, pixels);
        target.Apply();
    }

    private static void AddCameraRecords(CaptureResult result)
    {
        result.Cameras.Clear();
        foreach (var name in new[] { RouteCameraName, OverviewCameraName })
        {
            var cameras = FindNamedCameras(name);
            if (cameras.Count == 0)
            {
                result.Cameras.Add(CameraRecord.Missing(name, CameraRole(name)));
                continue;
            }

            foreach (var camera in cameras)
            {
                result.Cameras.Add(new CameraRecord
                {
                    Name = name,
                    Role = CameraRole(name),
                    Found = true,
                    Position = FormatVector(camera.transform.position),
                    Rotation = FormatVector(camera.transform.eulerAngles),
                    Fov = camera.orthographic ? camera.orthographicSize : camera.fieldOfView,
                    Notes = camera.orthographic ? "orthographic" : "perspective"
                });
            }
        }
    }

    private static string CameraRole(string name)
    {
        return name == RouteCameraName ? "blocking validation" : "secondary context";
    }

    private static void ValidateCaptureState(string state, CaptureResult result)
    {
        if (state == "current" || state == "before" || state == "after")
        {
            return;
        }

        result.AddError("CAPTURE_STATE_INVALID", "State must be one of: current, before, after.");
    }

    private static List<Camera> FindNamedCameras(string name)
    {
        return UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(camera => camera != null && camera.gameObject != null && camera.gameObject.name == name)
            .OrderBy(camera => camera.gameObject.GetInstanceID())
            .ToList();
    }

    private static Camera FindSingleNamedCamera(string name)
    {
        var cameras = FindNamedCameras(name);
        return cameras.Count == 1 ? cameras[0] : null;
    }

    private static void WriteReports(CaptureResult result)
    {
        Directory.CreateDirectory(result.OutputDirectory);
        result.ReportPathRelative = result.OutputDirectoryRelative + "/" + result.Timestamp + "-capture-report.md";
        result.MetadataPathRelative = result.OutputDirectoryRelative + "/" + result.Timestamp + "-capture-metadata.json";
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(Path.Combine(result.RepoRoot, result.ReportPathRelative), result.ToMarkdown(), utf8NoBom);
        File.WriteAllText(Path.Combine(result.RepoRoot, result.MetadataPathRelative), result.ToJson(), utf8NoBom);
    }

    private static void TryWriteReports(CaptureResult result)
    {
        try
        {
            WriteReports(result);
        }
        catch (Exception exception)
        {
            Debug.LogError("MYB-145 failed to write capture reports: " + exception);
        }
    }

    private static void OpenSceneIfProvided(CaptureOptions options)
    {
        if (options == null || string.IsNullOrWhiteSpace(options.ScenePath))
        {
            return;
        }

        EditorSceneManager.OpenScene(options.ScenePath, OpenSceneMode.Single);
    }

    private static string ReadCommandLineValue(string key, string fallback)
    {
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return fallback;
    }

    private static string GetGitValue(string command)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo("git", command)
            {
                WorkingDirectory = GetRepoRoot(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var process = System.Diagnostics.Process.Start(startInfo))
            {
                if (process == null)
                {
                    return string.Empty;
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(2000);
                return process.ExitCode == 0 ? output : string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ToFullPath(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.Combine(GetRepoRoot(), path);
    }

    private static string ToProjectRelativePath(string fullPath)
    {
        var repoRoot = GetRepoRoot().Replace('\\', '/').TrimEnd('/') + "/";
        var normalized = fullPath.Replace('\\', '/');
        return normalized.StartsWith(repoRoot, StringComparison.OrdinalIgnoreCase)
            ? normalized.Substring(repoRoot.Length)
            : normalized;
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
    }

    private static string FormatVector(Vector3 value)
    {
        return "(" + FormatFloat(value.x) + ", " + FormatFloat(value.y) + ", " + FormatFloat(value.z) + ")";
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string EscapeMarkdown(string value)
    {
        return (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", "<br>");
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static void LogResult(CaptureResult result)
    {
        var summary = "MYB-145 " + result.Verdict + ": " + result.ErrorCount + " errors, " + result.WarningCount + " warnings, " + result.InfoCount + " info. Report: " + result.ReportPathRelative;
        if (result.ErrorCount > 0)
        {
            Debug.LogError(summary);
        }
        else if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
        }
        else
        {
            Debug.Log(summary);
        }
    }

    public sealed class CaptureOptions
    {
        public string TicketId = "MYB-145";
        public string State = "current";
        public string ScenePath = string.Empty;
        public string BeforeRoutePath = string.Empty;
        public string AfterRoutePath = string.Empty;
        public string BeforeOverviewPath = string.Empty;
        public string AfterOverviewPath = string.Empty;
        public string BaselineSelectedBy = string.Empty;
        public string BaselineReason = string.Empty;
        public string BaselineSource = string.Empty;

        public static CaptureOptions Default()
        {
            return new CaptureOptions();
        }

        public static CaptureOptions FromCommandLine()
        {
            return new CaptureOptions
            {
                TicketId = ReadCommandLineValue("-myb145Ticket", "MYB-145"),
                State = ReadCommandLineValue("-myb145State", "current"),
                ScenePath = ReadCommandLineValue("-myb145Scene", string.Empty),
                BeforeRoutePath = ReadCommandLineValue("-myb145BeforeRoute", string.Empty),
                AfterRoutePath = ReadCommandLineValue("-myb145AfterRoute", string.Empty),
                BeforeOverviewPath = ReadCommandLineValue("-myb145BeforeOverview", string.Empty),
                AfterOverviewPath = ReadCommandLineValue("-myb145AfterOverview", string.Empty),
                BaselineSelectedBy = ReadCommandLineValue("-myb145BaselineSelectedBy", string.Empty),
                BaselineReason = ReadCommandLineValue("-myb145BaselineReason", string.Empty),
                BaselineSource = ReadCommandLineValue("-myb145BaselineSource", string.Empty)
            };
        }
    }

    public sealed class CaptureResult
    {
        public string ExecutionMode;
        public string Mode;
        public string TicketId;
        public string State;
        public string GeneratedAt;
        public string Timestamp;
        public string RepoRoot;
        public string ScenePath;
        public string SceneName;
        public string Branch;
        public string Commit;
        public string OutputDirectory;
        public string OutputDirectoryRelative;
        public string ReportPathRelative;
        public string MetadataPathRelative;
        public string ExplicitBaselineSelectedBy;
        public string ExplicitBaselineReason;
        public string ExplicitBaselineSource;
        public readonly List<CameraRecord> Cameras = new List<CameraRecord>();
        public readonly List<CaptureRecord> Captures = new List<CaptureRecord>();
        public readonly List<ComparisonRecord> Comparisons = new List<ComparisonRecord>();
        public readonly List<MessageRecord> Errors = new List<MessageRecord>();
        public readonly List<MessageRecord> Warnings = new List<MessageRecord>();
        public readonly List<MessageRecord> Info = new List<MessageRecord>();

        public int ErrorCount => Errors.Count;
        public int WarningCount => Warnings.Count;
        public int InfoCount => Info.Count;
        public string Verdict => ErrorCount > 0 ? "FAIL" : WarningCount > 0 ? "PASS_WITH_WARNINGS" : "PASS";

        public void AddError(string code, string message)
        {
            Errors.Add(new MessageRecord { Code = code, Message = message });
        }

        public void AddWarning(string code, string message)
        {
            Warnings.Add(new MessageRecord { Code = code, Message = message });
        }

        public void AddInfo(string code, string message)
        {
            Info.Add(new MessageRecord { Code = code, Message = message });
        }

        public string ToMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine("# MYB-145 Capture Report");
            builder.AppendLine();
            builder.AppendLine("Ticket:");
            builder.AppendLine("- `" + TicketId + "`");
            builder.AppendLine();
            builder.AppendLine("Generated at:");
            builder.AppendLine("- " + GeneratedAt);
            builder.AppendLine();
            builder.AppendLine("Output directory:");
            builder.AppendLine("- `" + OutputDirectoryRelative + "/`");
            builder.AppendLine();
            builder.AppendLine("Metadata:");
            builder.AppendLine("- `" + MetadataPathRelative + "`");
            builder.AppendLine();
            builder.AppendLine("Mode:");
            builder.AppendLine("- " + Mode);
            builder.AppendLine();
            builder.AppendLine("State:");
            builder.AppendLine("- " + State);
            builder.AppendLine();
            builder.AppendLine("Execution:");
            builder.AppendLine("- Mode: " + ExecutionMode);
            builder.AppendLine("- Branch: `" + Branch + "`");
            builder.AppendLine("- Commit: `" + Commit + "`");
            builder.AppendLine();
            builder.AppendLine("## Scene");
            builder.AppendLine();
            builder.AppendLine("Scene:");
            builder.AppendLine("- `" + ScenePath + "`");
            builder.AppendLine();
            builder.AppendLine("## Cameras");
            builder.AppendLine();
            builder.AppendLine("| Camera | Role | Found | Position | Rotation | FOV/Size | Notes |");
            builder.AppendLine("|---|---|---:|---|---|---:|---|");
            foreach (var camera in Cameras)
            {
                builder.AppendLine("| " + EscapeMarkdown(camera.Name) + " | " + EscapeMarkdown(camera.Role) + " | " + (camera.Found ? "Yes" : "No") + " | " + EscapeMarkdown(camera.Position) + " | " + EscapeMarkdown(camera.Rotation) + " | " + FormatFloat(camera.Fov) + " | " + EscapeMarkdown(camera.Notes) + " |");
            }

            builder.AppendLine();
            builder.AppendLine("## Captures");
            builder.AppendLine();
            builder.AppendLine("| State | Type | Path | Scene | Camera | Resolution |");
            builder.AppendLine("|---|---|---|---|---|---|");
            foreach (var capture in Captures)
            {
                builder.AppendLine("| " + EscapeMarkdown(capture.State) + " | " + EscapeMarkdown(capture.Type) + " | `" + EscapeMarkdown(capture.Path) + "` | `" + EscapeMarkdown(capture.Scene) + "` | " + EscapeMarkdown(capture.Camera) + " | " + EscapeMarkdown(capture.Resolution) + " |");
            }

            builder.AppendLine();
            builder.AppendLine("## Comparisons");
            builder.AppendLine();
            builder.AppendLine("| Type | Before | After | Sheet |");
            builder.AppendLine("|---|---|---|---|");
            foreach (var comparison in Comparisons)
            {
                builder.AppendLine("| " + EscapeMarkdown(comparison.Type) + " | `" + EscapeMarkdown(comparison.BeforePath) + "` | `" + EscapeMarkdown(comparison.AfterPath) + "` | `" + EscapeMarkdown(comparison.SheetPath) + "` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Explicit Baseline");
            builder.AppendLine();
            builder.AppendLine("Before selected by:");
            builder.AppendLine("- " + (string.IsNullOrWhiteSpace(ExplicitBaselineSelectedBy) ? "(not provided)" : ExplicitBaselineSelectedBy));
            builder.AppendLine();
            builder.AppendLine("Reason:");
            builder.AppendLine("- " + (string.IsNullOrWhiteSpace(ExplicitBaselineReason) ? "(not provided)" : ExplicitBaselineReason));
            builder.AppendLine();
            builder.AppendLine("Source:");
            builder.AppendLine("- " + (string.IsNullOrWhiteSpace(ExplicitBaselineSource) ? "(not provided)" : ExplicitBaselineSource));
            builder.AppendLine();
            AppendMessages(builder, "Errors", Errors);
            AppendMessages(builder, "Warnings", Warnings);
            AppendMessages(builder, "Info", Info);
            builder.AppendLine("## Verdict");
            builder.AppendLine();
            builder.AppendLine("- " + Verdict);
            builder.AppendLine();
            builder.AppendLine("RouteCamera is the blocking validation camera. OverviewCamera is required as secondary context. MYB-145 does not judge Premium target.");
            return builder.ToString();
        }

        public string ToJson()
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            builder.AppendLine("  \"schemaVersion\": " + MetadataSchemaVersion + ",");
            builder.AppendLine("  \"ticketId\": \"" + EscapeJson(TicketId) + "\",");
            builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(GeneratedAt) + "\",");
            builder.AppendLine("  \"mode\": \"" + EscapeJson(Mode) + "\",");
            builder.AppendLine("  \"state\": \"" + EscapeJson(State) + "\",");
            builder.AppendLine("  \"scenePath\": \"" + EscapeJson(ScenePath) + "\",");
            builder.AppendLine("  \"branch\": \"" + EscapeJson(Branch) + "\",");
            builder.AppendLine("  \"commit\": \"" + EscapeJson(Commit) + "\",");
            builder.AppendLine("  \"outputDirectory\": \"" + EscapeJson(OutputDirectoryRelative) + "\",");
            builder.AppendLine("  \"verdict\": \"" + Verdict + "\",");
            AppendJsonArray(builder, "cameras", Cameras.Select(camera => camera.ToJson()), comma: true);
            AppendJsonArray(builder, "captures", Captures.Select(capture => capture.ToJson()), comma: true);
            AppendJsonArray(builder, "comparisons", Comparisons.Select(comparison => comparison.ToJson()), comma: true);
            builder.AppendLine("  \"explicitBaseline\": {");
            builder.AppendLine("    \"selectedBy\": \"" + EscapeJson(ExplicitBaselineSelectedBy) + "\",");
            builder.AppendLine("    \"reason\": \"" + EscapeJson(ExplicitBaselineReason) + "\",");
            builder.AppendLine("    \"source\": \"" + EscapeJson(ExplicitBaselineSource) + "\"");
            builder.AppendLine("  },");
            AppendJsonArray(builder, "errors", Errors.Select(error => error.ToJson()), comma: true);
            AppendJsonArray(builder, "warnings", Warnings.Select(warning => warning.ToJson()), comma: true);
            AppendJsonArray(builder, "info", Info.Select(info => info.ToJson()), comma: false);
            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendMessages(StringBuilder builder, string title, IReadOnlyList<MessageRecord> messages)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            if (messages.Count == 0)
            {
                builder.AppendLine("- None");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("| Code | Message |");
            builder.AppendLine("|---|---|");
            foreach (var message in messages)
            {
                builder.AppendLine("| " + EscapeMarkdown(message.Code) + " | " + EscapeMarkdown(message.Message) + " |");
            }
            builder.AppendLine();
        }

        private static void AppendJsonArray(StringBuilder builder, string name, IEnumerable<string> items, bool comma)
        {
            var list = items.ToList();
            builder.AppendLine("  \"" + name + "\": [");
            for (var i = 0; i < list.Count; i++)
            {
                builder.Append(list[i]);
                builder.AppendLine(i == list.Count - 1 ? string.Empty : ",");
            }
            builder.AppendLine("  ]" + (comma ? "," : string.Empty));
        }
    }

    public sealed class CameraRecord
    {
        public string Name;
        public string Role;
        public bool Found;
        public string Position = string.Empty;
        public string Rotation = string.Empty;
        public float Fov;
        public string Notes = string.Empty;

        public static CameraRecord Missing(string name, string role)
        {
            return new CameraRecord { Name = name, Role = role, Found = false, Notes = "missing" };
        }

        public string ToJson()
        {
            return "    { \"name\": \"" + EscapeJson(Name) + "\", \"role\": \"" + EscapeJson(Role) + "\", \"found\": " + (Found ? "true" : "false") + ", \"position\": \"" + EscapeJson(Position) + "\", \"rotation\": \"" + EscapeJson(Rotation) + "\", \"fov\": " + Fov.ToString(CultureInfo.InvariantCulture) + ", \"notes\": \"" + EscapeJson(Notes) + "\" }";
        }
    }

    public sealed class CaptureRecord
    {
        public string State;
        public string Type;
        public string Path;
        public string Scene;
        public string Camera;
        public string Position;
        public string Rotation;
        public float Fov;
        public string Resolution;

        public string ToJson()
        {
            return "    { \"state\": \"" + EscapeJson(State) + "\", \"type\": \"" + EscapeJson(Type) + "\", \"path\": \"" + EscapeJson(Path) + "\", \"scene\": \"" + EscapeJson(Scene) + "\", \"camera\": \"" + EscapeJson(Camera) + "\", \"position\": \"" + EscapeJson(Position) + "\", \"rotation\": \"" + EscapeJson(Rotation) + "\", \"fov\": " + Fov.ToString(CultureInfo.InvariantCulture) + ", \"resolution\": \"" + EscapeJson(Resolution) + "\" }";
        }
    }

    public sealed class ComparisonRecord
    {
        public string Type;
        public string BeforePath;
        public string AfterPath;
        public string SheetPath;

        public string ToJson()
        {
            return "    { \"type\": \"" + EscapeJson(Type) + "\", \"beforePath\": \"" + EscapeJson(BeforePath) + "\", \"afterPath\": \"" + EscapeJson(AfterPath) + "\", \"sheetPath\": \"" + EscapeJson(SheetPath) + "\" }";
        }
    }

    public sealed class MessageRecord
    {
        public string Code;
        public string Message;

        public string ToJson()
        {
            return "    { \"code\": \"" + EscapeJson(Code) + "\", \"message\": \"" + EscapeJson(Message) + "\" }";
        }
    }
}
