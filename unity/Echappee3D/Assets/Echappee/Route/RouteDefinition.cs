using System;
using UnityEngine;

namespace MyBike.Echappee3D.Route
{
    [Serializable]
    public sealed class RouteDefinition
    {
        public string id = "echappee-mock-route";
        public float lengthMeters = 1000f;
        public RoutePoint[] points = Array.Empty<RoutePoint>();
        public BiomeSegment[] biomes = Array.Empty<BiomeSegment>();
    }

    [Serializable]
    public readonly struct RoutePoint
    {
        public RoutePoint(float distanceMeters, Vector3 position)
        {
            DistanceMeters = distanceMeters;
            Position = position;
        }

        public float DistanceMeters { get; }
        public Vector3 Position { get; }
    }

    [Serializable]
    public readonly struct BiomeSegment
    {
        public BiomeSegment(string id, float fromProgress01, float toProgress01)
        {
            Id = id;
            FromProgress01 = fromProgress01;
            ToProgress01 = toProgress01;
        }

        public string Id { get; }
        public float FromProgress01 { get; }
        public float ToProgress01 { get; }
    }

    public readonly struct CameraRailConfig
    {
        public CameraRailConfig(float heightMeters, float lookAheadMeters, float fovDegrees)
        {
            HeightMeters = Mathf.Clamp(heightMeters, 0.4f, 3f);
            LookAheadMeters = Mathf.Clamp(lookAheadMeters, 1f, 80f);
            FovDegrees = Mathf.Clamp(fovDegrees, 45f, 95f);
        }

        public float HeightMeters { get; }
        public float LookAheadMeters { get; }
        public float FovDegrees { get; }

        public static CameraRailConfig Default => new CameraRailConfig(1.6f, 12f, 70f);
    }
}
