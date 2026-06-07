using MyBike.Echappee3D.Core;
using UnityEngine;

namespace MyBike.Echappee3D.Route
{
    public static class RouteMath
    {
        private static readonly RouteDefinition PlaceholderRoute = new RouteDefinition
        {
            id = "placeholder-route",
            lengthMeters = 100f,
            points = new[]
            {
                new RoutePoint(0f, Vector3.zero),
                new RoutePoint(100f, new Vector3(0f, 0f, 100f))
            },
            biomes = new[]
            {
                new BiomeSegment("placeholder", 0f, 1f)
            }
        };

        public static RouteDefinition Resolve(RouteDefinition route)
        {
            return IsValid(route) ? route : PlaceholderRoute;
        }

        public static RouteProgressSnapshot Advance(
            RouteProgressSnapshot previous,
            float speedMps,
            float dtSeconds,
            RouteDefinition route)
        {
            var safeRoute = Resolve(route);
            var previousDistance = Mathf.Clamp(previous.DistanceMeters, 0f, safeRoute.lengthMeters);
            var nextDistance = Mathf.Clamp(
                previousDistance + Mathf.Max(0f, speedMps) * Mathf.Max(0f, dtSeconds),
                0f,
                safeRoute.lengthMeters);
            var progress01 = safeRoute.lengthMeters > 0f ? nextDistance / safeRoute.lengthMeters : 0f;

            return new RouteProgressSnapshot(progress01, nextDistance, progress01 >= 1f);
        }

        public static Vector3 SamplePosition(RouteDefinition route, float progress01)
        {
            var safeRoute = Resolve(route);
            var points = safeRoute.points;
            var distance = Mathf.Clamp01(progress01) * safeRoute.lengthMeters;

            for (var i = 0; i < points.Length - 1; i += 1)
            {
                var start = points[i];
                var end = points[i + 1];

                if (distance > end.DistanceMeters)
                {
                    continue;
                }

                var span = end.DistanceMeters - start.DistanceMeters;
                var t = span > 0f ? (distance - start.DistanceMeters) / span : 0f;
                return Vector3.Lerp(start.Position, end.Position, Mathf.Clamp01(t));
            }

            return points[points.Length - 1].Position;
        }

        public static CameraRigSnapshot CameraOnRail(
            RouteDefinition route,
            RouteProgressSnapshot progress,
            CameraRailConfig config)
        {
            var safeRoute = Resolve(route);
            var routePosition = SamplePosition(safeRoute, progress.Progress01);
            var position = routePosition + Vector3.up * config.HeightMeters;
            var lookAheadProgress = Mathf.Clamp01(
                progress.Progress01 + config.LookAheadMeters / Mathf.Max(1f, safeRoute.lengthMeters));
            var lookAheadPosition = SamplePosition(safeRoute, lookAheadProgress);
            var direction = lookAheadPosition - routePosition;

            if (direction.sqrMagnitude < 0.001f)
            {
                var lookBehindProgress = Mathf.Clamp01(
                    progress.Progress01 - config.LookAheadMeters / Mathf.Max(1f, safeRoute.lengthMeters));
                direction = routePosition - SamplePosition(safeRoute, lookBehindProgress);
            }

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            var lookAt = position +
                direction.normalized * Mathf.Max(1f, config.LookAheadMeters) -
                Vector3.up * config.HeightMeters * 0.2f;

            return new CameraRigSnapshot(position, lookAt, config.FovDegrees);
        }

        public static string SelectBiome(RouteDefinition route, float progress01)
        {
            var safeRoute = Resolve(route);
            var clamped = Mathf.Clamp01(progress01);

            foreach (var biome in safeRoute.biomes)
            {
                if (clamped >= biome.FromProgress01 && clamped <= biome.ToProgress01)
                {
                    return biome.Id;
                }
            }

            return safeRoute.biomes.Length > 0 ? safeRoute.biomes[0].Id : "placeholder";
        }

        public static RouteDefinition CreateDefaultMockRoute()
        {
            return new RouteDefinition
            {
                id = "echappee-mock-route",
                lengthMeters = 1000f,
                points = new[]
                {
                    new RoutePoint(0f, new Vector3(0f, 0f, 0f)),
                    new RoutePoint(150f, new Vector3(24f, 3f, 148f)),
                    new RoutePoint(320f, new Vector3(-18f, 12f, 315f)),
                    new RoutePoint(500f, new Vector3(36f, 7f, 496f)),
                    new RoutePoint(700f, new Vector3(4f, 15f, 698f)),
                    new RoutePoint(850f, new Vector3(-30f, 6f, 845f)),
                    new RoutePoint(1000f, new Vector3(0f, 0f, 1000f))
                },
                biomes = new[]
                {
                    new BiomeSegment("coast", 0f, 0.01f),
                    new BiomeSegment("forest", 0.01f, 1f)
                }
            };
        }

        private static bool IsValid(RouteDefinition route)
        {
            return route != null &&
                route.lengthMeters > 0f &&
                route.points != null &&
                route.points.Length >= 2 &&
                route.biomes != null &&
                route.biomes.Length > 0;
        }
    }
}
