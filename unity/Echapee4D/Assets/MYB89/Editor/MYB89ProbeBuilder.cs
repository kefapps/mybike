using System;
using System.Collections.Generic;
using System.IO;
using MYB89;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MYB89.Editor
{
    public static class MYB89ProbeBuilder
    {
        private const string ScenePath = "Assets/Scenes/MYB89UnityMcpProbe.unity";
        private const string CaptureIndexPath = "_bmad-output/video-captures/myb-89-unity-mcp-probe-latest.txt";
        private const float RoadWidth = 7.2f;

        private static readonly Vector3[] RoutePoints =
        {
            new Vector3(0f, 0.12f, 0f),
            new Vector3(2f, 0.16f, 24f),
            new Vector3(-4f, 0.26f, 55f),
            new Vector3(-7f, 0.38f, 88f),
            new Vector3(-1f, 0.2f, 118f),
            new Vector3(6f, 0.14f, 150f),
            new Vector3(3f, 0.22f, 184f),
            new Vector3(-3f, 0.28f, 215f),
            new Vector3(0f, 0.12f, 242f)
        };

        [MenuItem("Tools/MYB-89/Build Unity MCP Probe Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/MYB89");
            EnsureFolder("Assets/MYB89/Materials");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MYB89UnityMcpProbe";

            var materials = CreateMaterials();
            var root = new GameObject("MYB89_ProbeRoot");

            ConfigureRenderSettings();
            CreateTerrain(root.transform, materials);
            var routeMarkers = CreateRoute(root.transform, materials);
            CreateRoadsideRhythm(root.transform, materials);
            var keyLight = CreateLighting(root.transform);
            CreateCameraRig(root.transform, routeMarkers, keyLight, materials);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"MYB-89 probe scene built at {ScenePath} with {routeMarkers.Count} route markers.");
        }

        [MenuItem("Tools/MYB-89/Validate Unity MCP Probe Scene")]
        public static string ValidateProbeScene()
        {
            OpenProbeScene();

            var failures = new List<string>();
            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            var road = GameObject.Find("MYB89_RouteRoad");
            var hud = GameObject.Find("MYB89_HUD");
            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);

            if (ride == null)
            {
                failures.Add("Missing MYB89ProbeRide component.");
            }
            else
            {
                ride.RebuildRouteCache();

                if (ride.routeMarkers == null || ride.routeMarkers.Length < 8)
                {
                    failures.Add("Route needs at least 8 route markers.");
                }

                if (ride.RouteLength < 200f)
                {
                    failures.Add($"Route length too short: {ride.RouteLength:0.0} m.");
                }

                if (ride.distanceLabel == null || ride.speedLabel == null || ride.verdictLabel == null)
                {
                    failures.Add("HUD labels are not wired to MYB89ProbeRide.");
                }
            }

            if (camera == null)
            {
                failures.Add("Missing Main Camera.");
            }

            if (road == null)
            {
                failures.Add("Missing MYB89_RouteRoad mesh.");
            }

            if (hud == null)
            {
                failures.Add("Missing MYB89_HUD canvas.");
            }

            if (renderers.Length < 55)
            {
                failures.Add($"Scene has too few renderers for readable motion: {renderers.Length}.");
            }

            var report = WriteValidationReport(failures, ride, renderers.Length);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-89 probe validation failed. See " + report);
            }

            Debug.Log("MYB-89 probe validation passed. Report: " + report);
            return report;
        }

        [MenuItem("Tools/MYB-89/Capture Unity MCP Probe Frames")]
        public static string CaptureProbeFrames()
        {
            OpenProbeScene();

            var ride = UnityEngine.Object.FindAnyObjectByType<MYB89ProbeRide>();
            var camera = Camera.main;
            if (ride == null || camera == null)
            {
                throw new InvalidOperationException("Probe scene must contain MYB89ProbeRide and Main Camera before capture.");
            }

            ride.RebuildRouteCache();
            var routeLength = Mathf.Max(1f, ride.RouteLength);
            var repoRoot = GetRepoRoot();
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var relativeCaptureDir = Path.Combine("_bmad-output", "video-captures", "myb-89-unity-mcp-probe-" + stamp);
            var absoluteCaptureDir = Path.Combine(repoRoot, relativeCaptureDir);
            Directory.CreateDirectory(absoluteCaptureDir);

            SetCanvasCamera(camera);

            const int width = 1280;
            const int height = 720;
            const int frameCount = 72;
            for (var i = 0; i < frameCount; i++)
            {
                var normalized = i / (float)frameCount;
                ride.SetPreviewProgress(normalized * routeLength);
                Canvas.ForceUpdateCanvases();

                var framePath = Path.Combine(absoluteCaptureDir, $"frame_{i:000}.png");
                RenderCameraToPng(camera, width, height, framePath);
            }

            var screenshotPath = Path.Combine(absoluteCaptureDir, "screenshot.png");
            File.Copy(Path.Combine(absoluteCaptureDir, "frame_012.png"), screenshotPath, true);

            var latestPath = Path.Combine(repoRoot, CaptureIndexPath);
            Directory.CreateDirectory(Path.GetDirectoryName(latestPath) ?? repoRoot);
            File.WriteAllText(latestPath, absoluteCaptureDir);

            Debug.Log($"MYB-89 probe captured {frameCount} frames at {absoluteCaptureDir}");
            return absoluteCaptureDir;
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                { "road", MaterialAt("Assets/MYB89/Materials/MYB89_Road.mat", new Color(0.13f, 0.15f, 0.14f), 0.12f) },
                { "lane", MaterialAt("Assets/MYB89/Materials/MYB89_LanePaint.mat", new Color(0.96f, 0.91f, 0.73f), 0.04f) },
                { "edge", MaterialAt("Assets/MYB89/Materials/MYB89_EdgeLine.mat", new Color(0.88f, 0.96f, 0.95f), 0.04f) },
                { "grass", MaterialAt("Assets/MYB89/Materials/MYB89_Grass.mat", new Color(0.19f, 0.42f, 0.27f), 0f) },
                { "hill", MaterialAt("Assets/MYB89/Materials/MYB89_Hill.mat", new Color(0.31f, 0.5f, 0.35f), 0f) },
                { "postBlue", MaterialAt("Assets/MYB89/Materials/MYB89_PostBlue.mat", new Color(0.15f, 0.45f, 0.92f), 0.18f) },
                { "postCoral", MaterialAt("Assets/MYB89/Materials/MYB89_PostCoral.mat", new Color(0.95f, 0.31f, 0.24f), 0.18f) },
                { "postWhite", MaterialAt("Assets/MYB89/Materials/MYB89_PostWhite.mat", new Color(0.93f, 0.95f, 0.91f), 0.08f) },
                { "treeTrunk", MaterialAt("Assets/MYB89/Materials/MYB89_TreeTrunk.mat", new Color(0.31f, 0.2f, 0.12f), 0f) },
                { "treeLeaf", MaterialAt("Assets/MYB89/Materials/MYB89_TreeLeaf.mat", new Color(0.1f, 0.33f, 0.2f), 0f) },
                { "cockpit", MaterialAt("Assets/MYB89/Materials/MYB89_Cockpit.mat", new Color(0.04f, 0.045f, 0.05f), 0.35f) },
                { "banner", MaterialAt("Assets/MYB89/Materials/MYB89_Banner.mat", new Color(0.97f, 0.74f, 0.24f), 0.2f) }
            };
        }

        private static Material MaterialAt(string assetPath, Color color, float smoothness)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, assetPath);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.44f, 0.38f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.2f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.71f, 0.74f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 235f;
        }

        private static void CreateTerrain(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "MYB89_GroundPlane";
            ground.transform.SetParent(parent);
            ground.transform.position = new Vector3(0f, 0f, 118f);
            ground.transform.localScale = new Vector3(18f, 1f, 28f);
            SetMaterial(ground, materials["grass"]);

            CreateHill("MYB89_LeftHill_A", parent, new Vector3(-38f, 2.5f, 50f), new Vector3(26f, 5f, 18f), materials["hill"]);
            CreateHill("MYB89_RightHill_A", parent, new Vector3(36f, 3.5f, 92f), new Vector3(30f, 7f, 20f), materials["hill"]);
            CreateHill("MYB89_LeftHill_B", parent, new Vector3(-30f, 4f, 175f), new Vector3(34f, 8f, 25f), materials["hill"]);
            CreateHill("MYB89_RightHill_B", parent, new Vector3(42f, 4f, 210f), new Vector3(32f, 8f, 22f), materials["hill"]);
        }

        private static void CreateHill(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = name;
            hill.transform.SetParent(parent);
            hill.transform.position = position;
            hill.transform.localScale = scale;
            SetMaterial(hill, material);
        }

        private static List<Transform> CreateRoute(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            var markers = new List<Transform>();
            var markerRoot = new GameObject("MYB89_RouteMarkers");
            markerRoot.transform.SetParent(parent);

            for (var i = 0; i < RoutePoints.Length; i++)
            {
                var marker = new GameObject($"RouteMarker_{i:00}");
                marker.transform.SetParent(markerRoot.transform);
                marker.transform.position = RoutePoints[i];
                markers.Add(marker.transform);
            }

            CreateStripMesh("MYB89_RouteRoad", parent, RoutePoints, 0f, RoadWidth, 0.03f, materials["road"]);
            CreateStripMesh("MYB89_LeftEdgeLine", parent, RoutePoints, -RoadWidth * 0.5f + 0.16f, 0.12f, 0.07f, materials["edge"]);
            CreateStripMesh("MYB89_RightEdgeLine", parent, RoutePoints, RoadWidth * 0.5f - 0.16f, 0.12f, 0.07f, materials["edge"]);

            for (var distance = 8f; distance < RouteLength() - 10f; distance += 13f)
            {
                if (!SampleRoute(distance, out var position, out var forward, out _))
                {
                    continue;
                }

                CreateCube(
                    "MYB89_CenterDash",
                    parent,
                    position + Vector3.up * 0.1f,
                    new Vector3(0.18f, 0.035f, 4.8f),
                    Quaternion.LookRotation(forward, Vector3.up),
                    materials["lane"]);
            }

            return markers;
        }

        private static void CreateStripMesh(string name, Transform parent, IReadOnlyList<Vector3> points, float lateralOffset, float width, float yLift, Material material)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);

            var vertices = new Vector3[points.Count * 2];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[(points.Count - 1) * 6];

            for (var i = 0; i < points.Count; i++)
            {
                var tangent = TangentAt(points, i);
                var right = Vector3.Cross(Vector3.up, tangent).normalized;
                var center = points[i] + right * lateralOffset + Vector3.up * yLift;

                vertices[i * 2] = center - right * (width * 0.5f);
                vertices[i * 2 + 1] = center + right * (width * 0.5f);
                normals[i * 2] = Vector3.up;
                normals[i * 2 + 1] = Vector3.up;
                uvs[i * 2] = new Vector2(0f, i);
                uvs[i * 2 + 1] = new Vector2(1f, i);
            }

            var triangleIndex = 0;
            for (var i = 0; i < points.Count - 1; i++)
            {
                var leftA = i * 2;
                var rightA = i * 2 + 1;
                var leftB = (i + 1) * 2;
                var rightB = (i + 1) * 2 + 1;

                triangles[triangleIndex++] = leftA;
                triangles[triangleIndex++] = leftB;
                triangles[triangleIndex++] = rightA;
                triangles[triangleIndex++] = rightA;
                triangles[triangleIndex++] = leftB;
                triangles[triangleIndex++] = rightB;
            }

            var mesh = new Mesh { name = name + "Mesh" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateRoadsideRhythm(Transform parent, IReadOnlyDictionary<string, Material> materials)
        {
            for (var index = 0; index < 13; index++)
            {
                var distance = 14f + index * 17f;
                var material = index % 2 == 0 ? materials["postBlue"] : materials["postCoral"];
                CreateMarkerPair(parent, distance, index, material, materials["postWhite"]);
            }

            CreateCheckpointArch(parent, 34f, "WARMUP", materials["banner"], materials["postWhite"]);
            CreateCheckpointArch(parent, 112f, "CLIMB", materials["banner"], materials["postWhite"]);
            CreateCheckpointArch(parent, 190f, "SPRINT", materials["banner"], materials["postWhite"]);

            for (var i = 0; i < 20; i++)
            {
                var distance = 10f + i * 11.5f;
                if (!SampleRoute(distance, out var position, out _, out var right))
                {
                    continue;
                }

                var side = i % 2 == 0 ? -1f : 1f;
                var offset = right * side * (RoadWidth * 0.5f + 5f + (i % 3) * 1.7f);
                CreateTree($"MYB89_Tree_{i:00}", parent, position + offset, materials["treeTrunk"], materials["treeLeaf"]);
            }
        }

        private static void CreateMarkerPair(Transform parent, float distance, int index, Material postMaterial, Material capMaterial)
        {
            if (!SampleRoute(distance, out var position, out var forward, out var right))
            {
                return;
            }

            for (var side = -1; side <= 1; side += 2)
            {
                var basePosition = position + right * side * (RoadWidth * 0.5f + 1.15f);
                var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pole.name = $"MYB89_ClosePost_{index:00}_{(side < 0 ? "L" : "R")}";
                pole.transform.SetParent(parent);
                pole.transform.position = basePosition + Vector3.up * 0.72f;
                pole.transform.localScale = new Vector3(0.16f, 0.72f, 0.16f);
                SetMaterial(pole, postMaterial);

                CreateCube(
                    $"MYB89_PostCap_{index:00}_{(side < 0 ? "L" : "R")}",
                    parent,
                    basePosition + Vector3.up * 1.52f,
                    new Vector3(0.5f, 0.18f, 0.5f),
                    Quaternion.LookRotation(forward, Vector3.up),
                    capMaterial);
            }
        }

        private static void CreateCheckpointArch(Transform parent, float distance, string label, Material beamMaterial, Material postMaterial)
        {
            if (!SampleRoute(distance, out var position, out var forward, out var right))
            {
                return;
            }

            var rotation = Quaternion.LookRotation(forward, Vector3.up);
            var left = position - right * (RoadWidth * 0.5f + 0.8f);
            var rightPosition = position + right * (RoadWidth * 0.5f + 0.8f);
            CreateCube("MYB89_ArchPost_" + label + "_L", parent, left + Vector3.up * 2.1f, new Vector3(0.35f, 4.2f, 0.35f), rotation, postMaterial);
            CreateCube("MYB89_ArchPost_" + label + "_R", parent, rightPosition + Vector3.up * 2.1f, new Vector3(0.35f, 4.2f, 0.35f), rotation, postMaterial);
            CreateCube("MYB89_ArchBeam_" + label, parent, position + Vector3.up * 4.25f, new Vector3(RoadWidth + 2.1f, 0.42f, 0.55f), rotation, beamMaterial);
        }

        private static void CreateTree(string name, Transform parent, Vector3 position, Material trunkMaterial, Material leafMaterial)
        {
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = name + "_Trunk";
            trunk.transform.SetParent(parent);
            trunk.transform.position = position + Vector3.up * 0.85f;
            trunk.transform.localScale = new Vector3(0.26f, 0.85f, 0.26f);
            SetMaterial(trunk, trunkMaterial);

            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crown.name = name + "_Crown";
            crown.transform.SetParent(parent);
            crown.transform.position = position + Vector3.up * 2.2f;
            crown.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            SetMaterial(crown, leafMaterial);
        }

        private static Light CreateLighting(Transform parent)
        {
            var sun = new GameObject("MYB89_KeySun");
            sun.transform.SetParent(parent);
            sun.transform.rotation = Quaternion.Euler(42f, -34f, 0f);

            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.08f;
            light.color = new Color(1f, 0.92f, 0.78f);
            light.shadows = LightShadows.Soft;
            return light;
        }

        private static void CreateCameraRig(Transform parent, IReadOnlyList<Transform> routeMarkers, Light keyLight, IReadOnlyDictionary<string, Material> materials)
        {
            var rig = new GameObject("MYB89_RiderRig");
            rig.transform.SetParent(parent);
            rig.transform.position = RoutePoints[0];

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(rig.transform);
            cameraObject.transform.localPosition = new Vector3(0f, 1.55f, -1.75f);
            cameraObject.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 69f;
            camera.nearClipPlane = 0.03f;
            camera.farClipPlane = 320f;
            camera.clearFlags = CameraClearFlags.Skybox;

            cameraObject.AddComponent<AudioListener>();

            CreateCockpit(rig.transform, materials["cockpit"]);
            var hud = CreateHud(parent, camera, out var distanceLabel, out var speedLabel, out var verdictLabel);

            var ride = rig.AddComponent<MYB89ProbeRide>();
            ride.routeMarkers = new Transform[routeMarkers.Count];
            for (var i = 0; i < routeMarkers.Count; i++)
            {
                ride.routeMarkers[i] = routeMarkers[i];
            }

            ride.cameraPivot = cameraObject.transform;
            ride.distanceLabel = distanceLabel;
            ride.speedLabel = speedLabel;
            ride.verdictLabel = verdictLabel;
            ride.keyLight = keyLight;
            ride.speedMetersPerSecond = 12.5f;
            ride.SetPreviewProgress(0f);

            hud.name = "MYB89_HUD";
        }

        private static void CreateCockpit(Transform parent, Material material)
        {
            CreateCube("MYB89_Handlebar_Left", parent, new Vector3(-0.45f, 1.05f, 0.46f), new Vector3(0.85f, 0.08f, 0.08f), Quaternion.identity, material);
            CreateCube("MYB89_Handlebar_Right", parent, new Vector3(0.45f, 1.05f, 0.46f), new Vector3(0.85f, 0.08f, 0.08f), Quaternion.identity, material);
            CreateCube("MYB89_Stem", parent, new Vector3(0f, 0.97f, 0.25f), new Vector3(0.11f, 0.11f, 0.55f), Quaternion.identity, material);
        }

        private static GameObject CreateHud(Transform parent, Camera camera, out Text distanceLabel, out Text speedLabel, out Text verdictLabel)
        {
            var canvasObject = new GameObject("MYB89_HUD", typeof(RectTransform));
            canvasObject.transform.SetParent(parent);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.7f;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            distanceLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Distance", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -22f), new Vector2(330f, 48f), TextAnchor.UpperLeft, 28);
            speedLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Speed", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-26f, -22f), new Vector2(220f, 48f), TextAnchor.UpperRight, 30);
            verdictLabel = CreateHudText(canvasObject.transform, "MYB89_HUD_Verdict", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(680f, 42f), TextAnchor.LowerCenter, 22);

            return canvasObject;
        }

        private static Text CreateHudText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, TextAnchor alignment, int fontSize)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            var rect = (RectTransform)textObject.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.97f, 0.98f, 0.94f, 1f);
            text.raycastTarget = false;

            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.68f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return text;
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.SetPositionAndRotation(position, rotation);
            cube.transform.localScale = scale;
            SetMaterial(cube, material);
            return cube;
        }

        private static void SetMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static Vector3 TangentAt(IReadOnlyList<Vector3> points, int index)
        {
            if (index <= 0)
            {
                return (points[1] - points[0]).normalized;
            }

            if (index >= points.Count - 1)
            {
                return (points[index] - points[index - 1]).normalized;
            }

            return (points[index + 1] - points[index - 1]).normalized;
        }

        private static bool SampleRoute(float meters, out Vector3 position, out Vector3 forward, out Vector3 right)
        {
            var routeLength = RouteLength();
            var wrappedMeters = Mathf.Repeat(meters, routeLength);
            var cursor = 0f;

            for (var i = 0; i < RoutePoints.Length - 1; i++)
            {
                var a = RoutePoints[i];
                var b = RoutePoints[i + 1];
                var segmentLength = Vector3.Distance(a, b);

                if (wrappedMeters <= cursor + segmentLength || i == RoutePoints.Length - 2)
                {
                    var t = Mathf.InverseLerp(cursor, cursor + segmentLength, wrappedMeters);
                    position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t));
                    forward = (b - a).normalized;
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                    return true;
                }

                cursor += segmentLength;
            }

            position = RoutePoints[0];
            forward = Vector3.forward;
            right = Vector3.right;
            return false;
        }

        private static float RouteLength()
        {
            var length = 0f;
            for (var i = 0; i < RoutePoints.Length - 1; i++)
            {
                length += Vector3.Distance(RoutePoints[i], RoutePoints[i + 1]);
            }

            return length;
        }

        private static void SetCanvasCamera(Camera camera)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.worldCamera = camera;
                }
            }
        }

        private static void RenderCameraToPng(Camera camera, int width, int height, string path)
        {
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void OpenProbeScene()
        {
            if (!File.Exists(ProjectRelativeToAbsolute(ScenePath)))
            {
                BuildScene();
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static string WriteValidationReport(IReadOnlyList<string> failures, MYB89ProbeRide ride, int rendererCount)
        {
            var repoRoot = GetRepoRoot();
            var reportDir = Path.Combine(repoRoot, "_bmad-output", "unity-test-results");
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, "myb-89-unity-mcp-probe-validator.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-89 Unity-MCP IvanMurzak probe validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Scene: " + ScenePath);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Route markers: " + (ride == null || ride.routeMarkers == null ? 0 : ride.routeMarkers.Length));
                writer.WriteLine("Route length: " + (ride == null ? 0f : ride.RouteLength).ToString("0.0") + " m");
                writer.WriteLine("Renderer count: " + rendererCount);

                if (failures.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Failures:");
                    foreach (var failure in failures)
                    {
                        writer.WriteLine("- " + failure);
                    }
                }
            }

            return reportPath;
        }

        private static string ProjectRelativeToAbsolute(string assetPath)
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            return Path.Combine(projectRoot == null ? Application.dataPath : projectRoot.FullName, assetPath);
        }

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            if (projectRoot == null || projectRoot.Parent == null || projectRoot.Parent.Parent == null)
            {
                return projectRoot == null ? Application.dataPath : projectRoot.FullName;
            }

            return projectRoot.Parent.Parent.FullName;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];

            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
