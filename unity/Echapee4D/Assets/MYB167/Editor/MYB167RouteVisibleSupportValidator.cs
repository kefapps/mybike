using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MYB167RouteVisibleSupportValidator
{
    private const string MenuPath = "Tools/MyBike/Validation/MYB-167 Route-Visible Support Validator";
    private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
    private const string ImplementationRootRelative = "_bmad-output/implementation-artifacts/MYB-167";
    private const string ReportRelativePath = ImplementationRootRelative + "/myb-167-route-visible-support-report.md";
    private const string MetricsRelativePath = ImplementationRootRelative + "/myb-167-route-visible-support-metrics.json";
    private const string Myb144ReportRelativePath = "_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md";
    private const float RouteSampleSpacingMeters = 24f;
    private const float RouteVisibleFloatingWarningClearanceMeters = 0.05f;
    private const float RouteVisibleFloatingBlockingClearanceMeters = 0.10f;
    private const float GroundedBottomToleranceMeters = 0.12f;
    private const float SupportRequiredBottomClearanceMeters = 0.45f;
    private const float SupportCandidateMaxBottomClearanceMeters = 0.58f;
    private const float BaseSupportVerticalToleranceMeters = 0.35f;
    private const float BaseSupportFootprintPaddingMeters = 0.55f;
    private const float SupportVerticalGapMeters = 0.85f;
    private const float SupportFootprintPaddingMeters = 0.85f;
    private const float SupportCoVisibleSampleToleranceMeters = 0.25f;
    private const float BlockingVisibleDistanceMeters = 120f;
    private const float RouteCorridorWarningHalfWidthMeters = 4.2f;
    private const float RouteCorridorBlockingClearanceMeters = 0f;
    private const float RouteCameraSafetyEstimatedMetersPerSecond = 12.5f;
    private const float ReadabilityBlockingDominanceRatio = 0.12f;
    private const float ReadabilityWarningDominanceRatio = 0.07f;
    private const float ReadabilityBlockingProtectedOverlapRatio = 0.18f;
    private const float ReadabilityWarningProtectedOverlapRatio = 0.10f;
    private const float ElevatedRouteOcclusionBlockingDominanceRatio = 0.10f;
    private const float ElevatedRouteOcclusionBlockingOverlapRatio = 0.18f;
    private const float ElevatedRouteOcclusionCorridorClearanceMeters = 2.8f;
    private const float ElevatedRouteOcclusionHardCorridorClearanceMeters = 0.75f;
    private const float ElevatedRouteOcclusionHardDominanceRatio = 0.18f;
    private const float CloseScenicFramingProtectedOverlapLimit = 0.16f;
    private const float CloseScenicFramingElevatedOverlapLimit = 0.16f;
    private const float CloseScenicFramingDominanceLimit = 0.20f;

    private static readonly Rect ProtectedRouteReadabilityViewport = new Rect(0.18f, 0.20f, 0.64f, 0.58f);
    private static readonly Rect ElevatedRouteOcclusionViewport = new Rect(0.12f, 0.56f, 0.76f, 0.32f);

    private static readonly AllowlistRule[] AllowlistRules =
    {
        new AllowlistRule("hud-or-preview-ui", "Hierarchy contains HUD, Canvas, EventSystem, or MYB73 route preview UI."),
        new AllowlistRule("bike-pov-cockpit-system", "Hierarchy contains MYB165_BikePOVCues or camera-attached cockpit support cues."),
        new AllowlistRule("intended-gate-or-signage", "Hierarchy explicitly marks a route gate or signage object as intended route furniture."),
        new AllowlistRule("gameplay-cue", "Hierarchy explicitly marks gameplay-readable cue geometry."),
        new AllowlistRule("capture-helper", "Hierarchy explicitly marks screenshot/video capture helper geometry."),
        new AllowlistRule("route-line-renderer", "LineRenderer route/debug/difficulty cue surfaces are not scenic support assets."),
        new AllowlistRule("route-ground-surface", "Named road, route, edge, shoulder, floor, leaf, or moss ground surfaces are route/ground context.")
    };

    [MenuItem(MenuPath)]
    public static void RunFromMenu()
    {
        var result = RunValidation("Menu");
        var summary = result.ToConsoleSummary();
        if (result.HasBlockingFailure)
        {
            Debug.LogError(summary);
            return;
        }

        if (result.WarningCount > 0)
        {
            Debug.LogWarning(summary);
            return;
        }

        Debug.Log(summary);
    }

    public static void RunBatchValidate()
    {
        var result = RunValidation("Batch");
        if (result.HasBlockingFailure)
        {
            throw new InvalidOperationException(result.ToConsoleSummary());
        }

        Debug.Log(result.ToConsoleSummary());
    }

    public static ValidationResult RunValidation()
    {
        return RunValidation("Unknown");
    }

    public static ValidationResult RunValidation(string executionMode)
    {
        var result = new ValidationResult
        {
            ExecutionMode = string.IsNullOrWhiteSpace(executionMode) ? "Unknown" : executionMode,
            GeneratedAt = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
            RepoRoot = GetRepoRoot(),
            ScenePath = ScenePath,
            ReportPathRelative = ReportRelativePath,
            MetricsPathRelative = MetricsRelativePath,
            Myb144ReportPathRelative = Myb144ReportRelativePath,
            FixedPrefixOnlyDetectionUsed = false,
            AllowlistEntryCount = AllowlistRules.Length,
            AllowlistEntriesDocumented = true,
            GroundSource = "local generated forest floor / same-assembly base support when available; nearest smoothed route sample y fallback"
        };

        Directory.CreateDirectory(ToRepoPath(ImplementationRootRelative));

        try
        {
            OpenTargetScene(result);
            AnalyzeScene(result);
            RunMyb144(result);
        }
        catch (Exception exception)
        {
            result.AddBlocking(
                "VALIDATOR_EXCEPTION",
                "MYB-167 validator",
                exception.GetType().FullName + ": " + exception.Message,
                "Fix the validator exception before trusting route-visible support output.");
            result.AddInfo("VALIDATOR_EXCEPTION_DETAIL", exception.ToString());
        }

        try
        {
            WriteMetricsJson(result);
            WriteReport(result);
        }
        catch (Exception exception)
        {
            result.AddBlocking(
                "REPORT_WRITE_FAILED",
                ReportRelativePath,
                exception.GetType().FullName + ": " + exception.Message,
                "Ensure the MYB-167 implementation artifact directory is writable.");
        }

        return result;
    }

    private static void OpenTargetScene(ValidationResult result)
    {
        var fullPath = ToProjectPath(ScenePath);
        if (!File.Exists(fullPath))
        {
            result.AddBlocking(
                "SCENE_MISSING",
                ScenePath,
                "Canonical validation scene is missing.",
                "Build MYB-165/MYB-89 before running MYB-167.");
            return;
        }

        var active = SceneManager.GetActiveScene();
        if (!string.Equals(active.path, ScenePath, StringComparison.Ordinal))
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        result.ScenePath = SceneManager.GetActiveScene().path;
        result.SceneDirtyBeforeValidation = SceneManager.GetActiveScene().isDirty;
    }

    private static void AnalyzeScene(ValidationResult result)
    {
        var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
        if (ride == null)
        {
            result.AddBlocking(
                "RIDE_MISSING",
                "MYB89ProbeRide",
                "No canonical ride component found in the scene.",
                "Build the canonical ride scene before validating route-visible support.");
            return;
        }

        var camera = FindRouteCamera(ride);
        if (camera == null)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_MISSING",
                "Camera.main / ride.cameraPivot",
                "No route camera could be resolved for route-visible support validation.",
                "Restore the canonical bike POV camera before running MYB-167.");
            return;
        }

        ride.RebuildRouteCache();
        var route = MYB89RideTrajectory.BuildSmoothedPoints(ride.routeMarkers, ride.trajectorySamplesPerSegment);
        var routeLength = MYB89RideTrajectory.Length(route);
        result.RouteLengthMeters = routeLength;
        result.RoutePointCount = route.Length;

        if (route.Length < 2 || routeLength <= 0.01f)
        {
            result.AddBlocking(
                "ROUTE_MISSING",
                "MYB89ProbeRide.routeMarkers",
                "The ride route has fewer than two usable points.",
                "Restore route markers before validating route-visible support.");
            return;
        }

        result.AddInfo(
            "GROUND_SOURCE_APPROXIMATE",
            "MYB-167 prefers known local/generated ground and same-assembly base support, but still falls back to nearest route Y when no better local ground source exists. Keep MYB-154 bottomClearance/raycast checks for exact placement where exact placement is required.");

        var snapshots = BuildRouteCameraSnapshots(result, ride, camera, route, routeLength);
        if (snapshots.Count == 0)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAMPLES_MISSING",
                "route-camera sampling",
                "No route-camera frustum samples were produced.",
                "Check the route camera and ride preview pose.");
            return;
        }

        var records = CollectRendererRecords(result, route);
        ApplyRouteVisibility(records, snapshots, route);
        EvaluateSceneRecords(result, records);
        RunRegressionFixture(result, route, snapshots);
        RunRouteCameraSafetyFixture(result, route, snapshots);
        result.SceneDirtyAfterValidation = SceneManager.GetActiveScene().isDirty;
    }

    private static Camera FindRouteCamera(MYB89ProbeRide ride)
    {
        if (ride.cameraPivot != null && ride.cameraPivot.TryGetComponent<Camera>(out var rideCamera))
        {
            return rideCamera;
        }

        if (Camera.main != null)
        {
            return Camera.main;
        }

        var routeCameraObject = GameObject.Find("RouteCamera");
        return routeCameraObject == null ? null : routeCameraObject.GetComponent<Camera>();
    }

    private static List<RouteCameraSnapshot> BuildRouteCameraSnapshots(
        ValidationResult result,
        MYB89ProbeRide ride,
        Camera camera,
        IReadOnlyList<Vector3> route,
        float routeLength)
    {
        var snapshots = new List<RouteCameraSnapshot>();
        var previousProgress = ride.progressMeters;
        var previousAutoplay = ride.autoplay;
        var previousWaitForRoutePreview = ride.waitForRoutePreview;

        try
        {
            ride.autoplay = false;
            ride.waitForRoutePreview = false;
            var sampleCount = Mathf.Max(2, Mathf.CeilToInt(routeLength / RouteSampleSpacingMeters) + 1);
            for (var i = 0; i < sampleCount; i++)
            {
                var meters = Mathf.Min(routeLength - 0.1f, i * RouteSampleSpacingMeters);
                if (i == sampleCount - 1)
                {
                    meters = Mathf.Max(0f, routeLength - 0.1f);
                }

                ride.SetPreviewProgress(meters);
                Canvas.ForceUpdateCanvases();

                if (!MYB89RideTrajectory.TrySample(route, meters, false, out _))
                {
                    continue;
                }

                snapshots.Add(new RouteCameraSnapshot
                {
                    Meters = meters,
                    CameraPosition = camera.transform.position,
                    CameraForward = camera.transform.forward,
                    NearClipPlane = camera.nearClipPlane,
                    FrustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera),
                    WorldToCameraMatrix = camera.worldToCameraMatrix,
                    ProjectionMatrix = camera.projectionMatrix
                });
            }
        }
        finally
        {
            ride.autoplay = previousAutoplay;
            ride.waitForRoutePreview = previousWaitForRoutePreview;
            ride.SetPreviewProgress(previousProgress);
        }

        result.RouteCameraSampleCount = snapshots.Count;
        return snapshots;
    }

    private static List<RendererRecord> CollectRendererRecords(ValidationResult result, IReadOnlyList<Vector3> route)
    {
        var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude)
            .Where(renderer => renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
            .OrderBy(renderer => PathOf(renderer.gameObject), StringComparer.Ordinal)
            .ToArray();

        result.TotalRendererCount = renderers.Length;
        var records = new List<RendererRecord>(renderers.Length);
        foreach (var renderer in renderers)
        {
            var path = PathOf(renderer.gameObject);
            var bounds = renderer.bounds;
            var groundY = EstimateGroundY(route, bounds.center);
            var record = new RendererRecord
            {
                Name = renderer.gameObject.name,
                Path = path,
                AssetKey = AssetKeyForPath(path),
                RendererType = renderer.GetType().Name,
                Bounds = bounds,
                GroundY = groundY,
                RouteGroundY = groundY,
                BottomClearance = bounds.min.y - groundY,
                RouteBottomClearance = bounds.min.y - groundY,
                GroundSource = "nearest-route-y",
                UpFacingSurfaceRatio = UpFacingSurfaceRatioFor(renderer),
                ExclusionReason = ExclusionReasonFor(renderer, path),
                Synthetic = false
            };

            records.Add(record);
        }

        ApplyLocalGroundEstimates(records);
        return records;
    }

    private static void ApplyLocalGroundEstimates(IReadOnlyList<RendererRecord> records)
    {
        foreach (var record in records)
        {
            if (record.Synthetic)
            {
                continue;
            }

            if (!TryEstimateKnownGeneratedLocalGround(record, out var localGroundY, out var groundSource))
            {
                continue;
            }

            record.GroundY = localGroundY;
            record.BottomClearance = record.Bounds.min.y - localGroundY;
            record.GroundSource = groundSource;
        }
    }

    private static bool TryEstimateKnownGeneratedLocalGround(
        RendererRecord record,
        out float groundY,
        out string groundSource)
    {
        if (TryMyb163TerrainHeightForPath(record.Path, out var terrainHeight))
        {
            groundY = record.RouteGroundY + terrainHeight;
            groundSource = "myb-163-local-forest-floor";
            return true;
        }

        groundY = record.GroundY;
        groundSource = record.GroundSource;
        return false;
    }

    private static bool TryMyb163TerrainHeightForPath(string path, out float terrainHeight)
    {
        if (path.Contains("MYB163_TreeAssembly_CloseLeftFrame"))
        {
            terrainHeight = Myb163TerrainHeight(9.8f, -6.85f);
            return true;
        }

        if (path.Contains("MYB163_TreeAssembly_MidLeftEnclosure"))
        {
            terrainHeight = Myb163TerrainHeight(20.5f, -7.75f);
            return true;
        }

        if (path.Contains("MYB163_TreeAssembly_RightAnchor"))
        {
            terrainHeight = Myb163TerrainHeight(24.0f, 7.35f);
            return true;
        }

        if (path.Contains("MYB163_PremiumTreeAnchor_CloseLeft_A"))
        {
            terrainHeight = Myb163TerrainHeight(14.0f, -6.25f);
            return true;
        }

        if (path.Contains("MYB163_PremiumTreeAnchor_Right_B"))
        {
            terrainHeight = Myb163TerrainHeight(18.5f, 6.65f);
            return true;
        }

        if (path.Contains("MYB163_PremiumTreeAnchor_MidLeft_C"))
        {
            terrainHeight = Myb163TerrainHeight(29.0f, -6.85f);
            return true;
        }

        if (path.Contains("MYB163_LeftForestFloorShoulder_A"))
        {
            terrainHeight = Myb163TerrainHeight(12.2f, -6.15f);
            return true;
        }

        if (path.Contains("MYB163_LeftForestFloorShoulder_B"))
        {
            terrainHeight = Myb163TerrainHeight(19.0f, -6.35f);
            return true;
        }

        if (path.Contains("MYB163_RightLowMossBank_A"))
        {
            terrainHeight = Myb163TerrainHeight(13.8f, 5.85f);
            return true;
        }

        if (path.Contains("MYB163_RightLowMossBank_B"))
        {
            terrainHeight = Myb163TerrainHeight(22.0f, 6.05f);
            return true;
        }

        if (path.Contains("MYB163_RightForegroundRootCluster"))
        {
            terrainHeight = Myb163TerrainHeight(12.5f, 5.35f);
            return true;
        }

        if (path.Contains("MYB163_LeftGroundingRootCluster"))
        {
            terrainHeight = Myb163TerrainHeight(22.5f, -5.65f);
            return true;
        }

        if (path.Contains("MYB163_RightMossRockMarker"))
        {
            terrainHeight = Myb163TerrainHeight(17.0f, 5.75f);
            return true;
        }

        if (path.Contains("MYB163_RootThresholdHero") || path.Contains("MYB163_HeroGroundingHalo"))
        {
            terrainHeight = Myb163TerrainHeight(34.0f, -6.05f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_00"))
        {
            terrainHeight = Myb163TerrainHeight(28.0f, -9.4f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_01"))
        {
            terrainHeight = Myb163TerrainHeight(31.0f, 9.8f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_02"))
        {
            terrainHeight = Myb163TerrainHeight(39.0f, -10.4f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_03"))
        {
            terrainHeight = Myb163TerrainHeight(43.0f, 10.7f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_04"))
        {
            terrainHeight = Myb163TerrainHeight(52.0f, -11.2f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_05"))
        {
            terrainHeight = Myb163TerrainHeight(58.0f, 11.6f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_06"))
        {
            terrainHeight = Myb163TerrainHeight(69.0f, -12.6f);
            return true;
        }

        if (path.Contains("MYB163_GroupedBackWallMass_07"))
        {
            terrainHeight = Myb163TerrainHeight(74.0f, 12.2f);
            return true;
        }

        if (path.Contains("MYB163_SoftBackground_L_00"))
        {
            terrainHeight = Myb163TerrainHeight(82.0f, -15.0f);
            return true;
        }

        if (path.Contains("MYB163_SoftBackground_R_00"))
        {
            terrainHeight = Myb163TerrainHeight(86.0f, 14.6f);
            return true;
        }

        if (path.Contains("MYB163_SoftBackground_L_01"))
        {
            terrainHeight = Myb163TerrainHeight(96.0f, -16.2f);
            return true;
        }

        if (path.Contains("MYB163_SoftBackground_R_01"))
        {
            terrainHeight = Myb163TerrainHeight(102.0f, 15.8f);
            return true;
        }

        terrainHeight = 0f;
        return false;
    }

    private static float Myb163TerrainHeight(float meters, float offset)
    {
        var abs = Mathf.Abs(offset);
        if (abs < 2.1f)
        {
            return Mathf.Sin(meters * 0.04f) * 0.015f - abs * 0.002f;
        }

        if (abs < 3.5f)
        {
            return 0.025f + abs * 0.006f + Mathf.Sin(meters * 0.11f + offset) * 0.01f;
        }

        if (abs < 11.4f)
        {
            return 0.06f + abs * 0.010f + Mathf.Sin(meters * 0.08f + offset * 0.4f) * 0.035f;
        }

        return 0.12f + abs * 0.018f + Mathf.Sin(meters * 0.055f + offset * 0.25f) * 0.07f;
    }

    private static void ApplyRouteVisibility(
        IReadOnlyList<RendererRecord> records,
        IReadOnlyList<RouteCameraSnapshot> snapshots,
        IReadOnlyList<Vector3> route)
    {
        foreach (var record in records)
        {
            record.RouteCorridorClearanceMeters = MinRouteCorridorClearance(record.Bounds, route);
            foreach (var snapshot in snapshots)
            {
                if (!GeometryUtility.TestPlanesAABB(snapshot.FrustumPlanes, record.Bounds))
                {
                    continue;
                }

                var distance = Vector3.Distance(snapshot.CameraPosition, record.Bounds.center);
                var screenRect = Rect.zero;
                var dominanceRatio = 0f;
                var protectedOverlapRatio = 0f;
                var elevatedRouteOverlapRatio = 0f;
                if (TryProjectBounds(snapshot, record.Bounds, out var projectedRect))
                {
                    screenRect = projectedRect;
                    dominanceRatio = Area(projectedRect);
                    protectedOverlapRatio = Area(Intersect(projectedRect, ProtectedRouteReadabilityViewport))
                        / Mathf.Max(0.001f, Area(ProtectedRouteReadabilityViewport));
                    elevatedRouteOverlapRatio = Area(Intersect(projectedRect, ElevatedRouteOcclusionViewport))
                        / Mathf.Max(0.001f, Area(ElevatedRouteOcclusionViewport));
                }

                record.Observe(snapshot.Meters, distance, screenRect, dominanceRatio, protectedOverlapRatio, elevatedRouteOverlapRatio);
            }
        }
    }

    private static void EvaluateSceneRecords(ValidationResult result, IReadOnlyList<RendererRecord> records)
    {
        var routeVisible = records.Where(record => record.RouteVisible).ToList();
        result.RouteVisibleRendererCount = routeVisible.Count;
        result.RouteVisibleAssetCount = routeVisible
            .Select(record => record.AssetKey)
            .Distinct(StringComparer.Ordinal)
            .Count();

        foreach (var record in routeVisible)
        {
            var finding = EvaluateRecord(record, records, false);
            result.Findings.Add(finding);
            ApplyFindingToMetrics(result, finding);
        }

        EvaluateRouteCameraSafety(result, routeVisible);

        result.MaxUnsupportedBottomClearance = result.Findings
            .Where(finding => finding.IsUnsupported)
            .Select(finding => finding.BottomClearance)
            .DefaultIfEmpty(0f)
            .Max();
        result.MaxUnsupportedVisibleDistanceMeters = result.Findings
            .Where(finding => finding.IsUnsupported)
            .Select(finding => finding.MinVisibleDistanceMeters)
            .DefaultIfEmpty(0f)
            .Max();

        foreach (var finding in result.Findings.Where(finding => finding.Severity == FindingSeverity.Warning))
        {
            result.AddWarning(
                "UNSUPPORTED_ROUTE_VISIBLE_AMBIGUOUS",
                finding.Path,
                finding.Reason
                + ". bottomClearance="
                + FormatFloat(finding.BottomClearance)
                + "m, groundSource="
                + finding.GroundSource
                + ", minVisibleDistance="
                + FormatFloat(finding.MinVisibleDistanceMeters)
                + "m.",
                "Inspect from route camera and either add support, document an exception, or tune the validator if this is a false positive.");
        }

        foreach (var finding in result.Findings.Where(finding => finding.Severity == FindingSeverity.Blocking))
        {
            result.AddBlocking(
                "UNSUPPORTED_ROUTE_VISIBLE_ASSET",
                finding.Path,
                finding.Reason
                + ". bottomClearance="
                + FormatFloat(finding.BottomClearance)
                + "m, groundSource="
                + finding.GroundSource
                + ", minVisibleDistance="
                + FormatFloat(finding.MinVisibleDistanceMeters)
                + "m.",
                "Ground the asset visually, add trunk/post/branch/wall support, or document a human-accepted exception.");
        }
    }

    private static SupportFinding EvaluateRecord(
        RendererRecord record,
        IReadOnlyList<RendererRecord> allRecords,
        bool fixtureMode)
    {
        if (!string.IsNullOrEmpty(record.ExclusionReason))
        {
            return new SupportFinding(record, FindingSeverity.Excluded, "excluded: " + record.ExclusionReason);
        }

        var floatingSupportFinding = EvaluateFloatingVerticalSupport(record, allRecords);
        if (floatingSupportFinding != null)
        {
            return floatingSupportFinding;
        }

        if (!RequiresSupport(record))
        {
            return new SupportFinding(record, FindingSeverity.GroundedPass, "grounded or low visual-bottom pass");
        }

        var support = FindSupport(record, allRecords);
        if (support != null)
        {
            var coVisibleMeters = FirstCoVisibleRouteMeters(record, support);
            var finding = new SupportFinding(
                record,
                FindingSeverity.SupportedPass,
                "co-visible support found: " + support.Path + " at route m " + FormatFloat(coVisibleMeters));
            finding.NearestSupportPath = support.Path;
            finding.SupportHorizontalGapMeters = HorizontalGap(record.Bounds, support.Bounds);
            finding.SupportVerticalGapMeters = Mathf.Max(0f, record.Bounds.min.y - support.Bounds.max.y);
            finding.SupportCoVisibleMeters = coVisibleMeters;
            return finding;
        }

        var severity = !fixtureMode && record.MinVisibleDistanceMeters > BlockingVisibleDistanceMeters
            ? FindingSeverity.Warning
            : FindingSeverity.Blocking;
        return new SupportFinding(record, severity, "support required but no ground-connected vertical support found");
    }

    private static SupportFinding EvaluateFloatingVerticalSupport(
        RendererRecord record,
        IReadOnlyList<RendererRecord> allRecords)
    {
        if (!IsVerticalSupportShape(record) || record.BottomClearance <= RouteVisibleFloatingWarningClearanceMeters)
        {
            return null;
        }

        if (IsIntegratedPremiumTreeSubRenderer(record))
        {
            return new SupportFinding(record, FindingSeverity.SupportedPass, "integrated premium tree LOD/moss sub-renderer");
        }

        var baseSupport = FindBaseSupport(record, allRecords);
        if (baseSupport != null)
        {
            var coVisibleMeters = FirstCoVisibleRouteMeters(record, baseSupport);
            var finding = new SupportFinding(
                record,
                FindingSeverity.SupportedPass,
                "co-visible base/root support found: " + baseSupport.Path + " at route m " + FormatFloat(coVisibleMeters));
            finding.NearestSupportPath = baseSupport.Path;
            finding.SupportHorizontalGapMeters = HorizontalGap(record.Bounds, baseSupport.Bounds);
            finding.SupportVerticalGapMeters = Mathf.Max(0f, record.Bounds.min.y - baseSupport.Bounds.max.y);
            finding.SupportCoVisibleMeters = coVisibleMeters;
            return finding;
        }

        var severity = record.BottomClearance > RouteVisibleFloatingBlockingClearanceMeters
            ? FindingSeverity.Blocking
            : FindingSeverity.Warning;
        return new SupportFinding(
            record,
            severity,
            "vertical support-shaped renderer floats above the local ground source; bottomClearance="
            + FormatFloat(record.BottomClearance)
            + "m, groundSource="
            + record.GroundSource);
    }

    private static void ApplyFindingToMetrics(ValidationResult result, SupportFinding finding)
    {
        switch (finding.Severity)
        {
            case FindingSeverity.Excluded:
                result.ExcludedSystemRendererCount++;
                break;
            case FindingSeverity.GroundedPass:
                result.GroundedPassCount++;
                break;
            case FindingSeverity.SupportedPass:
                result.SupportRequiredCount++;
                result.SupportedPassCount++;
                break;
            case FindingSeverity.Warning:
                result.SupportRequiredCount++;
                result.UnsupportedWarningCount++;
                break;
            case FindingSeverity.Blocking:
                result.SupportRequiredCount++;
                result.UnsupportedBlockingCount++;
                break;
        }
    }

    private static void EvaluateRouteCameraSafety(ValidationResult result, IReadOnlyList<RendererRecord> routeVisible)
    {
        foreach (var record in routeVisible)
        {
            var finding = EvaluateRouteCameraSafetyRecord(record);
            if (finding.Severity != FindingSeverity.Excluded)
            {
                result.WorstRouteVisibilityScore = Mathf.Max(result.WorstRouteVisibilityScore, record.WorstVisibilityScore);
            }

            if (finding.Severity != FindingSeverity.Warning && finding.Severity != FindingSeverity.Blocking)
            {
                continue;
            }

            result.RouteCameraSafetyFindings.Add(finding);
            if (string.Equals(finding.Rule, "routeCorridorIntrusion", StringComparison.Ordinal))
            {
                result.RouteCorridorIntrusionCount++;
            }

            if (finding.Severity == FindingSeverity.Warning)
            {
                result.RouteReadabilityWarningCount++;
                result.AddWarning(
                    "ROUTE_CAMERA_SAFETY_WARNING",
                    finding.Path,
                    finding.Reason + " routeMeters="
                    + FormatFloat(finding.RouteMeters)
                    + ", screenRect="
                    + FormatRect(finding.ScreenRect)
                    + ", dominance="
                    + FormatFloat(finding.DominanceRatio)
                    + ", protectedOverlap="
                    + FormatFloat(finding.ProtectedOverlapRatio)
                    + ", elevatedRouteOverlap="
                    + FormatFloat(finding.ElevatedRouteOverlapRatio)
                    + ".",
                    finding.RecommendedAction);
                continue;
            }

            result.RouteReadabilityBlockingCount++;
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_BLOCKER",
                finding.Path,
                finding.Reason + " routeMeters="
                + FormatFloat(finding.RouteMeters)
                + ", screenRect="
                + FormatRect(finding.ScreenRect)
                + ", dominance="
                + FormatFloat(finding.DominanceRatio)
                + ", protectedOverlap="
                + FormatFloat(finding.ProtectedOverlapRatio)
                + ", elevatedRouteOverlap="
                + FormatFloat(finding.ElevatedRouteOverlapRatio)
                + ".",
                finding.RecommendedAction);
        }
    }

    private static RouteCameraSafetyFinding EvaluateRouteCameraSafetyRecord(RendererRecord record)
    {
        if (!string.IsNullOrEmpty(record.ExclusionReason))
        {
            var exemption = RouteCameraSafetyExemptionFor(record);
            if (exemption == RouteCameraSafetyExemptionMode.Full)
            {
                return new RouteCameraSafetyFinding(
                    record,
                    FindingSeverity.Excluded,
                    "exemptRole",
                    "Renderer is explicitly exempt from route-camera safety checks by role and geometry.",
                    "No action.");
            }
        }

        if (record.MaxViewportDominanceRatio >= ReadabilityBlockingDominanceRatio
            && record.MaxProtectedZoneOverlapRatio >= ReadabilityBlockingProtectedOverlapRatio)
        {
            var severity = IsIntentionalRouteFurniture(record)
                ? FindingSeverity.Warning
                : FindingSeverity.Blocking;
            return new RouteCameraSafetyFinding(
                record,
                severity,
                "routeCameraReadabilityBlocker",
                IsIntentionalRouteFurniture(record)
                    ? "Intentional route furniture projected bounds dominate the protected route-camera readability zone."
                    : "Renderer projected bounds dominate the protected route-camera readability zone.",
                IsIntentionalRouteFurniture(record)
                    ? "Inspect the route camera and reduce, move, or document the intentional cue if it compromises route reading."
                    : "Move, shrink, split, support, or remove the asset so it does not cover the route/horizon reading zone.");
        }

        if (!IsIntentionalRouteFurniture(record)
            && record.RouteCorridorClearanceMeters <= ElevatedRouteOcclusionCorridorClearanceMeters
            && record.MaxViewportDominanceRatio >= ElevatedRouteOcclusionBlockingDominanceRatio
            && record.MaxElevatedRouteOverlapRatio >= ElevatedRouteOcclusionBlockingOverlapRatio)
        {
            var severity = record.RouteCorridorClearanceMeters <= ElevatedRouteOcclusionHardCorridorClearanceMeters
                || record.MaxProtectedZoneOverlapRatio >= ReadabilityWarningProtectedOverlapRatio
                || record.MaxViewportDominanceRatio >= ElevatedRouteOcclusionHardDominanceRatio
                    ? FindingSeverity.Blocking
                    : FindingSeverity.Warning;
            return new RouteCameraSafetyFinding(
                record,
                severity,
                "routeCameraReadabilityBlocker",
                severity == FindingSeverity.Blocking
                    ? "Renderer is a real scene elevated occluder over the upper route-camera readability band and also has hard route-camera/corridor evidence."
                    : "Renderer overlaps the upper route-camera readability band but lacks enough hard corridor/protected-zone evidence to block automatically.",
                severity == FindingSeverity.Blocking
                    ? "Move, shrink, split, lower, or remove the elevated mass so it no longer masks the route/horizon from the bike POV."
                    : "Inspect the route camera; keep as authored canopy/enclosure if the road remains readable, otherwise reduce or move it.");
        }

        if (record.RouteCorridorClearanceMeters < RouteCorridorBlockingClearanceMeters)
        {
            if (RouteCameraSafetyExemptionFor(record) == RouteCameraSafetyExemptionMode.CorridorOnly)
            {
                return new RouteCameraSafetyFinding(
                    record,
                    FindingSeverity.Excluded,
                    "corridorExemptRole",
                    "Renderer is intentional route furniture; corridor crossing is allowed but readability checks still apply.",
                    "No action.");
            }

            return new RouteCameraSafetyFinding(
                record,
                FindingSeverity.Blocking,
                "routeCorridorIntrusion",
                "Renderer bounds intrude into the route safety corridor.",
                "Move the renderer outside the route corridor or explicitly convert it into intended route furniture with visual clearance.");
        }

        if (IsAllowedCloseScenicFraming(record))
        {
            return new RouteCameraSafetyFinding(
                record,
                FindingSeverity.GroundedPass,
                "closeScenicFramingPass",
                "Renderer is an authored close forest framing element and does not mask the protected route/horizon bands.",
                "Keep the close forest framing; only tune if route-camera capture shows visual obstruction.");
        }

        if (record.MaxViewportDominanceRatio >= ReadabilityWarningDominanceRatio
            && record.MaxProtectedZoneOverlapRatio >= ReadabilityWarningProtectedOverlapRatio)
        {
            return new RouteCameraSafetyFinding(
                record,
                FindingSeverity.Warning,
                "routeCameraReadabilityBlocker",
                "Renderer projected bounds moderately overlap the protected route-camera readability zone.",
                "Inspect the route camera and tune placement, scale, support, or exception metadata if the object is intentional.");
        }

        if (record.RouteCorridorClearanceMeters < RouteCorridorWarningHalfWidthMeters)
        {
            if (RouteCameraSafetyExemptionFor(record) == RouteCameraSafetyExemptionMode.CorridorOnly)
            {
                return new RouteCameraSafetyFinding(
                    record,
                    FindingSeverity.Excluded,
                    "corridorExemptRole",
                    "Renderer is intentional route furniture; corridor proximity is allowed but readability checks still apply.",
                    "No action.");
            }

            return new RouteCameraSafetyFinding(
                record,
                FindingSeverity.Warning,
                "routeCorridorIntrusion",
                "Renderer bounds are close to the route safety corridor.",
                "Inspect from route camera and move the renderer farther from the ride line unless this is intentional route furniture.");
        }

        return new RouteCameraSafetyFinding(
            record,
            FindingSeverity.GroundedPass,
            "routeCameraSafetyPass",
            "Renderer does not intrude into route corridor or dominate the protected route-camera zone.",
            "No action.");
    }

    private static bool IsAllowedCloseScenicFraming(RendererRecord record)
    {
        if (!IsAuthoredCloseForestFraming(record))
        {
            return false;
        }

        return record.RouteCorridorClearanceMeters >= RouteCorridorBlockingClearanceMeters
            && record.MaxProtectedZoneOverlapRatio < CloseScenicFramingProtectedOverlapLimit
            && record.MaxElevatedRouteOverlapRatio < CloseScenicFramingElevatedOverlapLimit
            && record.MaxViewportDominanceRatio < CloseScenicFramingDominanceLimit;
    }

    private static bool IsAuthoredCloseForestFraming(RendererRecord record)
    {
        var path = record.Path ?? string.Empty;
        return path.IndexOf("MYB163_TreeAssembly", StringComparison.Ordinal) >= 0
            || path.IndexOf("MYB163_PremiumTreeAnchor", StringComparison.Ordinal) >= 0
            || path.IndexOf("MYB163_RootThresholdHero", StringComparison.Ordinal) >= 0
            || path.IndexOf("MYB112_PremiumTree", StringComparison.Ordinal) >= 0
            || path.IndexOf("PremiumTreePolyHaven", StringComparison.Ordinal) >= 0;
    }

    private static bool IsIntegratedPremiumTreeSubRenderer(RendererRecord record)
    {
        var path = record.Path ?? string.Empty;
        if (path.IndexOf("MYB163_PremiumTreeAnchor", StringComparison.Ordinal) < 0
            || record.BottomClearance > SupportCandidateMaxBottomClearanceMeters)
        {
            return false;
        }

        return path.IndexOf("_moss", StringComparison.Ordinal) >= 0
            || path.IndexOf("LOD1_StylizedProxy_Trunk", StringComparison.Ordinal) >= 0
            || path.IndexOf("LOD2_DistantSilhouette_Trunk", StringComparison.Ordinal) >= 0;
    }

    private static RouteCameraSafetyExemptionMode RouteCameraSafetyExemptionFor(RendererRecord record)
    {
        switch (record.ExclusionReason)
        {
            case "hud-or-preview-ui":
            case "bike-pov-cockpit-system":
            case "route-line-renderer":
            case "capture-helper":
            case "route-ground-surface":
                return RouteCameraSafetyExemptionMode.Full;
            case "intended-gate-or-signage":
            case "gameplay-cue":
                return RouteCameraSafetyExemptionMode.CorridorOnly;
            default:
                return RouteCameraSafetyExemptionMode.None;
        }
    }

    private static bool IsIntentionalRouteFurniture(RendererRecord record)
    {
        return string.Equals(record.ExclusionReason, "intended-gate-or-signage", StringComparison.Ordinal)
            || string.Equals(record.ExclusionReason, "gameplay-cue", StringComparison.Ordinal);
    }

    private static bool IsLowFlatRouteSurface(RendererRecord record)
    {
        var horizontalSpan = Mathf.Max(record.Bounds.size.x, record.Bounds.size.z);
        var centerAboveGround = record.Bounds.center.y - record.GroundY;
        var maxGroundLikeHeight = Mathf.Max(0.35f, Mathf.Min(1.1f, horizontalSpan * 0.08f));
        var localGroundPatch = record.UpFacingSurfaceRatio >= 0.70f
            && record.BottomClearance <= GroundedBottomToleranceMeters + 0.35f
            && record.Bounds.size.y <= maxGroundLikeHeight;
        var routeRibbon = horizontalSpan >= 35f
            && record.UpFacingSurfaceRatio >= 0.70f
            && record.BottomClearance <= GroundedBottomToleranceMeters + 1.0f
            && record.Bounds.size.y <= horizontalSpan * 0.22f
            && centerAboveGround <= Mathf.Max(0.65f, Mathf.Min(3.0f, record.Bounds.size.y * 0.5f + 0.35f));
        return localGroundPatch || routeRibbon;
    }

    private static float UpFacingSurfaceRatioFor(Renderer renderer)
    {
        if (renderer == null)
        {
            return 0f;
        }

        var meshFilter = renderer.GetComponent<MeshFilter>();
        var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        if (mesh == null)
        {
            return 0f;
        }

        var normals = mesh.normals;
        if (normals == null || normals.Length == 0)
        {
            return 0f;
        }

        var upFacing = 0;
        var localToWorld = renderer.transform.localToWorldMatrix;
        for (var i = 0; i < normals.Length; i++)
        {
            var worldNormal = localToWorld.MultiplyVector(normals[i]).normalized;
            if (Vector3.Dot(worldNormal, Vector3.up) >= 0.65f)
            {
                upFacing++;
            }
        }

        return upFacing / (float)normals.Length;
    }

    private static bool RequiresSupport(RendererRecord record)
    {
        if (record.BottomClearance <= GroundedBottomToleranceMeters)
        {
            return false;
        }

        var horizontalSpan = Mathf.Max(record.Bounds.size.x, record.Bounds.size.z);
        var centerAboveGround = record.Bounds.center.y - record.GroundY;
        if (record.BottomClearance > SupportRequiredBottomClearanceMeters && horizontalSpan >= 0.35f)
        {
            return true;
        }

        return centerAboveGround > 1.4f && record.BottomClearance > 0.22f && horizontalSpan >= 0.25f;
    }

    private static RendererRecord FindSupport(RendererRecord target, IReadOnlyList<RendererRecord> records)
    {
        RendererRecord best = null;
        var bestScore = float.MaxValue;
        foreach (var candidate in records)
        {
            if (ReferenceEquals(candidate, target))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(candidate.ExclusionReason))
            {
                continue;
            }

            if (!HasCoVisibleRouteSample(target, candidate))
            {
                continue;
            }

            if (!IsCredibleSupport(candidate, target))
            {
                continue;
            }

            var horizontalGap = HorizontalGap(target.Bounds, candidate.Bounds);
            var verticalGap = Mathf.Max(0f, target.Bounds.min.y - candidate.Bounds.max.y);
            var sameAssemblyBonus = ShareUsefulAncestor(target.Path, candidate.Path) ? -0.35f : 0f;
            var score = horizontalGap + verticalGap * 1.5f + sameAssemblyBonus;
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private static RendererRecord FindBaseSupport(RendererRecord target, IReadOnlyList<RendererRecord> records)
    {
        RendererRecord best = null;
        var bestScore = float.MaxValue;
        foreach (var candidate in records)
        {
            if (ReferenceEquals(candidate, target))
            {
                continue;
            }

            if (!CanServeAsBaseSupport(candidate, target))
            {
                continue;
            }

            if (!HasCoVisibleRouteSample(target, candidate))
            {
                continue;
            }

            var horizontalGap = HorizontalGap(target.Bounds, candidate.Bounds);
            var verticalDelta = target.Bounds.min.y - candidate.Bounds.max.y;
            var score = Mathf.Abs(verticalDelta) + horizontalGap * 0.45f;
            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    private static bool CanServeAsBaseSupport(RendererRecord candidate, RendererRecord target)
    {
        if (candidate.Synthetic || !candidate.RouteVisible)
        {
            return false;
        }

        if (!ShareUsefulAncestor(candidate.Path, target.Path))
        {
            return false;
        }

        if (IsOverheadOrCanopyLike(candidate))
        {
            return false;
        }

        if (!IsBaseSupportShape(candidate))
        {
            return false;
        }

        var verticalDelta = target.Bounds.min.y - candidate.Bounds.max.y;
        if (verticalDelta < -BaseSupportVerticalToleranceMeters || verticalDelta > BaseSupportVerticalToleranceMeters)
        {
            return false;
        }

        var targetSpan = Mathf.Max(target.Bounds.size.x, target.Bounds.size.z);
        var candidateSpan = Mathf.Max(candidate.Bounds.size.x, candidate.Bounds.size.z);
        var supportRadius = Mathf.Max(
            0.45f,
            Mathf.Min(3.0f, targetSpan * 0.5f + candidateSpan * 0.5f + BaseSupportFootprintPaddingMeters));
        return HorizontalGap(target.Bounds, candidate.Bounds) <= supportRadius;
    }

    private static bool IsBaseSupportShape(RendererRecord record)
    {
        var lowerName = record.Name.ToLowerInvariant();
        var lowerPath = record.Path.ToLowerInvariant();
        if (lowerName.Contains("base")
            || lowerName.Contains("root")
            || lowerName.Contains("ground")
            || lowerName.Contains("halo")
            || lowerName.Contains("moss")
            || lowerName.Contains("leaf")
            || lowerName.Contains("floor")
            || lowerName.Contains("bank")
            || lowerName.Contains("rock")
            || lowerPath.Contains("grounding")
            || lowerPath.Contains("rootcluster")
            || lowerPath.Contains("mossbank"))
        {
            return true;
        }

        var horizontalSpan = Mathf.Max(record.Bounds.size.x, record.Bounds.size.z);
        return record.UpFacingSurfaceRatio >= 0.55f
            && record.Bounds.size.y <= Mathf.Max(0.65f, horizontalSpan * 0.35f);
    }

    private static bool IsOverheadOrCanopyLike(RendererRecord record)
    {
        var lowerName = record.Name.ToLowerInvariant();
        return lowerName.Contains("canopy")
            || lowerName.Contains("crown")
            || lowerName.Contains("leafmass")
            || lowerName.Contains("lobe");
    }

    private static bool HasCoVisibleRouteSample(RendererRecord target, RendererRecord candidate)
    {
        return FirstCoVisibleRouteMeters(target, candidate) >= 0f;
    }

    private static float FirstCoVisibleRouteMeters(RendererRecord target, RendererRecord candidate)
    {
        if (!target.RouteVisible || !candidate.RouteVisible)
        {
            return -1f;
        }

        foreach (var targetMeters in target.VisibleMeters)
        {
            foreach (var candidateMeters in candidate.VisibleMeters)
            {
                if (Mathf.Abs(targetMeters - candidateMeters) <= SupportCoVisibleSampleToleranceMeters)
                {
                    return targetMeters;
                }
            }
        }

        return -1f;
    }

    private static bool IsCredibleSupport(RendererRecord candidate, RendererRecord target)
    {
        if (candidate.BottomClearance > SupportCandidateMaxBottomClearanceMeters)
        {
            return false;
        }

        if (candidate.Bounds.size.y < 0.45f)
        {
            return false;
        }

        var candidateHorizontalSpan = Mathf.Max(candidate.Bounds.size.x, candidate.Bounds.size.z);
        if (candidate.Bounds.size.y < candidateHorizontalSpan * 0.18f)
        {
            return false;
        }

        if (candidate.Bounds.max.y + SupportVerticalGapMeters < target.Bounds.min.y)
        {
            return false;
        }

        var supportRadius = Mathf.Max(
            0.95f,
            Mathf.Min(4.5f, Mathf.Max(target.Bounds.size.x, target.Bounds.size.z) * 0.5f + SupportFootprintPaddingMeters));
        if (ShareUsefulAncestor(target.Path, candidate.Path))
        {
            supportRadius += 1.25f;
        }

        return HorizontalGap(target.Bounds, candidate.Bounds) <= supportRadius;
    }

    private static bool IsVerticalSupportShape(RendererRecord record)
    {
        var horizontalSpan = Mathf.Max(record.Bounds.size.x, record.Bounds.size.z);
        return record.Bounds.size.y >= 0.7f && record.Bounds.size.y >= horizontalSpan * 1.25f;
    }

    private static void RunRegressionFixture(
        ValidationResult result,
        IReadOnlyList<Vector3> route,
        IReadOnlyList<RouteCameraSnapshot> snapshots)
    {
        if (!MYB89RideTrajectory.TrySample(route, Mathf.Min(36f, Mathf.Max(1f, result.RouteLengthMeters - 1f)), false, out var unsupportedSample)
            || !MYB89RideTrajectory.TrySample(route, Mathf.Min(48f, Mathf.Max(1f, result.RouteLengthMeters - 1f)), false, out var supportedSample))
        {
            result.FixtureVerdict = "FAIL";
            result.FixtureUnsupportedArbitraryDetected = false;
            result.FixtureSupportedCounterpartPassed = false;
            result.AddBlocking(
                "FIXTURE_ROUTE_SAMPLE_FAILED",
                "MYB-167 regression fixture",
                "Unable to place synthetic fixture bounds along the route.",
                "Restore route sampling before trusting MYB-167.");
            return;
        }

        var unsupportedGround = EstimateGroundY(route, unsupportedSample.Position);
        var supportedGround = EstimateGroundY(route, supportedSample.Position);
        var unsupportedCenter = unsupportedSample.Position + unsupportedSample.Right * 2.9f + Vector3.up * 3.05f;
        var supportedCenter = supportedSample.Position - supportedSample.Right * 2.9f + Vector3.up * 3.05f;
        var floatingPostCenter = unsupportedSample.Position - unsupportedSample.Right * 2.1f + Vector3.up * 1.95f;
        var records = new List<RendererRecord>
        {
            SyntheticRecord(
                "AetherPanel_NoKnownPrefix",
                "MYB167_Fixture/AetherPanel_NoKnownPrefix",
                new Bounds(unsupportedCenter, new Vector3(2.6f, 0.32f, 1.2f)),
                unsupportedGround),
            SyntheticRecord(
                "SupportedBeam_NoKnownPrefix",
                "MYB167_Fixture/SupportedBeam_NoKnownPrefix",
                new Bounds(supportedCenter, new Vector3(2.6f, 0.32f, 1.2f)),
                supportedGround),
            SyntheticRecord(
                "LeftPost_NoKnownPrefix",
                "MYB167_Fixture/LeftPost_NoKnownPrefix",
                new Bounds(supportedCenter + supportedSample.Right * 0.92f + Vector3.down * 1.52f, new Vector3(0.22f, 3.0f, 0.22f)),
                supportedGround),
            SyntheticRecord(
                "RightPost_NoKnownPrefix",
                "MYB167_Fixture/RightPost_NoKnownPrefix",
                new Bounds(supportedCenter - supportedSample.Right * 0.92f + Vector3.down * 1.52f, new Vector3(0.22f, 3.0f, 0.22f)),
                supportedGround),
            SyntheticRecord(
                "FloatingVerticalPost_NoKnownPrefix",
                "MYB167_Fixture/FloatingVerticalPost_NoKnownPrefix",
                new Bounds(floatingPostCenter, new Vector3(0.24f, 2.4f, 0.24f)),
                unsupportedGround)
        };

        ApplyRouteVisibility(records, snapshots, route);
        var unsupportedFinding = EvaluateRecord(records[0], records, true);
        var supportedFinding = EvaluateRecord(records[1], records, true);
        var floatingVerticalFinding = EvaluateRecord(records[4], records, true);
        result.FixtureFindings.Add(unsupportedFinding);
        result.FixtureFindings.Add(supportedFinding);
        result.FixtureFindings.Add(floatingVerticalFinding);
        result.FixtureUnsupportedArbitraryDetected = unsupportedFinding.Severity == FindingSeverity.Blocking && records[0].RouteVisible;
        result.FixtureSupportedCounterpartPassed = supportedFinding.Severity == FindingSeverity.SupportedPass && records[1].RouteVisible;
        result.FixtureFloatingVerticalSupportDetected = floatingVerticalFinding.Severity == FindingSeverity.Blocking && records[4].RouteVisible;
        result.FixtureVerdict = result.FixtureUnsupportedArbitraryDetected && result.FixtureSupportedCounterpartPassed
            && result.FixtureFloatingVerticalSupportDetected
            ? "PASS"
            : "FAIL";

        if (!result.FixtureUnsupportedArbitraryDetected)
        {
            result.AddBlocking(
                "FIXTURE_UNSUPPORTED_NOT_DETECTED",
                records[0].Path,
                "The arbitrary elevated fixture was not detected as route-visible blocking unsupported.",
                "Fix support-required classification before trusting scene results.");
        }

        if (!result.FixtureSupportedCounterpartPassed)
        {
            result.AddBlocking(
                "FIXTURE_SUPPORTED_COUNTERPART_FAILED",
                records[1].Path,
                "The supported fixture counterpart did not pass the geometric support rule.",
                "Tune support detection so real posts/trunks/columns can support elevated assets.");
        }

        if (!result.FixtureFloatingVerticalSupportDetected)
        {
            result.AddBlocking(
                "FIXTURE_FLOATING_VERTICAL_SUPPORT_NOT_DETECTED",
                records[4].Path,
                "The arbitrary floating vertical support-shaped fixture was not detected as blocking.",
                "Fix floating vertical support classification before trusting MYB-167.");
        }
    }

    private static void RunRouteCameraSafetyFixture(
        ValidationResult result,
        IReadOnlyList<Vector3> route,
        IReadOnlyList<RouteCameraSnapshot> snapshots)
    {
        var baseMeters = Mathf.Min(72f, Mathf.Max(1f, result.RouteLengthMeters - 1f));
        var corridorMeters = Mathf.Min(baseMeters + 36f, Mathf.Max(1f, result.RouteLengthMeters - 1f));
        if (!MYB89RideTrajectory.TrySample(route, baseMeters, false, out var sample)
            || !MYB89RideTrajectory.TrySample(route, corridorMeters, false, out var corridorSample))
        {
            result.RouteCameraSafetyFixtureVerdict = "FAIL";
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_ROUTE_SAMPLE_FAILED",
                "MYB-167 route-camera safety fixture",
                "Unable to place synthetic route-camera safety fixture bounds along the route.",
                "Restore route sampling before trusting MYB-167 route-camera safety output.");
            return;
        }

        var ground = EstimateGroundY(route, sample.Position);
        var dominantCenter = sample.Position + sample.Forward * 58f + sample.Right * 16f + Vector3.up * 2.8f;
        var corridorCenter = corridorSample.Position + Vector3.up * 0.7f;
        var markerCenter = sample.Position + sample.Forward * 42f - sample.Right * 10f + Vector3.up * 0.55f;
        var closeScenicCenter = sample.Position + sample.Forward * 34f - sample.Right * 4.8f + Vector3.up * 3.1f;
        var nearPlaneSnapshot = snapshots[Mathf.Min(1, snapshots.Count - 1)];
        var nearPlaneCenter = nearPlaneSnapshot.CameraPosition
            + nearPlaneSnapshot.CameraForward * Mathf.Max(0.02f, nearPlaneSnapshot.NearClipPlane * 0.5f);
        var records = new List<RendererRecord>
        {
            SyntheticRecord(
                "DominantMound_NoKnownPrefix",
                "MYB167_RouteCameraSafetyFixture/DominantMound_NoKnownPrefix",
                new Bounds(dominantCenter, new Vector3(22f, 4.2f, 14f)),
                ground),
            SyntheticRecord(
                "CorridorIntruder_NoKnownPrefix",
                "MYB167_RouteCameraSafetyFixture/CorridorIntruder_NoKnownPrefix",
                new Bounds(corridorCenter, new Vector3(1.4f, 1.2f, 1.4f)),
                EstimateGroundY(route, corridorCenter)),
            SyntheticRecord(
                "NearPlaneCanopy_NoKnownPrefix",
                "MYB167_RouteCameraSafetyFixture/NearPlaneCanopy_NoKnownPrefix",
                new Bounds(nearPlaneCenter, new Vector3(4.0f, 4.0f, 4.0f)),
                EstimateGroundY(route, nearPlaneCenter)),
            SyntheticRecord(
                "BenignSmallMarker_NoKnownPrefix",
                "MYB167_RouteCameraSafetyFixture/BenignSmallMarker_NoKnownPrefix",
                new Bounds(markerCenter, new Vector3(0.45f, 0.8f, 0.45f)),
                ground),
            SyntheticRecord(
                "CloseScenicFrame_KnownSupported",
                "MYB167_RouteCameraSafetyFixture/MYB163_TreeAssembly_CloseLeftFrame/CloseScenicFrame_KnownSupported",
                new Bounds(closeScenicCenter, new Vector3(2.2f, 2.4f, 1.7f)),
                ground)
        };

        ApplyRouteVisibility(records, snapshots, route);
        var dominantFinding = EvaluateRouteCameraSafetyRecord(records[0]);
        var corridorFinding = EvaluateRouteCameraSafetyRecord(records[1]);
        var nearPlaneFinding = EvaluateRouteCameraSafetyRecord(records[2]);
        var benignFinding = EvaluateRouteCameraSafetyRecord(records[3]);
        var closeScenicFinding = EvaluateRouteCameraSafetyRecord(records[4]);
        result.RouteCameraSafetyFixtureFindings.Add(dominantFinding);
        result.RouteCameraSafetyFixtureFindings.Add(corridorFinding);
        result.RouteCameraSafetyFixtureFindings.Add(nearPlaneFinding);
        result.RouteCameraSafetyFixtureFindings.Add(benignFinding);
        result.RouteCameraSafetyFixtureFindings.Add(closeScenicFinding);
        result.RouteCameraSafetyFixtureDominantBlockerDetected =
            dominantFinding.Severity == FindingSeverity.Blocking
            && string.Equals(dominantFinding.Rule, "routeCameraReadabilityBlocker", StringComparison.Ordinal)
            && records[0].RouteVisible;
        result.RouteCameraSafetyFixtureCorridorIntruderDetected =
            corridorFinding.Severity == FindingSeverity.Blocking
            && string.Equals(corridorFinding.Rule, "routeCorridorIntrusion", StringComparison.Ordinal)
            && records[1].RouteVisible;
        result.RouteCameraSafetyFixtureNearPlaneBlockerDetected =
            nearPlaneFinding.Severity == FindingSeverity.Blocking
            && string.Equals(nearPlaneFinding.Rule, "routeCameraReadabilityBlocker", StringComparison.Ordinal)
            && records[2].RouteVisible
            && nearPlaneFinding.DominanceRatio >= 0.95f;
        result.RouteCameraSafetyFixtureBenignMarkerPassed =
            records[3].RouteVisible
            && benignFinding.Severity != FindingSeverity.Warning
            && benignFinding.Severity != FindingSeverity.Blocking;
        result.RouteCameraSafetyFixtureCloseScenicFramingPassed =
            records[4].RouteVisible
            && closeScenicFinding.Severity != FindingSeverity.Warning
            && closeScenicFinding.Severity != FindingSeverity.Blocking
            && string.Equals(closeScenicFinding.Rule, "closeScenicFramingPass", StringComparison.Ordinal);
        result.RouteCameraSafetyFixtureVerdict = result.RouteCameraSafetyFixtureDominantBlockerDetected
            && result.RouteCameraSafetyFixtureCorridorIntruderDetected
            && result.RouteCameraSafetyFixtureNearPlaneBlockerDetected
            && result.RouteCameraSafetyFixtureBenignMarkerPassed
            && result.RouteCameraSafetyFixtureCloseScenicFramingPassed
                ? "PASS"
                : "FAIL";

        if (!result.RouteCameraSafetyFixtureDominantBlockerDetected)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_BLOCKER_NOT_DETECTED",
                records[0].Path,
                "The arbitrary dominant route-camera fixture was not detected as a routeCameraReadabilityBlocker.",
                "Fix projection, protected-zone overlap, or dominance thresholds before trusting MYB-167 route-camera safety output.");
        }

        if (!result.RouteCameraSafetyFixtureCorridorIntruderDetected)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_CORRIDOR_INTRUDER_NOT_DETECTED",
                records[1].Path,
                "The arbitrary corridor-intruding fixture was not detected as a routeCorridorIntrusion blocker.",
                "Fix route corridor clearance classification before trusting MYB-167 route-camera safety output.");
        }

        if (!result.RouteCameraSafetyFixtureNearPlaneBlockerDetected)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_NEAR_PLANE_NOT_DETECTED",
                records[2].Path,
                "The arbitrary near-plane fixture was not detected as a routeCameraReadabilityBlocker.",
                "Fix near-plane projection fallback before trusting MYB-167 route-camera safety output.");
        }

        if (!result.RouteCameraSafetyFixtureBenignMarkerPassed)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_BENIGN_MARKER_FAILED",
                records[3].Path,
                "The small arbitrary fixture marker was incorrectly treated as a route-camera safety blocker/warning or was not route-visible.",
                "Tune route-camera safety thresholds so small nearby markers are not confused with blockers.");
        }

        if (!result.RouteCameraSafetyFixtureCloseScenicFramingPassed)
        {
            result.AddBlocking(
                "ROUTE_CAMERA_SAFETY_FIXTURE_CLOSE_SCENIC_FRAMING_FAILED",
                records[4].Path,
                "The authored close forest framing fixture was incorrectly treated as a route-camera safety blocker/warning or was not route-visible.",
                "Tune closeScenicFraming thresholds so close supported trees can frame the road without masking the protected route/horizon bands.");
        }
    }

    private static RendererRecord SyntheticRecord(string name, string path, Bounds bounds, float groundY)
    {
        return new RendererRecord
        {
            Name = name,
            Path = path,
            AssetKey = path,
            RendererType = "SyntheticBounds",
            Bounds = bounds,
            GroundY = groundY,
            RouteGroundY = groundY,
            BottomClearance = bounds.min.y - groundY,
            RouteBottomClearance = bounds.min.y - groundY,
            GroundSource = "synthetic-ground",
            UpFacingSurfaceRatio = 0f,
            Synthetic = true
        };
    }

    private static void RunMyb144(ValidationResult result)
    {
        var myb144 = MYB144ArtAssetValidator.RunValidation("MYB-167-RouteVisibleSupportValidator");
        result.Myb144Verdict = myb144.Verdict;
        result.Myb144Errors = myb144.ErrorCount;
        result.Myb144Warnings = myb144.WarningCount;
        result.Myb144ReportPathRelative = Myb144ReportRelativePath;

        if (myb144.ErrorCount > 0)
        {
            result.AddBlocking(
                "MYB144_ERRORS",
                "MYB-144 Art Asset Validator",
                "MYB-144 reported " + myb144.ErrorCount.ToString(CultureInfo.InvariantCulture) + " errors.",
                "Resolve MYB-144 errors before treating MYB-167 as clean.");
        }
        else if (myb144.WarningCount > 0)
        {
            result.AddWarning(
                "MYB144_WARNINGS",
                "MYB-144 Art Asset Validator",
                "MYB-144 reported " + myb144.WarningCount.ToString(CultureInfo.InvariantCulture) + " warnings.",
                "Document whether warnings are existing or introduced by MYB-167.");
        }
    }

    private static string ExclusionReasonFor(Renderer renderer, string path)
    {
        var lowerPath = path.ToLowerInvariant();
        var lowerName = renderer.gameObject.name.ToLowerInvariant();
        if (lowerPath.Contains("hud") || lowerPath.Contains("canvas") || lowerPath.Contains("eventsystem")
            || lowerPath.Contains("myb73_routepreview"))
        {
            return "hud-or-preview-ui";
        }

        if (lowerPath.Contains("myb165_bikepovcues") || lowerPath.Contains("myb89_riderrig") || lowerPath.Contains("bikepovcue"))
        {
            return "bike-pov-cockpit-system";
        }

        if (lowerPath.Contains("intendedgate") || lowerPath.Contains("intended_gate")
            || lowerPath.Contains("routesignage") || lowerPath.Contains("route_signage")
            || lowerPath.Contains("intendedroutesign") || lowerPath.Contains("intended_route_sign")
            || lowerName.Contains("checkpointbeam")
            || lowerName.Contains("archbeam")
            || lowerName.Contains("archpost"))
        {
            return "intended-gate-or-signage";
        }

        if (lowerPath.Contains("gameplaycue") || lowerPath.Contains("gameplay_cue")
            || lowerPath.Contains("routedifficultycues")
            || lowerName.Contains("toprune"))
        {
            return "gameplay-cue";
        }

        if (lowerPath.Contains("capturehelper") || lowerPath.Contains("capture_helper"))
        {
            return "capture-helper";
        }

        if (renderer is LineRenderer)
        {
            return "route-line-renderer";
        }

        if (IsRouteGroundSurfaceName(lowerName))
        {
            return "route-ground-surface";
        }

        return string.Empty;
    }

    private static bool IsRouteGroundSurfaceName(string lowerName)
    {
        return lowerName.Contains("routeroad")
            || lowerName.Contains("route_road")
            || lowerName.Contains("routeedge")
            || lowerName.Contains("edgeline")
            || lowerName.Contains("edge_line")
            || lowerName.Contains("centerdash")
            || lowerName.Contains("meadowband")
            || lowerName.Contains("villageverge")
            || lowerName.Contains("longground")
            || lowerName.Contains("groundplane")
            || lowerName.Contains("forestfloor")
            || lowerName.Contains("floorpatch")
            || lowerName.Contains("meadowshoulder")
            || lowerName.Contains("shoulder")
            || lowerName.Contains("mossmat")
            || lowerName.Contains("leafmoss")
            || lowerName.Contains("leaf_moss")
            || lowerName.Contains("groundinghalo")
            || lowerName.Contains("mossbank")
            || lowerName.Contains("ground_root");
    }

    private static float EstimateGroundY(IReadOnlyList<Vector3> route, Vector3 worldPosition)
    {
        if (route == null || route.Count == 0)
        {
            return 0f;
        }

        var bestDistanceSq = float.MaxValue;
        var bestY = route[0].y;
        var query = new Vector2(worldPosition.x, worldPosition.z);
        foreach (var point in route)
        {
            var distanceSq = (new Vector2(point.x, point.z) - query).sqrMagnitude;
            if (distanceSq >= bestDistanceSq)
            {
                continue;
            }

            bestDistanceSq = distanceSq;
            bestY = point.y;
        }

        return bestY;
    }

    private static float HorizontalGap(Bounds a, Bounds b)
    {
        var xGap = Mathf.Max(0f, Mathf.Max(a.min.x - b.max.x, b.min.x - a.max.x));
        var zGap = Mathf.Max(0f, Mathf.Max(a.min.z - b.max.z, b.min.z - a.max.z));
        return Mathf.Sqrt(xGap * xGap + zGap * zGap);
    }

    private static bool TryProjectBounds(RouteCameraSnapshot snapshot, Bounds bounds, out Rect screenRect)
    {
        var fullViewport = new Rect(0f, 0f, 1f, 1f);
        var nearDistance = Mathf.Max(0.05f, snapshot.NearClipPlane + 0.05f);
        var touchesNearCamera = bounds.Contains(snapshot.CameraPosition)
            || bounds.SqrDistance(snapshot.CameraPosition) <= nearDistance * nearDistance;
        var viewProjection = snapshot.ProjectionMatrix * snapshot.WorldToCameraMatrix;
        var corners = BoundsCorners(bounds);
        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;
        var projectedAnyCorner = false;
        var clippedAnyCorner = false;

        foreach (var corner in corners)
        {
            var clip = viewProjection * new Vector4(corner.x, corner.y, corner.z, 1f);
            if (clip.w <= 0.01f)
            {
                clippedAnyCorner = true;
                continue;
            }

            var viewportX = clip.x / clip.w * 0.5f + 0.5f;
            var viewportY = clip.y / clip.w * 0.5f + 0.5f;
            minX = Mathf.Min(minX, viewportX);
            minY = Mathf.Min(minY, viewportY);
            maxX = Mathf.Max(maxX, viewportX);
            maxY = Mathf.Max(maxY, viewportY);
            projectedAnyCorner = true;
        }

        if (!projectedAnyCorner)
        {
            if (touchesNearCamera)
            {
                screenRect = fullViewport;
                return true;
            }

            screenRect = Rect.zero;
            return false;
        }

        if (touchesNearCamera || clippedAnyCorner && BoundsExtendsAcrossCameraForward(snapshot, bounds))
        {
            screenRect = fullViewport;
            return true;
        }

        screenRect = Intersect(
            new Rect(minX, minY, Mathf.Max(0f, maxX - minX), Mathf.Max(0f, maxY - minY)),
            fullViewport);
        return Area(screenRect) > 0.0001f;
    }

    private static bool BoundsExtendsAcrossCameraForward(RouteCameraSnapshot snapshot, Bounds bounds)
    {
        var minForwardDistance = float.MaxValue;
        var maxForwardDistance = float.MinValue;
        foreach (var corner in BoundsCorners(bounds))
        {
            var distance = Vector3.Dot(corner - snapshot.CameraPosition, snapshot.CameraForward);
            minForwardDistance = Mathf.Min(minForwardDistance, distance);
            maxForwardDistance = Mathf.Max(maxForwardDistance, distance);
        }

        return minForwardDistance <= snapshot.NearClipPlane + 0.05f && maxForwardDistance >= 0f;
    }

    private static Vector3[] BoundsCorners(Bounds bounds)
    {
        var min = bounds.min;
        var max = bounds.max;
        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };
    }

    private static Rect Intersect(Rect a, Rect b)
    {
        var minX = Mathf.Max(a.xMin, b.xMin);
        var minY = Mathf.Max(a.yMin, b.yMin);
        var maxX = Mathf.Min(a.xMax, b.xMax);
        var maxY = Mathf.Min(a.yMax, b.yMax);
        if (maxX <= minX || maxY <= minY)
        {
            return Rect.zero;
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private static float Area(Rect rect)
    {
        return Mathf.Max(0f, rect.width) * Mathf.Max(0f, rect.height);
    }

    private static float MinRouteCorridorClearance(Bounds bounds, IReadOnlyList<Vector3> route)
    {
        if (route == null || route.Count < 2)
        {
            return float.MaxValue;
        }

        var query = new Vector2(bounds.center.x, bounds.center.z);
        var radius = Mathf.Max(bounds.extents.x, bounds.extents.z);
        var best = float.MaxValue;
        for (var i = 1; i < route.Count; i++)
        {
            var a = new Vector2(route[i - 1].x, route[i - 1].z);
            var b = new Vector2(route[i].x, route[i].z);
            best = Mathf.Min(best, DistancePointSegment(query, a, b) - radius);
        }

        return best;
    }

    private static float DistancePointSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var segment = b - a;
        var lengthSq = segment.sqrMagnitude;
        if (lengthSq <= 0.0001f)
        {
            return Vector2.Distance(point, a);
        }

        var t = Mathf.Clamp01(Vector2.Dot(point - a, segment) / lengthSq);
        return Vector2.Distance(point, a + segment * t);
    }

    private static bool ShareUsefulAncestor(string a, string b)
    {
        var aParts = a.Split('/');
        var bParts = b.Split('/');
        var limit = Mathf.Min(aParts.Length, bParts.Length);
        var shared = 0;
        for (var i = 0; i < limit; i++)
        {
            if (!string.Equals(aParts[i], bParts[i], StringComparison.Ordinal))
            {
                break;
            }

            shared++;
        }

        return shared >= 3;
    }

    private static string PathOf(GameObject gameObject)
    {
        var names = new List<string>();
        var current = gameObject.transform;
        while (current != null)
        {
            names.Add(current.name);
            current = current.parent;
        }

        names.Reverse();
        return string.Join("/", names);
    }

    private static string AssetKeyForPath(string path)
    {
        var parts = path.Split('/');
        if (parts.Length <= 2)
        {
            return path;
        }

        var generatedRootIndex = Array.FindIndex(parts, part =>
            part.StartsWith("MYB", StringComparison.Ordinal)
            && (part.EndsWith("Root", StringComparison.Ordinal) || part.Contains("_Root")));
        if (generatedRootIndex >= 0 && generatedRootIndex + 1 < parts.Length)
        {
            return string.Join("/", parts.Take(generatedRootIndex + 2).ToArray());
        }

        return string.Join("/", parts.Take(Mathf.Min(3, parts.Length)).ToArray());
    }

    private static void WriteReport(ValidationResult result)
    {
        File.WriteAllText(ToRepoPath(ReportRelativePath), BuildMarkdownReport(result));
    }

    private static string BuildMarkdownReport(ValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# MYB-167 Route-Camera Safety Gate Report");
        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine("MYB-167 implements a generic route-camera safety gate for route-visible renderers. It scans all enabled scene `Renderer` instances, samples the bike POV route camera across the route, classifies visible renderers by bounds/support geometry, projected screen dominance, protected route-zone overlap, and route corridor clearance, then proves arbitrary synthetic fixtures are detected without known prefixes.");
        builder.AppendLine();
        builder.AppendLine("## Scope");
        builder.AppendLine("- Validator path: `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`");
        builder.AppendLine("- Scene validated: `" + result.ScenePath + "`");
        builder.AppendLine("- Report: `" + ReportRelativePath + "`");
        builder.AppendLine("- Metrics: `" + MetricsRelativePath + "`");
        builder.AppendLine("- Code/scene/assets modified by validator run: No scene save, no gameplay change, no route/collider/HUD/telemetry change, no assets generated.");
        builder.AppendLine();
        builder.AppendLine("## Files Changed");
        builder.AppendLine("- `unity/Echapee4D/Assets/MYB167/Editor/MYB167RouteVisibleSupportValidator.cs`");
        builder.AppendLine("- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-report.md`");
        builder.AppendLine("- `_bmad-output/implementation-artifacts/MYB-167/myb-167-route-visible-support-metrics.json`");
        builder.AppendLine("- `docs/validation/route-camera-safety-gate.md`");
        builder.AppendLine("- `docs/validation/unity-visual-support-policy.md`");
        builder.AppendLine("- `CONTEXT.md`");
        builder.AppendLine("- `CONTEXT-MAP.md`");
        builder.AppendLine("- `_bmad-output/linear-sync.md`");
        builder.AppendLine("- `_bmad-output/unity-test-results/myb-144-art-asset-validator-report.md` refreshed by MYB-144.");
        builder.AppendLine();
        builder.AppendLine("## Detection Model");
        builder.AppendLine("- Visibility: `GeometryUtility.TestPlanesAABB` against sampled bike POV route-camera frustums.");
        builder.AppendLine("- Projection: renderer world bounds are projected through sampled route-camera view/projection matrices into viewport rectangles.");
        builder.AppendLine("- Protected route-camera zone: `x=0.18..0.82, y=0.20..0.78`; objects that dominate this zone become route-camera readability suspects.");
        builder.AppendLine("- Route samples: `" + result.RouteCameraSampleCount + "` at about `" + FormatFloat(RouteSampleSpacingMeters) + "m` spacing.");
        builder.AppendLine("- Ground source: `" + result.GroundSource + "`.");
        builder.AppendLine("- Floating vertical support rule: route-visible vertical support-shaped renderers warn above `" + FormatFloat(RouteVisibleFloatingWarningClearanceMeters) + "m` and block above `" + FormatFloat(RouteVisibleFloatingBlockingClearanceMeters) + "m` when measured against a credible local ground source.");
        builder.AppendLine("- Local ground correction: known MYB-163 generated forest masses use their authored shoulder/forest-floor height instead of route centerline Y, so side banks and back-wall trunks are not false-blocked for being above the road plane.");
        builder.AppendLine("- Base support rule: vertical supports may pass when a same-assembly base/root/grounding element is route-camera co-visible and physically overlaps the support footprint.");
        builder.AppendLine("- Support-required rule: elevated route-visible bounds with bottomClearance above `" + FormatFloat(SupportRequiredBottomClearanceMeters) + "m` need nearby ground-connected vertical geometry that is co-visible in the route camera sample.");
        builder.AppendLine("- Support candidate tolerance: candidate supports may be associated up to `" + FormatFloat(SupportCandidateMaxBottomClearanceMeters) + "m` bottomClearance because ground is approximate; the support renderer is still evaluated separately by the floating vertical support rule.");
        builder.AppendLine("- routeCorridorIntrusion rule: non-exempt renderer bounds with route-centerline clearance below `" + FormatFloat(RouteCorridorWarningHalfWidthMeters) + "m` warn, and below `" + FormatFloat(RouteCorridorBlockingClearanceMeters) + "m` block.");
        builder.AppendLine("- routeCameraReadabilityBlocker rule: non-exempt renderer bounds with viewport dominance/protected-zone overlap above `" + FormatFloat(ReadabilityWarningDominanceRatio) + "`/`" + FormatFloat(ReadabilityWarningProtectedOverlapRatio) + "` warn, and above `" + FormatFloat(ReadabilityBlockingDominanceRatio) + "`/`" + FormatFloat(ReadabilityBlockingProtectedOverlapRatio) + "` block.");
        builder.AppendLine("- Elevated route occlusion rule: real scene renderers near the route corridor warn when they dominate at least `" + FormatFloat(ElevatedRouteOcclusionBlockingDominanceRatio) + "` of the viewport and overlap at least `" + FormatFloat(ElevatedRouteOcclusionBlockingOverlapRatio) + "` of the upper route/horizon band `" + FormatRect(ElevatedRouteOcclusionViewport) + "`; they block only with hard corridor, protected-zone, or strong-dominance evidence.");
        builder.AppendLine("- closeScenicFramingPass rule: authored MYB-163/MYB-112 forest framing may be close to the road when it remains outside hard corridor intrusion and below protected route/horizon overlap limits `" + FormatFloat(CloseScenicFramingProtectedOverlapLimit) + "`/`" + FormatFloat(CloseScenicFramingElevatedOverlapLimit) + "`.");
        builder.AppendLine("- Fixed-prefix-only detection used: `" + BoolText(result.FixedPrefixOnlyDetectionUsed) + "`.");
        builder.AppendLine();
        builder.AppendLine("## Allowlist / Exclusions");
        builder.AppendLine("These exclusions are category-level system/route exemptions, not a visual-asset deny-list.");
        builder.AppendLine();
        builder.AppendLine("| Rule | Reason |");
        builder.AppendLine("|---|---|");
        foreach (var rule in AllowlistRules)
        {
            builder.AppendLine("| `" + rule.Id + "` | " + rule.Description + " |");
        }
        builder.AppendLine();
        builder.AppendLine("## Metrics");
        builder.AppendLine("- totalRendererCount: `" + result.TotalRendererCount + "`");
        builder.AppendLine("- routeVisibleRendererCount: `" + result.RouteVisibleRendererCount + "`");
        builder.AppendLine("- routeVisibleAssetCount: `" + result.RouteVisibleAssetCount + "`");
        builder.AppendLine("- excludedSystemRendererCount: `" + result.ExcludedSystemRendererCount + "`");
        builder.AppendLine("- groundedPassCount: `" + result.GroundedPassCount + "`");
        builder.AppendLine("- supportRequiredCount: `" + result.SupportRequiredCount + "`");
        builder.AppendLine("- supportedPassCount: `" + result.SupportedPassCount + "`");
        builder.AppendLine("- unsupportedWarningCount: `" + result.UnsupportedWarningCount + "`");
        builder.AppendLine("- unsupportedBlockingCount: `" + result.UnsupportedBlockingCount + "`");
        builder.AppendLine("- maxUnsupportedBottomClearance: `" + FormatFloat(result.MaxUnsupportedBottomClearance) + "m`");
        builder.AppendLine("- maxUnsupportedVisibleDistanceMeters: `" + FormatFloat(result.MaxUnsupportedVisibleDistanceMeters) + "m`");
        builder.AppendLine("- routeCorridorIntrusionCount: `" + result.RouteCorridorIntrusionCount + "`");
        builder.AppendLine("- routeReadabilityWarningCount: `" + result.RouteReadabilityWarningCount + "`");
        builder.AppendLine("- routeReadabilityBlockingCount: `" + result.RouteReadabilityBlockingCount + "`");
        builder.AppendLine("- worstRouteVisibilityScore: `" + FormatFloat(result.WorstRouteVisibilityScore) + "`");
        builder.AppendLine("- routeCameraSafetyVerdict: `" + result.RouteCameraSafetyVerdict + "`");
        builder.AppendLine();
        builder.AppendLine("## Support Regression Fixture");
        builder.AppendLine("- Verdict: `" + result.FixtureVerdict + "`");
        builder.AppendLine("- Unsupported arbitrary object detected: `" + YesNo(result.FixtureUnsupportedArbitraryDetected) + "`");
        builder.AppendLine("- Supported counterpart passed: `" + YesNo(result.FixtureSupportedCounterpartPassed) + "`");
        builder.AppendLine("- Floating vertical support detected: `" + YesNo(result.FixtureFloatingVerticalSupportDetected) + "`");
        builder.AppendLine("- Fixture writes scene objects: `No`; the fixture is synthetic bounds evaluated through the same frustum/support logic.");
        builder.AppendLine();
        foreach (var finding in result.FixtureFindings)
        {
            builder.AppendLine("- `" + finding.Path + "`: `" + finding.Severity + "` - " + finding.Reason);
        }
        builder.AppendLine();
        builder.AppendLine("## Route-Camera Safety Fixture");
        builder.AppendLine("- Verdict: `" + result.RouteCameraSafetyFixtureVerdict + "`");
        builder.AppendLine("- Dominant arbitrary blocker detected: `" + YesNo(result.RouteCameraSafetyFixtureDominantBlockerDetected) + "`");
        builder.AppendLine("- Corridor arbitrary intruder detected: `" + YesNo(result.RouteCameraSafetyFixtureCorridorIntruderDetected) + "`");
        builder.AppendLine("- Near-plane arbitrary blocker detected: `" + YesNo(result.RouteCameraSafetyFixtureNearPlaneBlockerDetected) + "`");
        builder.AppendLine("- Benign small marker passed: `" + YesNo(result.RouteCameraSafetyFixtureBenignMarkerPassed) + "`");
        builder.AppendLine("- Close scenic forest framing passed: `" + YesNo(result.RouteCameraSafetyFixtureCloseScenicFramingPassed) + "`");
        builder.AppendLine("- Fixture writes scene objects: `No`; the fixture is synthetic bounds evaluated through the same route-camera projection/corridor logic.");
        builder.AppendLine();
        foreach (var finding in result.RouteCameraSafetyFixtureFindings)
        {
            builder.AppendLine("- `" + finding.Path + "`: `" + finding.Severity + "` `" + finding.Rule + "` - " + finding.Reason);
        }
        builder.AppendLine();
        builder.AppendLine("## Suspects");
        builder.AppendLine("### Route-Camera Safety");
        builder.AppendLine();
        AppendRouteCameraSafetyFindingsTable(builder, result.RouteCameraSafetyFindings);
        AppendFindingsTable(builder, "Blocking", result.Findings.Where(finding => finding.Severity == FindingSeverity.Blocking));
        AppendFindingsTable(builder, "Warnings", result.Findings.Where(finding => finding.Severity == FindingSeverity.Warning));
        builder.AppendLine("## Comparison With MYB-165 Fix");
        builder.AppendLine("MYB-165 removed known inherited unsupported route-visible props using a fixed prefix cleanup. MYB-167 does not use that fixed-prefix list as the detection source: it samples the bike POV route-camera, collects every enabled scene `Renderer` visible in at least one route frustum, evaluates bounds/support geometry, and projects bounds into the route-camera viewport to detect corridor intrusion or readability blockers. Prefixes are used only for documented system/route/cockpit/explicit-role exclusions.");
        builder.AppendLine();
        builder.AppendLine("## MYB-144");
        builder.AppendLine("- Verdict: `" + result.Myb144Verdict + "`");
        builder.AppendLine("- Errors: `" + result.Myb144Errors + "`");
        builder.AppendLine("- Warnings: `" + result.Myb144Warnings + "`");
        builder.AppendLine("- Report: `" + result.Myb144ReportPathRelative + "`");
        builder.AppendLine();
        builder.AppendLine("## Findings");
        AppendMessages(builder, "Blocking Errors", result.BlockingErrors);
        AppendMessages(builder, "Warnings", result.Warnings);
        AppendMessages(builder, "Info", result.Info);
        builder.AppendLine("## Governance");
        builder.AppendLine("- This is a validation/governance hardening ticket.");
        builder.AppendLine("- No scene correction or asset deletion was performed by MYB-167.");
        builder.AppendLine("- No Meshy, Tripo, Blender, or Poly Haven call was made.");
        builder.AppendLine("- The validator improves on the MYB-165 fixed-prefix cleanup by scanning all route-visible renderers and using geometric support, corridor clearance, and route-camera projection evidence.");
        builder.AppendLine("- Route-camera video remains useful human proof, but this gate is analytical and can fail before recording a video.");
        builder.AppendLine("- Premium target reached: `No`.");
        builder.AppendLine("- Recommended Linear status: `In Review` until Julien validates the severity wording.");
        builder.AppendLine();
        builder.AppendLine("## Verdict");
        builder.AppendLine("- Validator verdict: `" + result.Verdict + "`");
        builder.AppendLine("- Scene support gate: `" + (result.UnsupportedBlockingCount == 0 ? "PASS" : "FAIL") + "`");
        builder.AppendLine("- RouteCameraSafetyGate: `" + result.RouteCameraSafetyVerdict + "`");
        builder.AppendLine("- Support fixture gate: `" + result.FixtureVerdict + "`");
        builder.AppendLine("- Route-camera safety fixture gate: `" + result.RouteCameraSafetyFixtureVerdict + "`");
        return builder.ToString();
    }

    private static void AppendFindingsTable(StringBuilder builder, string title, IEnumerable<SupportFinding> findings)
    {
        var list = findings
            .OrderBy(finding => finding.MinVisibleDistanceMeters)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToList();
        builder.AppendLine("### " + title);
        builder.AppendLine();
        if (list.Count == 0)
        {
            builder.AppendLine("- None.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Renderer | Type | first visible m | min distance | bottom clearance | ground source | support | reason |");
        builder.AppendLine("|---|---|---:|---:|---:|---|---|---|");
        foreach (var finding in list.Take(60))
        {
            builder.AppendLine("| `" + EscapeMarkdown(finding.Path) + "` | `"
                + finding.RendererType + "` | "
                + FormatFloat(finding.FirstVisibleMeters) + " | "
                + FormatFloat(finding.MinVisibleDistanceMeters) + "m | "
                + FormatFloat(finding.BottomClearance) + "m | "
                + EscapeMarkdown(finding.GroundSource) + " | "
                + (string.IsNullOrEmpty(finding.NearestSupportPath) ? "-" : "`" + EscapeMarkdown(finding.NearestSupportPath) + "`") + " | "
                + EscapeMarkdown(finding.Reason) + " |");
        }

        if (list.Count > 60)
        {
            builder.AppendLine("| ... | ... | ... | ... | ... | ... | ... | `" + (list.Count - 60) + " more omitted from markdown report; see metrics JSON.` |");
        }

        builder.AppendLine();
    }

    private static void AppendRouteCameraSafetyFindingsTable(
        StringBuilder builder,
        IEnumerable<RouteCameraSafetyFinding> findings)
    {
        var list = findings
            .OrderByDescending(finding => finding.Severity)
            .ThenByDescending(finding => finding.VisibilityScore)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToList();
        if (list.Count == 0)
        {
            builder.AppendLine("- None.");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("| Renderer | Rule | role | route m | est. time | min distance | corridor clearance | screenRect | dominance | protected overlap | elevated overlap | action |");
        builder.AppendLine("|---|---|---|---:|---:|---:|---:|---|---:|---:|---:|---|");
        foreach (var finding in list.Take(60))
        {
            builder.AppendLine("| `"
                + EscapeMarkdown(finding.Path)
                + "` | `"
                + finding.Rule
                + "` | `"
                + finding.Role
                + "` | "
                + FormatFloat(finding.RouteMeters)
                + " | "
                + FormatFloat(finding.EstimatedTimeSeconds)
                + "s | "
                + FormatFloat(finding.MinVisibleDistanceMeters)
                + "m | "
                + FormatFloat(finding.RouteCorridorClearanceMeters)
                + "m | `"
                + FormatRect(finding.ScreenRect)
                + "` | "
                + FormatFloat(finding.DominanceRatio)
                + " | "
                + FormatFloat(finding.ProtectedOverlapRatio)
                + " | "
                + FormatFloat(finding.ElevatedRouteOverlapRatio)
                + " | "
                + EscapeMarkdown(finding.RecommendedAction)
                + " |");
        }

        if (list.Count > 60)
        {
            builder.AppendLine("| ... | ... | ... | ... | ... | ... | ... | ... | ... | ... | ... | `" + (list.Count - 60) + " more omitted from markdown report; see metrics JSON.` |");
        }

        builder.AppendLine();
    }

    private static void AppendMessages(StringBuilder builder, string title, IReadOnlyList<ValidationMessage> messages)
    {
        builder.AppendLine("### " + title);
        builder.AppendLine();
        if (messages.Count == 0)
        {
            builder.AppendLine("- None.");
            builder.AppendLine();
            return;
        }

        foreach (var message in messages)
        {
            builder.AppendLine("- `" + message.Code + "` `" + EscapeMarkdown(message.Subject) + "` - " + message.Message + " Action: " + message.Action);
        }

        builder.AppendLine();
    }

    private static void WriteMetricsJson(ValidationResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine("{");
        builder.AppendLine("  \"ticket\": \"MYB-167\",");
        builder.AppendLine("  \"generatedAt\": \"" + EscapeJson(result.GeneratedAt) + "\",");
        builder.AppendLine("  \"scenePath\": \"" + EscapeJson(result.ScenePath) + "\",");
        builder.AppendLine("  \"routeLengthMeters\": " + FormatJson(result.RouteLengthMeters) + ",");
        builder.AppendLine("  \"routePointCount\": " + result.RoutePointCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeCameraSampleCount\": " + result.RouteCameraSampleCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"totalRendererCount\": " + result.TotalRendererCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeVisibleRendererCount\": " + result.RouteVisibleRendererCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeVisibleAssetCount\": " + result.RouteVisibleAssetCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"excludedSystemRendererCount\": " + result.ExcludedSystemRendererCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"groundedPassCount\": " + result.GroundedPassCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"supportRequiredCount\": " + result.SupportRequiredCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"supportedPassCount\": " + result.SupportedPassCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"unsupportedWarningCount\": " + result.UnsupportedWarningCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"unsupportedBlockingCount\": " + result.UnsupportedBlockingCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"maxUnsupportedBottomClearance\": " + FormatJson(result.MaxUnsupportedBottomClearance) + ",");
        builder.AppendLine("  \"maxUnsupportedVisibleDistanceMeters\": " + FormatJson(result.MaxUnsupportedVisibleDistanceMeters) + ",");
        builder.AppendLine("  \"routeCorridorIntrusionCount\": " + result.RouteCorridorIntrusionCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeReadabilityWarningCount\": " + result.RouteReadabilityWarningCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"routeReadabilityBlockingCount\": " + result.RouteReadabilityBlockingCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"worstRouteVisibilityScore\": " + FormatJson(result.WorstRouteVisibilityScore) + ",");
        builder.AppendLine("  \"routeCameraSafetyVerdict\": \"" + EscapeJson(result.RouteCameraSafetyVerdict) + "\",");
        builder.AppendLine("  \"fixedPrefixOnlyDetectionUsed\": " + BoolJson(result.FixedPrefixOnlyDetectionUsed) + ",");
        builder.AppendLine("  \"allowlistEntryCount\": " + result.AllowlistEntryCount.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"allowlistEntriesDocumented\": " + BoolJson(result.AllowlistEntriesDocumented) + ",");
        builder.AppendLine("  \"groundSource\": \"" + EscapeJson(result.GroundSource) + "\",");
        builder.AppendLine("  \"fixtureVerdict\": \"" + EscapeJson(result.FixtureVerdict) + "\",");
        builder.AppendLine("  \"fixtureUnsupportedArbitraryDetected\": " + BoolJson(result.FixtureUnsupportedArbitraryDetected) + ",");
        builder.AppendLine("  \"fixtureSupportedCounterpartPassed\": " + BoolJson(result.FixtureSupportedCounterpartPassed) + ",");
        builder.AppendLine("  \"fixtureFloatingVerticalSupportDetected\": " + BoolJson(result.FixtureFloatingVerticalSupportDetected) + ",");
        builder.AppendLine("  \"routeCameraSafetyFixtureVerdict\": \"" + EscapeJson(result.RouteCameraSafetyFixtureVerdict) + "\",");
        builder.AppendLine("  \"routeCameraSafetyFixtureDominantBlockerDetected\": " + BoolJson(result.RouteCameraSafetyFixtureDominantBlockerDetected) + ",");
        builder.AppendLine("  \"routeCameraSafetyFixtureCorridorIntruderDetected\": " + BoolJson(result.RouteCameraSafetyFixtureCorridorIntruderDetected) + ",");
        builder.AppendLine("  \"routeCameraSafetyFixtureNearPlaneBlockerDetected\": " + BoolJson(result.RouteCameraSafetyFixtureNearPlaneBlockerDetected) + ",");
        builder.AppendLine("  \"routeCameraSafetyFixtureBenignMarkerPassed\": " + BoolJson(result.RouteCameraSafetyFixtureBenignMarkerPassed) + ",");
        builder.AppendLine("  \"routeCameraSafetyFixtureCloseScenicFramingPassed\": " + BoolJson(result.RouteCameraSafetyFixtureCloseScenicFramingPassed) + ",");
        builder.AppendLine("  \"myb144Verdict\": \"" + EscapeJson(result.Myb144Verdict) + "\",");
        builder.AppendLine("  \"myb144Errors\": " + result.Myb144Errors.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"myb144Warnings\": " + result.Myb144Warnings.ToString(CultureInfo.InvariantCulture) + ",");
        builder.AppendLine("  \"verdict\": \"" + EscapeJson(result.Verdict) + "\",");
        builder.AppendLine("  \"suspects\": [");
        var suspects = result.Findings
            .Where(finding => finding.Severity == FindingSeverity.Warning || finding.Severity == FindingSeverity.Blocking)
            .OrderBy(finding => finding.Severity)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToList();
        for (var i = 0; i < suspects.Count; i++)
        {
            var finding = suspects[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"path\": \"" + EscapeJson(finding.Path) + "\",");
            builder.AppendLine("      \"assetKey\": \"" + EscapeJson(finding.AssetKey) + "\",");
            builder.AppendLine("      \"rendererType\": \"" + EscapeJson(finding.RendererType) + "\",");
            builder.AppendLine("      \"severity\": \"" + EscapeJson(finding.Severity.ToString()) + "\",");
            builder.AppendLine("      \"firstVisibleMeters\": " + FormatJson(finding.FirstVisibleMeters) + ",");
            builder.AppendLine("      \"minVisibleDistanceMeters\": " + FormatJson(finding.MinVisibleDistanceMeters) + ",");
            builder.AppendLine("      \"boundsMinY\": " + FormatJson(finding.BoundsMinY) + ",");
            builder.AppendLine("      \"groundY\": " + FormatJson(finding.GroundY) + ",");
            builder.AppendLine("      \"routeGroundY\": " + FormatJson(finding.RouteGroundY) + ",");
            builder.AppendLine("      \"bottomClearance\": " + FormatJson(finding.BottomClearance) + ",");
            builder.AppendLine("      \"routeBottomClearance\": " + FormatJson(finding.RouteBottomClearance) + ",");
            builder.AppendLine("      \"groundSource\": \"" + EscapeJson(finding.GroundSource) + "\",");
            builder.AppendLine("      \"nearestSupportPath\": \"" + EscapeJson(finding.NearestSupportPath) + "\",");
            builder.AppendLine("      \"supportHorizontalGapMeters\": " + FormatJson(finding.SupportHorizontalGapMeters) + ",");
            builder.AppendLine("      \"supportVerticalGapMeters\": " + FormatJson(finding.SupportVerticalGapMeters) + ",");
            builder.AppendLine("      \"supportCoVisibleMeters\": " + FormatJson(finding.SupportCoVisibleMeters) + ",");
            builder.AppendLine("      \"reason\": \"" + EscapeJson(finding.Reason) + "\"");
            builder.Append("    }");
            if (i < suspects.Count - 1)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        builder.AppendLine("  ],");
        builder.AppendLine("  \"routeCameraSafetySuspects\": [");
        var routeCameraSafetySuspects = result.RouteCameraSafetyFindings
            .Where(finding => finding.Severity == FindingSeverity.Warning || finding.Severity == FindingSeverity.Blocking)
            .OrderByDescending(finding => finding.Severity)
            .ThenByDescending(finding => finding.VisibilityScore)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToList();
        for (var i = 0; i < routeCameraSafetySuspects.Count; i++)
        {
            var finding = routeCameraSafetySuspects[i];
            builder.AppendLine("    {");
            builder.AppendLine("      \"path\": \"" + EscapeJson(finding.Path) + "\",");
            builder.AppendLine("      \"rendererType\": \"" + EscapeJson(finding.RendererType) + "\",");
            builder.AppendLine("      \"role\": \"" + EscapeJson(finding.Role) + "\",");
            builder.AppendLine("      \"severity\": \"" + EscapeJson(finding.Severity.ToString()) + "\",");
            builder.AppendLine("      \"rule\": \"" + EscapeJson(finding.Rule) + "\",");
            builder.AppendLine("      \"routeMeters\": " + FormatJson(finding.RouteMeters) + ",");
            builder.AppendLine("      \"estimatedTimeSeconds\": " + FormatJson(finding.EstimatedTimeSeconds) + ",");
            builder.AppendLine("      \"minVisibleDistanceMeters\": " + FormatJson(finding.MinVisibleDistanceMeters) + ",");
            builder.AppendLine("      \"routeCorridorClearanceMeters\": " + FormatJson(finding.RouteCorridorClearanceMeters) + ",");
            builder.AppendLine("      \"screenRect\": \"" + EscapeJson(FormatRect(finding.ScreenRect)) + "\",");
            builder.AppendLine("      \"dominanceRatio\": " + FormatJson(finding.DominanceRatio) + ",");
            builder.AppendLine("      \"protectedOverlapRatio\": " + FormatJson(finding.ProtectedOverlapRatio) + ",");
            builder.AppendLine("      \"elevatedRouteOverlapRatio\": " + FormatJson(finding.ElevatedRouteOverlapRatio) + ",");
            builder.AppendLine("      \"visibilityScore\": " + FormatJson(finding.VisibilityScore) + ",");
            builder.AppendLine("      \"reason\": \"" + EscapeJson(finding.Reason) + "\",");
            builder.AppendLine("      \"recommendedAction\": \"" + EscapeJson(finding.RecommendedAction) + "\"");
            builder.Append("    }");
            if (i < routeCameraSafetySuspects.Count - 1)
            {
                builder.Append(",");
            }

            builder.AppendLine();
        }

        builder.AppendLine("  ]");
        builder.AppendLine("}");
        File.WriteAllText(ToRepoPath(MetricsRelativePath), builder.ToString());
    }

    private static string ToProjectPath(string assetRelativePath)
    {
        if (!assetRelativePath.StartsWith("Assets/", StringComparison.Ordinal))
        {
            return assetRelativePath;
        }

        return Path.Combine(Application.dataPath, assetRelativePath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
    }

    private static string ToRepoPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../..", relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    private static string GetRepoRoot()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../../.."));
    }

    private static string FormatFloat(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string FormatJson(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static string FormatRect(Rect rect)
    {
        return "x="
            + FormatFloat(rect.xMin)
            + ".."
            + FormatFloat(rect.xMax)
            + ", y="
            + FormatFloat(rect.yMin)
            + ".."
            + FormatFloat(rect.yMax);
    }

    private static string BoolJson(bool value)
    {
        return value ? "true" : "false";
    }

    private static string BoolText(bool value)
    {
        return value ? "true" : "false";
    }

    private static string YesNo(bool value)
    {
        return value ? "Yes" : "No";
    }

    private static string EscapeJson(string value)
    {
        return (value ?? string.Empty)
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string EscapeMarkdown(string value)
    {
        return (value ?? string.Empty).Replace("|", "\\|");
    }

    private readonly struct AllowlistRule
    {
        public AllowlistRule(string id, string description)
        {
            Id = id;
            Description = description;
        }

        public string Id { get; }
        public string Description { get; }
    }

    private sealed class RouteCameraSnapshot
    {
        public float Meters;
        public Vector3 CameraPosition;
        public Vector3 CameraForward;
        public float NearClipPlane;
        public Plane[] FrustumPlanes;
        public Matrix4x4 WorldToCameraMatrix;
        public Matrix4x4 ProjectionMatrix;
    }

    public sealed class RendererRecord
    {
        public string Name = "";
        public string Path = "";
        public string AssetKey = "";
        public string RendererType = "";
        public Bounds Bounds;
        public float GroundY;
        public float RouteGroundY;
        public float BottomClearance;
        public float RouteBottomClearance;
        public string GroundSource = "";
        public float UpFacingSurfaceRatio;
        public string ExclusionReason = "";
        public bool Synthetic;
        public bool RouteVisible;
        public int VisibleSampleCount;
        public float FirstVisibleMeters = -1f;
        public float MinVisibleDistanceMeters = float.MaxValue;
        public float RouteCorridorClearanceMeters = float.MaxValue;
        public readonly List<float> VisibleMeters = new List<float>();
        public Rect WorstScreenRect;
        public float MaxViewportDominanceRatio;
        public float MaxProtectedZoneOverlapRatio;
        public float MaxElevatedRouteOverlapRatio;
        public float WorstVisibilityScore;
        public float WorstVisibilityMeters = -1f;

        public void Observe(
            float meters,
            float distance,
            Rect screenRect,
            float viewportDominanceRatio,
            float protectedZoneOverlapRatio,
            float elevatedRouteOverlapRatio)
        {
            if (!RouteVisible)
            {
                FirstVisibleMeters = meters;
            }

            RouteVisible = true;
            VisibleSampleCount++;
            if (!VisibleMeters.Any(existing => Mathf.Abs(existing - meters) <= SupportCoVisibleSampleToleranceMeters))
            {
                VisibleMeters.Add(meters);
            }

            MinVisibleDistanceMeters = Mathf.Min(MinVisibleDistanceMeters, distance);
            MaxViewportDominanceRatio = Mathf.Max(MaxViewportDominanceRatio, viewportDominanceRatio);
            MaxProtectedZoneOverlapRatio = Mathf.Max(MaxProtectedZoneOverlapRatio, protectedZoneOverlapRatio);
            MaxElevatedRouteOverlapRatio = Mathf.Max(MaxElevatedRouteOverlapRatio, elevatedRouteOverlapRatio);

            var visibilityScore = viewportDominanceRatio * 0.50f
                + protectedZoneOverlapRatio * 0.25f
                + elevatedRouteOverlapRatio * 0.25f;
            if (visibilityScore > WorstVisibilityScore)
            {
                WorstVisibilityScore = visibilityScore;
                WorstVisibilityMeters = meters;
                WorstScreenRect = screenRect;
            }
        }
    }

    public enum FindingSeverity
    {
        Excluded,
        GroundedPass,
        SupportedPass,
        Warning,
        Blocking
    }

    private enum RouteCameraSafetyExemptionMode
    {
        None,
        Full,
        CorridorOnly
    }

    public sealed class SupportFinding
    {
        public SupportFinding(RendererRecord record, FindingSeverity severity, string reason)
        {
            Path = record.Path;
            AssetKey = record.AssetKey;
            RendererType = record.RendererType;
            Severity = severity;
            Reason = reason;
            BottomClearance = record.BottomClearance;
            RouteBottomClearance = record.RouteBottomClearance;
            GroundY = record.GroundY;
            RouteGroundY = record.RouteGroundY;
            BoundsMinY = record.Bounds.min.y;
            GroundSource = record.GroundSource;
            FirstVisibleMeters = record.FirstVisibleMeters;
            MinVisibleDistanceMeters = record.MinVisibleDistanceMeters == float.MaxValue ? 0f : record.MinVisibleDistanceMeters;
        }

        public string Path;
        public string AssetKey;
        public string RendererType;
        public FindingSeverity Severity;
        public string Reason;
        public float BottomClearance;
        public float RouteBottomClearance;
        public float GroundY;
        public float RouteGroundY;
        public float BoundsMinY;
        public string GroundSource;
        public float FirstVisibleMeters;
        public float MinVisibleDistanceMeters;
        public string NearestSupportPath = "";
        public float SupportHorizontalGapMeters;
        public float SupportVerticalGapMeters;
        public float SupportCoVisibleMeters = -1f;
        public bool IsUnsupported => Severity == FindingSeverity.Warning || Severity == FindingSeverity.Blocking;
    }

    public sealed class RouteCameraSafetyFinding
    {
        public RouteCameraSafetyFinding(
            RendererRecord record,
            FindingSeverity severity,
            string rule,
            string reason,
            string recommendedAction)
        {
            Path = record.Path;
            RendererType = record.RendererType;
            Role = string.IsNullOrEmpty(record.ExclusionReason) ? "scenic-or-unclassified-renderer" : record.ExclusionReason;
            Severity = severity;
            Rule = rule;
            Reason = reason;
            RecommendedAction = recommendedAction;
            RouteMeters = record.WorstVisibilityMeters >= 0f ? record.WorstVisibilityMeters : record.FirstVisibleMeters;
            EstimatedTimeSeconds = RouteMeters <= 0f ? 0f : RouteMeters / RouteCameraSafetyEstimatedMetersPerSecond;
            MinVisibleDistanceMeters = record.MinVisibleDistanceMeters == float.MaxValue ? 0f : record.MinVisibleDistanceMeters;
            RouteCorridorClearanceMeters = record.RouteCorridorClearanceMeters;
            DominanceRatio = record.MaxViewportDominanceRatio;
            ProtectedOverlapRatio = record.MaxProtectedZoneOverlapRatio;
            ElevatedRouteOverlapRatio = record.MaxElevatedRouteOverlapRatio;
            VisibilityScore = record.WorstVisibilityScore;
            ScreenRect = record.WorstScreenRect;
        }

        public string Path;
        public string RendererType;
        public string Role;
        public FindingSeverity Severity;
        public string Rule;
        public string Reason;
        public string RecommendedAction;
        public float RouteMeters;
        public float EstimatedTimeSeconds;
        public float MinVisibleDistanceMeters;
        public float RouteCorridorClearanceMeters;
        public float DominanceRatio;
        public float ProtectedOverlapRatio;
        public float ElevatedRouteOverlapRatio;
        public float VisibilityScore;
        public Rect ScreenRect;
    }

    public sealed class ValidationMessage
    {
        public string Code = "";
        public string Subject = "";
        public string Message = "";
        public string Action = "";
    }

    public sealed class ValidationResult
    {
        public string ExecutionMode = "";
        public string GeneratedAt = "";
        public string RepoRoot = "";
        public string ScenePath = "";
        public string ReportPathRelative = "";
        public string MetricsPathRelative = "";
        public string Myb144ReportPathRelative = "";
        public string GroundSource = "";
        public bool SceneDirtyBeforeValidation;
        public bool SceneDirtyAfterValidation;
        public float RouteLengthMeters;
        public int RoutePointCount;
        public int RouteCameraSampleCount;
        public int TotalRendererCount;
        public int RouteVisibleRendererCount;
        public int RouteVisibleAssetCount;
        public int ExcludedSystemRendererCount;
        public int GroundedPassCount;
        public int SupportRequiredCount;
        public int SupportedPassCount;
        public int UnsupportedWarningCount;
        public int UnsupportedBlockingCount;
        public float MaxUnsupportedBottomClearance;
        public float MaxUnsupportedVisibleDistanceMeters;
        public int RouteCorridorIntrusionCount;
        public int RouteReadabilityWarningCount;
        public int RouteReadabilityBlockingCount;
        public float WorstRouteVisibilityScore;
        public bool FixedPrefixOnlyDetectionUsed;
        public int AllowlistEntryCount;
        public bool AllowlistEntriesDocumented;
        public string FixtureVerdict = "Not run";
        public bool FixtureUnsupportedArbitraryDetected;
        public bool FixtureSupportedCounterpartPassed;
        public bool FixtureFloatingVerticalSupportDetected;
        public string RouteCameraSafetyFixtureVerdict = "Not run";
        public bool RouteCameraSafetyFixtureDominantBlockerDetected;
        public bool RouteCameraSafetyFixtureCorridorIntruderDetected;
        public bool RouteCameraSafetyFixtureNearPlaneBlockerDetected;
        public bool RouteCameraSafetyFixtureBenignMarkerPassed;
        public bool RouteCameraSafetyFixtureCloseScenicFramingPassed;
        public string Myb144Verdict = "Not run";
        public int Myb144Errors;
        public int Myb144Warnings;
        public readonly List<SupportFinding> Findings = new List<SupportFinding>();
        public readonly List<SupportFinding> FixtureFindings = new List<SupportFinding>();
        public readonly List<RouteCameraSafetyFinding> RouteCameraSafetyFindings = new List<RouteCameraSafetyFinding>();
        public readonly List<RouteCameraSafetyFinding> RouteCameraSafetyFixtureFindings = new List<RouteCameraSafetyFinding>();
        public readonly List<ValidationMessage> BlockingErrors = new List<ValidationMessage>();
        public readonly List<ValidationMessage> Warnings = new List<ValidationMessage>();
        public readonly List<ValidationMessage> Info = new List<ValidationMessage>();

        public int BlockingErrorCount => BlockingErrors.Count;
        public int WarningCount => Warnings.Count;
        public string RouteCameraSafetyVerdict =>
            RouteReadabilityBlockingCount > 0 || RouteCameraSafetyFixtureVerdict == "FAIL"
                ? "FAIL"
                : RouteReadabilityWarningCount > 0 ? "PASS_WITH_WARNINGS" : "PASS";
        public bool HasBlockingFailure =>
            BlockingErrorCount > 0
            || UnsupportedBlockingCount > 0
            || RouteReadabilityBlockingCount > 0
            || FixtureVerdict == "FAIL"
            || RouteCameraSafetyFixtureVerdict == "FAIL";
        public string Verdict => HasBlockingFailure ? "FAIL" : WarningCount > 0 ? "PASS_WITH_WARNINGS" : "PASS";

        public void AddBlocking(string code, string subject, string message, string action)
        {
            BlockingErrors.Add(new ValidationMessage { Code = code, Subject = subject, Message = message, Action = action });
        }

        public void AddWarning(string code, string subject, string message, string action)
        {
            Warnings.Add(new ValidationMessage { Code = code, Subject = subject, Message = message, Action = action });
        }

        public void AddInfo(string code, string message)
        {
            Info.Add(new ValidationMessage { Code = code, Subject = "Info", Message = message, Action = "None." });
        }

        public string ToConsoleSummary()
        {
            return "MYB-167 route-camera safety validator: "
                + Verdict
                + " | routeVisibleRenderers="
                + RouteVisibleRendererCount.ToString(CultureInfo.InvariantCulture)
                + " | unsupportedBlockingCount="
                + UnsupportedBlockingCount.ToString(CultureInfo.InvariantCulture)
                + " | unsupportedWarningCount="
                + UnsupportedWarningCount.ToString(CultureInfo.InvariantCulture)
                + " | routeReadabilityBlockingCount="
                + RouteReadabilityBlockingCount.ToString(CultureInfo.InvariantCulture)
                + " | routeReadabilityWarningCount="
                + RouteReadabilityWarningCount.ToString(CultureInfo.InvariantCulture)
                + " | supportFixture="
                + FixtureVerdict
                + " | routeCameraSafetyFixture="
                + RouteCameraSafetyFixtureVerdict
                + " | report="
                + ReportPathRelative;
        }
    }
}
