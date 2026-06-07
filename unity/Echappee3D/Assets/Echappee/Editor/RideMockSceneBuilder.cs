using MyBike.Echappee3D.Bootstrap;
using MyBike.Echappee3D.Rendering;
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
            var ui = CreateUi();
            CreateRideSession(camera.controller, route.renderer, fog, ui.input, ui.hud);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        private static (MockRideInput input, HudController hud) CreateUi()
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
            var speed = CreateText(canvasRoot.transform, "SpeedText", new Vector2(16f, -16f));
            var distance = CreateText(canvasRoot.transform, "DistanceText", new Vector2(16f, -44f));
            var time = CreateText(canvasRoot.transform, "TimeText", new Vector2(16f, -72f));
            var source = CreateText(canvasRoot.transform, "SourceText", new Vector2(16f, -100f));
            var phase = CreateText(canvasRoot.transform, "PhaseText", new Vector2(16f, -128f));

            var input = canvasRoot.AddComponent<MockRideInput>();
            SetObjectReference(input, "effortSlider", slider);

            var hud = canvasRoot.AddComponent<HudController>();
            SetObjectReference(hud, "speedText", speed);
            SetObjectReference(hud, "distanceText", distance);
            SetObjectReference(hud, "timeText", time);
            SetObjectReference(hud, "sourceText", source);
            SetObjectReference(hud, "phaseText", phase);

            return (input, hud);
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

            var fill = new GameObject("Fill");
            fill.transform.SetParent(root.transform, false);
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

        private static Text CreateText(Transform parent, string name, Vector2 anchoredPosition)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(240f, 24f);

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
            HudController hud)
        {
            var root = new GameObject("RideSession");
            var controller = root.AddComponent<RideSessionController>();
            SetObjectReference(controller, "cameraController", camera);
            SetObjectReference(controller, "routeRenderer", route);
            SetObjectReference(controller, "fogController", fog);
            SetObjectReference(controller, "mockRideInput", input);
            SetObjectReference(controller, "hudController", hud);
        }

        private static Material CreateRouteMaterial()
        {
            var shader = Shader.Find("Unlit/Color");
            var material = AssetDatabase.LoadAssetAtPath<Material>(RouteMaterialPath);

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, RouteMaterialPath);
            }

            material.color = new Color(0.16f, 0.18f, 0.16f, 1f);
            return material;
        }

        private static void SetObjectReference(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
