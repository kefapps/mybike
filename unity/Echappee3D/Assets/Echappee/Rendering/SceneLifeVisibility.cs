using MyBike.Echappee3D.Route;
using MyBike.Echappee3D.Core;
using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public readonly struct SceneLifeVisibilitySample
    {
        public SceneLifeVisibilitySample(
            string elementType,
            string elementName,
            Vector3 worldPosition,
            float progress01,
            float distanceMeters,
            float yawDegrees,
            float pitchDegrees,
            bool isInCameraCone,
            bool isFinite)
        {
            ElementType = elementType;
            ElementName = elementName;
            WorldPosition = worldPosition;
            Progress01 = progress01;
            DistanceMeters = distanceMeters;
            YawDegrees = yawDegrees;
            PitchDegrees = pitchDegrees;
            IsInCameraCone = isInCameraCone;
            IsFinite = isFinite;
        }

        public string ElementType { get; }
        public string ElementName { get; }
        public Vector3 WorldPosition { get; }
        public float Progress01 { get; }
        public float DistanceMeters { get; }
        public float YawDegrees { get; }
        public float PitchDegrees { get; }
        public bool IsInCameraCone { get; }
        public bool IsFinite { get; }
    }

    public static class SceneLifeVisibility
    {
        private const float Epsilon = 0.0001f;

        public static float HorizontalFovFromVertical(float verticalFovDegrees, float aspect)
        {
            var safeVerticalFov = Mathf.Clamp(verticalFovDegrees, 1f, 179f);
            var safeAspect = Mathf.Max(Epsilon, aspect);
            var halfVerticalRadians = safeVerticalFov * Mathf.Deg2Rad * 0.5f;
            return Mathf.Atan(Mathf.Tan(halfVerticalRadians) * safeAspect) * Mathf.Rad2Deg * 2f;
        }

        public static SceneLifeVisibilitySample Measure(
            string elementType,
            string elementName,
            Vector3 worldPosition,
            RouteDefinition route,
            CameraRailConfig cameraConfig,
            float horizontalFovDegrees,
            float verticalFovDegrees)
        {
            var resolvedRoute = RouteMath.Resolve(route);
            var progress01 = resolvedRoute.lengthMeters <= Epsilon
                ? 0f
                : Mathf.Clamp01(worldPosition.z / resolvedRoute.lengthMeters);
            var progress = new RouteProgressSnapshot(
                progress01,
                progress01 * resolvedRoute.lengthMeters,
                progress01 >= 1f);
            var camera = RouteMath.CameraOnRail(resolvedRoute, progress, cameraConfig);
            var cameraForward = camera.LookAt - camera.Position;

            if (cameraForward.sqrMagnitude <= Epsilon)
            {
                cameraForward = Vector3.forward;
            }

            var toElement = worldPosition - camera.Position;
            var distanceMeters = toElement.magnitude;

            if (distanceMeters <= Epsilon)
            {
                return new SceneLifeVisibilitySample(
                    elementType,
                    elementName,
                    worldPosition,
                    progress01,
                    0f,
                    0f,
                    0f,
                    false,
                    IsFinite(worldPosition));
            }

            var cameraRotation = Quaternion.LookRotation(cameraForward.normalized, Vector3.up);
            var localDirection = Quaternion.Inverse(cameraRotation) * (toElement / distanceMeters);
            var yawDegrees = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
            var pitchDegrees = Mathf.Asin(Mathf.Clamp(localDirection.y, -1f, 1f)) * Mathf.Rad2Deg;
            var isFinite = IsFinite(worldPosition) &&
                IsFinite(distanceMeters) &&
                IsFinite(yawDegrees) &&
                IsFinite(pitchDegrees);
            var isInCameraCone = isFinite &&
                localDirection.z > 0f &&
                Mathf.Abs(yawDegrees) <= horizontalFovDegrees * 0.5f &&
                Mathf.Abs(pitchDegrees) <= verticalFovDegrees * 0.5f;

            return new SceneLifeVisibilitySample(
                elementType,
                elementName,
                worldPosition,
                progress01,
                distanceMeters,
                yawDegrees,
                pitchDegrees,
                isInCameraCone,
                isFinite);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
