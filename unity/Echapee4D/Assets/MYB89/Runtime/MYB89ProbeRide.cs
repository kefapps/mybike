using System;
using UnityEngine;
using UnityEngine.UI;

namespace MYB89
{
    public sealed class MYB89ProbeRide : MonoBehaviour
    {
        [Header("Route")]
        public Transform[] routeMarkers = Array.Empty<Transform>();
        public float speedMetersPerSecond = 12.5f;
        public float progressMeters;
        public bool autoplay = true;

        [Header("View")]
        public Transform cameraPivot;
        public float cameraBobMeters = 0.045f;
        public float cameraBobFrequency = 1.8f;

        [Header("HUD")]
        public Text distanceLabel;
        public Text speedLabel;
        public Text verdictLabel;

        [Header("Lighting")]
        public Light keyLight;

        private float[] segmentLengths = Array.Empty<float>();
        private float routeLength;
        private Vector3 cameraBaseLocalPosition;

        public float RouteLength => routeLength;

        private void Awake()
        {
            RebuildRouteCache();
            CacheCameraBasePosition();
            ApplyPose(progressMeters);
            UpdateHud();
        }

        private void Start()
        {
            RebuildRouteCache();
            CacheCameraBasePosition();
            ApplyPose(progressMeters);
            UpdateHud();
        }

        private void OnValidate()
        {
            speedMetersPerSecond = Mathf.Max(0.1f, speedMetersPerSecond);
            cameraBobMeters = Mathf.Max(0f, cameraBobMeters);
            cameraBobFrequency = Mathf.Max(0.1f, cameraBobFrequency);
        }

        private void Update()
        {
            if (routeMarkers == null || routeMarkers.Length < 2)
            {
                return;
            }

            if (segmentLengths.Length != routeMarkers.Length - 1)
            {
                RebuildRouteCache();
            }

            if (Application.isPlaying && autoplay)
            {
                progressMeters += speedMetersPerSecond * Time.deltaTime;
            }

            ApplyPose(progressMeters);
            UpdateHud();
            AnimateKeyLight();
        }

        public void SetPreviewProgress(float meters)
        {
            RebuildRouteCache();
            CacheCameraBasePosition();
            progressMeters = meters;
            ApplyPose(progressMeters);
            UpdateHud();
        }

        public void RebuildRouteCache()
        {
            if (routeMarkers == null || routeMarkers.Length < 2)
            {
                segmentLengths = Array.Empty<float>();
                routeLength = 0f;
                return;
            }

            segmentLengths = new float[routeMarkers.Length - 1];
            routeLength = 0f;

            for (var i = 0; i < routeMarkers.Length - 1; i++)
            {
                var a = routeMarkers[i];
                var b = routeMarkers[i + 1];
                var segmentLength = a == null || b == null ? 0f : Vector3.Distance(a.position, b.position);
                segmentLengths[i] = segmentLength;
                routeLength += segmentLength;
            }
        }

        private void CacheCameraBasePosition()
        {
            if (cameraPivot != null)
            {
                cameraBaseLocalPosition = cameraPivot.localPosition;
            }
        }

        private void ApplyPose(float meters)
        {
            if (!TrySampleRoute(meters, out var position, out var forward))
            {
                return;
            }

            transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));

            if (cameraPivot != null)
            {
                var bob = Mathf.Sin((meters / Mathf.Max(1f, speedMetersPerSecond)) * Mathf.PI * 2f * cameraBobFrequency) * cameraBobMeters;
                cameraPivot.localPosition = cameraBaseLocalPosition + new Vector3(0f, bob, 0f);
            }
        }

        private bool TrySampleRoute(float meters, out Vector3 position, out Vector3 forward)
        {
            position = Vector3.zero;
            forward = Vector3.forward;

            if (routeMarkers == null || routeMarkers.Length < 2 || routeLength <= 0.01f)
            {
                return false;
            }

            var wrappedMeters = Mathf.Repeat(meters, routeLength);
            var cursor = 0f;

            for (var i = 0; i < segmentLengths.Length; i++)
            {
                var segmentLength = segmentLengths[i];
                if (segmentLength <= 0.01f)
                {
                    continue;
                }

                if (wrappedMeters <= cursor + segmentLength || i == segmentLengths.Length - 1)
                {
                    var a = routeMarkers[i].position;
                    var b = routeMarkers[i + 1].position;
                    var t = Mathf.InverseLerp(cursor, cursor + segmentLength, wrappedMeters);

                    position = Vector3.Lerp(a, b, Mathf.SmoothStep(0f, 1f, t));
                    forward = (b - a).normalized;
                    return true;
                }

                cursor += segmentLength;
            }

            return false;
        }

        private void UpdateHud()
        {
            var total = routeLength <= 0.01f ? 1f : routeLength;
            var wrappedMeters = Mathf.Repeat(progressMeters, total);
            var percent = Mathf.Clamp01(wrappedMeters / total) * 100f;

            if (distanceLabel != null)
            {
                distanceLabel.text = $"{wrappedMeters:000} m / {routeLength:000} m";
            }

            if (speedLabel != null)
            {
                speedLabel.text = $"{speedMetersPerSecond * 3.6f:00} km/h";
            }

            if (verdictLabel != null)
            {
                verdictLabel.text = $"MYB-89 MCP probe | {percent:00}% | readable motion corridor";
            }
        }

        private void AnimateKeyLight()
        {
            if (keyLight == null)
            {
                return;
            }

            keyLight.intensity = 1.05f + Mathf.Sin(progressMeters * 0.035f) * 0.08f;
        }
    }
}
