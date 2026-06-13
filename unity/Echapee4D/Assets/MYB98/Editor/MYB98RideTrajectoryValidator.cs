using System;
using System.Collections.Generic;
using System.IO;
using MYB89;
using UnityEditor;
using UnityEngine;

namespace MYB98.Editor
{
    public static class MYB98RideTrajectoryValidator
    {
        [MenuItem("Tools/MYB-98/Validate Ride Trajectory")]
        public static string ValidateRideTrajectoryCli()
        {
            var failures = new List<string>();
            var notes = new List<string>();

            ValidateEmptyAndSinglePointRoutes(failures, notes);
            ValidateStraightRouteSampling(failures, notes);
            ValidateSmoothedRouteShape(failures, notes);
            ValidateWrapAndClampSampling(failures, notes);
            ValidateTransformRouteMarkers(failures, notes);

            var report = WriteReport(failures, notes);
            if (failures.Count > 0)
            {
                throw new InvalidOperationException("MYB-98 ride trajectory validation failed. See " + report);
            }

            Debug.Log("MYB-98 ride trajectory validation passed. Report: " + report);
            return report;
        }

        private static void ValidateEmptyAndSinglePointRoutes(ICollection<string> failures, ICollection<string> notes)
        {
            var empty = MYB89RideTrajectory.BuildSmoothedPoints(Array.Empty<Vector3>());
            if (empty.Length != 0 || MYB89RideTrajectory.Length(empty) != 0f)
            {
                failures.Add("Empty routes must produce no smoothed points and zero length.");
            }

            var singleInput = new[] { new Vector3(3f, 0.5f, 7f) };
            var single = MYB89RideTrajectory.BuildSmoothedPoints(singleInput);
            if (single.Length != 1 || Vector3.Distance(single[0], singleInput[0]) > 0.0001f)
            {
                failures.Add("Single-point routes must preserve their only control point.");
            }

            notes.Add("Empty and single-point route handling validated.");
        }

        private static void ValidateStraightRouteSampling(ICollection<string> failures, ICollection<string> notes)
        {
            var controls = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 10f),
                new Vector3(0f, 0f, 20f)
            };

            var smoothed = MYB89RideTrajectory.BuildSmoothedPoints(controls, 4);
            if (smoothed.Length != 9)
            {
                failures.Add($"Straight route should generate 9 samples, got {smoothed.Length}.");
            }

            if (Vector3.Distance(smoothed[0], controls[0]) > 0.0001f
                || Vector3.Distance(smoothed[smoothed.Length - 1], controls[controls.Length - 1]) > 0.0001f)
            {
                failures.Add("Smoothed routes must preserve the first and last control points.");
            }

            if (!MYB89RideTrajectory.TrySample(smoothed, 5f, false, out var sample))
            {
                failures.Add("Straight route must sample at 5 meters.");
                return;
            }

            if (Vector3.Distance(sample.Position, new Vector3(0f, 0f, 5f)) > 0.01f)
            {
                failures.Add($"Straight route 5 m sample drifted to {sample.Position}.");
            }

            if (Vector3.Angle(sample.Forward, Vector3.forward) > 0.1f)
            {
                failures.Add($"Straight route forward should be world forward, got {sample.Forward}.");
            }

            notes.Add($"Straight route samples: {smoothed.Length}, length {MYB89RideTrajectory.Length(smoothed):0.00} m.");
        }

        private static void ValidateSmoothedRouteShape(ICollection<string> failures, ICollection<string> notes)
        {
            var controls = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(6f, 0.2f, 18f),
                new Vector3(-5f, 0.45f, 38f),
                new Vector3(2f, 0.1f, 58f),
                new Vector3(0f, 0f, 76f)
            };

            var smoothed = MYB89RideTrajectory.BuildSmoothedPoints(controls, MYB89RideTrajectory.DefaultSamplesPerSegment);
            var expectedSamples = (controls.Length - 1) * MYB89RideTrajectory.DefaultSamplesPerSegment + 1;
            if (smoothed.Length != expectedSamples)
            {
                failures.Add($"Curved route sample count should be {expectedSamples}, got {smoothed.Length}.");
            }

            var rawLength = MYB89RideTrajectory.Length(controls);
            var smoothedLength = MYB89RideTrajectory.Length(smoothed);
            if (smoothedLength < rawLength * 0.94f || smoothedLength > rawLength * 1.08f)
            {
                failures.Add($"Smoothed route length should stay near raw length: raw {rawLength:0.00}, smoothed {smoothedLength:0.00}.");
            }

            var maxDeviation = MaxDistanceFromPolyline(smoothed, controls);
            if (maxDeviation > 1.8f)
            {
                failures.Add($"Smoothed route deviates too far from control polyline: {maxDeviation:0.00} m.");
            }

            for (var distance = 0f; distance <= smoothedLength; distance += 5f)
            {
                if (!MYB89RideTrajectory.TrySample(smoothed, distance, false, out var sample))
                {
                    failures.Add($"Curved route failed to sample at {distance:0.0} m.");
                    continue;
                }

                if (sample.Forward.sqrMagnitude < 0.98f || sample.Forward.sqrMagnitude > 1.02f)
                {
                    failures.Add($"Sample forward should be normalized at {distance:0.0} m.");
                }
            }

            notes.Add($"Curved route raw/smoothed length: {rawLength:0.00}/{smoothedLength:0.00} m.");
            notes.Add($"Curved route max polyline deviation: {maxDeviation:0.00} m.");
        }

        private static void ValidateWrapAndClampSampling(ICollection<string> failures, ICollection<string> notes)
        {
            var points = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(0f, 0f, 10f),
                new Vector3(10f, 0f, 10f)
            };
            var length = MYB89RideTrajectory.Length(points);

            MYB89RideTrajectory.TrySample(points, 1f, true, out var expectedWrapped);
            MYB89RideTrajectory.TrySample(points, length + 1f, true, out var wrapped);
            if (Vector3.Distance(expectedWrapped.Position, wrapped.Position) > 0.001f)
            {
                failures.Add("Wrapped sampling should repeat distances past route length.");
            }

            MYB89RideTrajectory.TrySample(points, length + 25f, false, out var clamped);
            if (Vector3.Distance(clamped.Position, points[points.Length - 1]) > 0.001f)
            {
                failures.Add("Clamped sampling should stop at the final route point.");
            }

            notes.Add($"Wrap/clamp sampling validated on {length:0.00} m route.");
        }

        private static void ValidateTransformRouteMarkers(ICollection<string> failures, ICollection<string> notes)
        {
            var root = new GameObject("MYB98_TrajectoryValidatorMarkers");
            try
            {
                var markerA = Marker(root.transform, "A", new Vector3(0f, 0f, 0f));
                var markerB = Marker(root.transform, "B", new Vector3(0f, 0f, 12f));
                var markerC = Marker(root.transform, "C", new Vector3(4f, 0f, 24f));
                var markers = new[] { markerA, null, markerB, markerC };

                var smoothed = MYB89RideTrajectory.BuildSmoothedPoints(markers, 3);
                if (smoothed.Length != 7)
                {
                    failures.Add($"Transform marker routes should ignore null markers and generate 7 samples, got {smoothed.Length}.");
                }

                if (!MYB89RideTrajectory.TrySample(smoothed, 6f, false, out var sample)
                    || sample.Position.z < 5.9f
                    || sample.Position.z > 6.1f)
                {
                    failures.Add("Transform marker route must sample correctly after null marker filtering.");
                }

                notes.Add("Transform marker route filtering validated.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Transform Marker(Transform parent, string name, Vector3 position)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent);
            marker.transform.position = position;
            return marker.transform;
        }

        private static float MaxDistanceFromPolyline(IReadOnlyList<Vector3> points, IReadOnlyList<Vector3> polyline)
        {
            var maxDistance = 0f;
            for (var i = 0; i < points.Count; i++)
            {
                var closest = float.PositiveInfinity;
                for (var segment = 0; segment < polyline.Count - 1; segment++)
                {
                    closest = Mathf.Min(closest, DistanceToSegment(points[i], polyline[segment], polyline[segment + 1]));
                }

                maxDistance = Mathf.Max(maxDistance, closest);
            }

            return maxDistance;
        }

        private static float DistanceToSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            var delta = b - a;
            var denominator = Vector3.Dot(delta, delta);
            if (denominator <= 0.0001f)
            {
                return Vector3.Distance(point, a);
            }

            var t = Mathf.Clamp01(Vector3.Dot(point - a, delta) / denominator);
            return Vector3.Distance(point, a + delta * t);
        }

        private static string WriteReport(IReadOnlyList<string> failures, IReadOnlyList<string> notes)
        {
            var reportDir = Path.Combine(GetRepoRoot(), "_bmad-output", "unity-test-results");
            Directory.CreateDirectory(reportDir);

            var reportPath = Path.Combine(reportDir, "myb-98-ride-trajectory-validator.txt");
            using (var writer = new StreamWriter(reportPath, false))
            {
                writer.WriteLine("MYB-98 Ride Trajectory unit validation");
                writer.WriteLine("Timestamp UTC: " + DateTime.UtcNow.ToString("O"));
                writer.WriteLine("Unity: " + Application.unityVersion);
                writer.WriteLine("Status: " + (failures.Count == 0 ? "PASS" : "FAIL"));
                writer.WriteLine("Default samples per segment: " + MYB89RideTrajectory.DefaultSamplesPerSegment);
                writer.WriteLine("Scope: pure trajectory sampler, no BLE, FTMS, Unity scene dependency, or camera rendering");

                if (notes.Count > 0)
                {
                    writer.WriteLine();
                    writer.WriteLine("Notes:");
                    foreach (var note in notes)
                    {
                        writer.WriteLine("- " + note);
                    }
                }

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

        private static string GetRepoRoot()
        {
            var projectRoot = Directory.GetParent(Application.dataPath);
            var unityFolder = projectRoot?.Parent;
            var repoRoot = unityFolder?.Parent;
            return repoRoot == null ? Directory.GetCurrentDirectory() : repoRoot.FullName;
        }
    }
}
