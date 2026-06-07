using MyBike.Echappee3D.Core;
using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public sealed class RideCameraController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        public void Apply(CameraRigSnapshot snapshot)
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.transform.position = snapshot.Position;
            targetCamera.transform.LookAt(snapshot.LookAt);
            targetCamera.fieldOfView = snapshot.FovDegrees;
        }
    }
}
