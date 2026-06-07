using MyBike.Echappee3D.Route;
using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public sealed class RouteRendererPlaceholder : MonoBehaviour
    {
        [SerializeField] private float routeWidthMeters = 4f;
        [SerializeField] private float routeLiftMeters = 0.08f;
        [SerializeField] private Material routeMaterial;

        public void EnsureVisible(RouteDefinition route)
        {
            var safeRoute = RouteMath.Resolve(route);
            var line = GetComponent<LineRenderer>() ?? gameObject.AddComponent<LineRenderer>();

            line.positionCount = safeRoute.points.Length;
            line.widthMultiplier = Mathf.Max(1f, routeWidthMeters);
            line.useWorldSpace = true;
            line.loop = false;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;

            if (routeMaterial != null)
            {
                line.sharedMaterial = routeMaterial;
            }

            var lift = Vector3.up * Mathf.Max(0.05f, routeLiftMeters);
            for (var i = 0; i < safeRoute.points.Length; i += 1)
            {
                line.SetPosition(i, safeRoute.points[i].Position + lift);
            }
        }
    }
}
