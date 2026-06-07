using MyBike.Echappee3D.Bootstrap;
using MyBike.Echappee3D.Rendering;
using MyBike.Echappee3D.Route;
using MyBike.Echappee3D.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MyBike.Echappee3D.EditorTools
{
    public static class RideMockSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/RideMock.unity";
        private const string RouteMaterialPath = "Assets/Materials/RoutePlaceholder.mat";
        private const string BirdMaterialPath = "Assets/Materials/SceneLifeBird.mat";
        private const string HumanMaterialPath = "Assets/Materials/SceneLifeHuman.mat";

        [MenuItem("Echappee/MYB-14/Rebuild RideMock Scene")]
        public static void Build()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var routeMaterial = CreateRouteMaterial();

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.78f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 220f;

            var camera = CreateCamera();
            var route = CreateRoute(routeMaterial);
            var fog = CreateFog();
            CreateSceneLife();
            var ui = CreateUi();

            route.renderer.EnsureVisible(RouteMath.CreateDefaultMockRoute());
            CreateRideSession(camera.controller, route.renderer, fog, ui.input, ui.hud, ui.controls);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Echappee/MYB-13/Rebuild RideMock Scene")]
        public static void BuildMyb13()
        {
            Build();
        }

        [MenuItem("Echappee/MYB-12/Rebuild RideMock Scene")]
        public static void BuildLegacy()
        {
            Build();
        }

        private static (GameObject root, RideCameraController controller) CreateCamera()
        {
            var root = new GameObject("Main Camera");
            root.tag = "MainCamera";
            var camera = root.AddComponent<Camera>();
            camera.fieldOfView = 70f;
            root.transform.position = new Vector3(0f, 1.6f, -6f);
            root.transform.LookAt(new Vector3(0f, 1.2f, 20f));

            var controller = root.AddComponent<RideCameraController>();
            SetObjectReference(controller, "targetCamera", camera);
            return (root, controller);
        }

        private static (GameObject root, RouteRendererPlaceholder renderer) CreateRoute(Material material)
        {
            var root = new GameObject("Route");
            var line = root.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.widthMultiplier = 4f;
            var renderer = root.AddComponent<RouteRendererPlaceholder>();
            SetObjectReference(renderer, "routeMaterial", material);
            return (root, renderer);
        }

        private static DepthFogController CreateFog()
        {
            var root = new GameObject("Fog");
            return root.AddComponent<DepthFogController>();
        }

        private static LightweightSceneLife CreateSceneLife()
        {
            var birdMaterial = CreateMaterial(BirdMaterialPath, new Color(0.82f, 0.88f, 0.84f, 1f));
            var humanMaterial = CreateMaterial(HumanMaterialPath, new Color(0.08f, 0.11f, 0.12f, 1f));
            var root = new GameObject(LightweightSceneLife.RootName);
            var birdsRoot = new GameObject(LightweightSceneLife.BirdsRootName).transform;
            var humansRoot = new GameObject(LightweightSceneLife.HumansRootName).transform;
            birdsRoot.SetParent(root.transform, false);
            humansRoot.SetParent(root.transform, false);

            var birdPositions = new[]
            {
                new Vector3(-20f, 18f, 120f),
                new Vector3(18f, 22f, 260f),
                new Vector3(38f, 19f, 430f),
                new Vector3(-34f, 24f, 620f),
                new Vector3(20f, 21f, 790f)
            };

            for (var i = 0; i < birdPositions.Length; i += 1)
            {
                CreateBird(birdsRoot, $"Bird_{i + 1:00}", birdPositions[i], birdMaterial, i);
            }

            var route = RouteMath.CreateDefaultMockRoute();
            CreateHumanSilhouette(humansRoot, "HumanSilhouette_01", new Vector3(-14f, 0f, 180f), route, humanMaterial);
            CreateHumanSilhouette(humansRoot, "HumanSilhouette_02", new Vector3(52f, 0f, 420f), route, humanMaterial);
            CreateHumanSilhouette(humansRoot, "HumanSilhouette_03", new Vector3(24f, 0f, 760f), route, humanMaterial);

            var life = root.AddComponent<LightweightSceneLife>();
            life.Bind(birdsRoot, humansRoot);
            return life;
        }

        private static void CreateBird(
            Transform parent,
            string name,
            Vector3 position,
            Material material,
            int index)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, index * 18f - 25f, 0f);
            root.transform.localScale = Vector3.one * 2.2f;

            var meshFilter = root.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateBirdMesh(name);
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }

        private static Mesh CreateBirdMesh(string name)
        {
            var mesh = new Mesh
            {
                name = $"{name}_Mesh",
                vertices = new[]
                {
                    new Vector3(-0.8f, 0f, 0f),
                    new Vector3(0f, 0f, 0.16f),
                    new Vector3(0.8f, 0f, 0f),
                    new Vector3(-0.12f, 0.12f, 0f),
                    new Vector3(0.12f, 0.12f, 0f)
                },
                triangles = new[] { 0, 1, 3, 1, 2, 4 }
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateHumanSilhouette(
            Transform parent,
            string name,
            Vector3 lateralPosition,
            RouteDefinition route,
            Material material)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var progress01 = Mathf.Clamp01(lateralPosition.z / Mathf.Max(1f, route.lengthMeters));
            var ground = RouteMath.SamplePosition(route, progress01);
            root.transform.position = new Vector3(lateralPosition.x, ground.y, lateralPosition.z);

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            body.transform.localScale = new Vector3(0.42f, 0.78f, 0.32f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(body.GetComponent<Collider>());

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.78f, 0f);
            head.transform.localScale = Vector3.one * 0.36f;
            head.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(head.GetComponent<Collider>());
        }

        private static (MockRideInput input, HudController hud, RideControlPanel controls) CreateUi()
        {
            var canvasRoot = new GameObject("Canvas");
            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasRoot.AddComponent<CanvasScaler>();
            canvasRoot.AddComponent<GraphicRaycaster>();

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            var slider = CreateSlider(canvasRoot.transform);
            var start = CreateButton(canvasRoot.transform, "StartButton", "Start", new Vector2(16f, 64f));
            var pause = CreateButton(canvasRoot.transform, "PauseButton", "Pause", new Vector2(100f, 64f));
            var resume = CreateButton(canvasRoot.transform, "ResumeButton", "Resume", new Vector2(184f, 64f));
            var finish = CreateButton(canvasRoot.transform, "FinishButton", "Finish", new Vector2(284f, 64f));

            var speed = CreateText(canvasRoot.transform, "SpeedText", new Vector2(16f, -16f), 280f);
            var effort = CreateText(canvasRoot.transform, "EffortText", new Vector2(16f, -44f), 280f);
            var progress = CreateText(canvasRoot.transform, "ProgressText", new Vector2(16f, -72f), 280f);
            var distance = CreateText(canvasRoot.transform, "DistanceText", new Vector2(16f, -100f), 280f);
            var time = CreateText(canvasRoot.transform, "TimeText", new Vector2(16f, -128f), 280f);
            var source = CreateText(canvasRoot.transform, "SourceText", new Vector2(16f, -156f), 280f);
            var phase = CreateText(canvasRoot.transform, "PhaseText", new Vector2(16f, -184f), 280f);
            var summary = CreateText(canvasRoot.transform, "SummaryText", new Vector2(16f, -224f), 520f);

            var input = canvasRoot.AddComponent<MockRideInput>();
            SetObjectReference(input, "effortSlider", slider);

            var hud = canvasRoot.AddComponent<HudController>();
            SetObjectReference(hud, "speedText", speed);
            SetObjectReference(hud, "effortText", effort);
            SetObjectReference(hud, "progressText", progress);
            SetObjectReference(hud, "distanceText", distance);
            SetObjectReference(hud, "timeText", time);
            SetObjectReference(hud, "sourceText", source);
            SetObjectReference(hud, "phaseText", phase);
            SetObjectReference(hud, "summaryText", summary);

            var controls = canvasRoot.AddComponent<RideControlPanel>();
            SetObjectReference(controls, "startButton", start);
            SetObjectReference(controls, "pauseButton", pause);
            SetObjectReference(controls, "resumeButton", resume);
            SetObjectReference(controls, "finishButton", finish);

            return (input, hud, controls);
        }

        private static Slider CreateSlider(Transform parent)
        {
            var root = new GameObject("MockEffortSlider");
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(16f, 24f);
            rect.sizeDelta = new Vector2(240f, 24f);

            var background = new GameObject("Background");
            background.transform.SetParent(root.transform, false);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.18f, 0.2f, 0.22f, 0.9f);
            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(root.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(2f, 2f);
            fillAreaRect.offsetMax = new Vector2(-2f, -2f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.45f, 1f);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            var handle = new GameObject("Handle");
            handle.transform.SetParent(root.transform, false);
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16f, 28f);

            var slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.35f;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            return slider;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(label == "Resume" ? 88f : 72f, 28f);

            var image = root.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
            var button = root.AddComponent<Button>();
            button.targetGraphic = image;

            var labelText = CreateText(root.transform, "Label", Vector2.zero, rect.sizeDelta.x);
            var labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.text = label;
            return button;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition, float width)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(width, 24f);

            var text = root.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.text = name;
            return text;
        }

        private static void CreateRideSession(
            RideCameraController camera,
            RouteRendererPlaceholder route,
            DepthFogController fog,
            MockRideInput input,
            HudController hud,
            RideControlPanel controls)
        {
            var root = new GameObject("RideSession");
            var controller = root.AddComponent<RideSessionController>();
            SetObjectReference(controller, "cameraController", camera);
            SetObjectReference(controller, "routeRenderer", route);
            SetObjectReference(controller, "fogController", fog);
            SetObjectReference(controller, "mockRideInput", input);
            SetObjectReference(controller, "hudController", hud);
            SetObjectReference(controller, "controlPanel", controls);
        }

        private static Material CreateRouteMaterial()
        {
            return CreateMaterial(RouteMaterialPath, new Color(0.16f, 0.18f, 0.16f, 1f));
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var shader = Shader.Find("Unlit/Color");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            return material;
        }

        private static void SetObjectReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
