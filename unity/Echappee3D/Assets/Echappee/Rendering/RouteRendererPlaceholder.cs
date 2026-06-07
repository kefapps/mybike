using MyBike.Echappee3D.Route;
using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public sealed class RouteRendererPlaceholder : MonoBehaviour
    {
        [SerializeField] private float routeWidthMeters = 4f;
        [SerializeField] private Material routeMaterial;

        public void EnsureVisible(RouteDefinition route)
        {
            var safeRoute = RouteMath.Resolve(route);
            var line = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

            line.positionCount = safeRoute.points.Length;
            line.widthMultiplier = routeWidthMeters;
            line.useWorldSpace = true;

            if (routeMaterial != null)
            {
                line.sharedMaterial = routeMaterial;
            }

            for (var i = 0; i < safeRoute.points.Length; i += 1)
            {
                line.SetPosition(i, safeRoute.points[i].Position + Vector3.up * 0.03f);
            }
        }
    }
}
