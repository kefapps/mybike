using System;
using System.Collections.Generic;
using UnityEngine;

namespace MYB89
{
    public readonly struct MYB89RideTrajectorySample
    {
        public MYB89RideTrajectorySample(Vector3 position, Vector3 forward, int segmentIndex, float routeLength)
        {
            Position = position;
            Forward = forward;
            SegmentIndex = segmentIndex;
            RouteLength = routeLength;
        }

        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public int SegmentIndex { get; }
        public float RouteLength { get; }
        public Vector3 Right => Vector3.Cross(Vector3.up, Forward).normalized;
    }

    public static class MYB89RideTrajectory
    {
        public const int DefaultSamplesPerSegment = 8;

        public static Vector3[] BuildSmoothedPoints(IReadOnlyList<Vector3> controlPoints, int samplesPerSegment = DefaultSamplesPerSegment)
        {
            if (controlPoints == null || controlPoints.Count == 0)
            {
                return Array.Empty<Vector3>();
            }

            if (controlPoints.Count == 1)
            {
                return new[] { controlPoints[0] };
            }

            var clampedSamples = Mathf.Clamp(samplesPerSegment, 2, 24);
            var points = new List<Vector3>((controlPoints.Count - 1) * clampedSamples + 1);
            for (var i = 0; i < controlPoints.Count - 1; i++)
            {
                var p0 = i == 0 ? controlPoints[i] : controlPoints[i - 1];
                var p1 = controlPoints[i];
                var p2 = controlPoints[i + 1];
                var p3 = i + 2 >= controlPoints.Count ? controlPoints[i + 1] : controlPoints[i + 2];
                var tangentA = LimitedTangent(p0, p1, p2);
                var tangentB = LimitedTangent(p1, p2, p3);

                for (var step = 0; step < clampedSamples; step++)
                {
                    var t = step / (float)clampedSamples;
                    points.Add(Hermite(p1, p2, tangentA, tangentB, t));
                }
            }

            points.Add(controlPoints[controlPoints.Count - 1]);
            return points.ToArray();
        }

        public static Vector3[] BuildSmoothedPoints(Transform[] routeMarkers, int samplesPerSegment = DefaultSamplesPerSegment)
        {
            if (routeMarkers == null || routeMarkers.Length == 0)
            {
                return Array.Empty<Vector3>();
            }

            var points = new List<Vector3>(routeMarkers.Length);
            for (var i = 0; i < routeMarkers.Length; i++)
            {
                if (routeMarkers[i] != null)
                {
                    points.Add(routeMarkers[i].position);
                }
            }

            return BuildSmoothedPoints(points, samplesPerSegment);
        }

        public static float Length(IReadOnlyList<Vector3> points)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            var length = 0f;
            for (var i = 0; i < points.Count - 1; i++)
            {
                length += Vector3.Distance(points[i], points[i + 1]);
            }

            return length;
        }

        public static bool TrySample(
            IReadOnlyList<Vector3> points,
            float meters,
            bool wrap,
            out MYB89RideTrajectorySample sample)
        {
            sample = new MYB89RideTrajectorySample(Vector3.zero, Vector3.forward, -1, 0f);
            var routeLength = Length(points);
            if (points == null || points.Count < 2 || routeLength <= 0.01f)
            {
                return false;
            }

            var targetMeters = wrap ? Mathf.Repeat(meters, routeLength) : Mathf.Clamp(meters, 0f, routeLength);
            var cursor = 0f;
            for (var i = 0; i < points.Count - 1; i++)
            {
                var a = points[i];
                var b = points[i + 1];
                var segmentLength = Vector3.Distance(a, b);
                if (segmentLength <= 0.01f)
                {
                    continue;
                }

                if (targetMeters <= cursor + segmentLength || i == points.Count - 2)
                {
                    var t = Mathf.InverseLerp(cursor, cursor + segmentLength, targetMeters);
                    var position = Vector3.Lerp(a, b, t);
                    var forward = (b - a).normalized;
                    sample = new MYB89RideTrajectorySample(position, forward, i, routeLength);
                    return true;
                }

                cursor += segmentLength;
            }

            return false;
        }

        private static Vector3 LimitedTangent(Vector3 previous, Vector3 current, Vector3 next)
        {
            var tangent = (next - previous) * 0.5f;
            var previousLength = Vector3.Distance(previous, current);
            var nextLength = Vector3.Distance(current, next);
            var shortestNeighbor = Mathf.Max(0.01f, Mathf.Min(
                previousLength <= 0.01f ? nextLength : previousLength,
                nextLength <= 0.01f ? previousLength : nextLength));
            var maxLength = shortestNeighbor * 0.65f;
            return tangent.magnitude > maxLength ? tangent.normalized * maxLength : tangent;
        }

        private static Vector3 Hermite(Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * start
                + (t3 - 2f * t2 + t) * startTangent
                + (-2f * t3 + 3f * t2) * end
                + (t3 - t2) * endTangent;
        }
    }
}
