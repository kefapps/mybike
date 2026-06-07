using UnityEngine;

namespace MyBike.Echappee3D.Rendering
{
    public sealed class LightweightSceneLife : MonoBehaviour
    {
        public const string RootName = "SceneLife";
        public const string BirdsRootName = "Birds";
        public const string HumansRootName = "RoadsideHumans";
        public const int RequiredBirdCount = 5;
        public const int RequiredHumanCount = 3;
        public const float MinHumanRouteClearanceMeters = 6f;
        public const float MinBirdHeightMeters = 12f;
        public const int MaxLifeElementCount = 12;

        [SerializeField] private Transform birdsRoot;
        [SerializeField] private Transform humansRoot;

        public Transform BirdsRoot => birdsRoot;
        public Transform HumansRoot => humansRoot;
        public int BirdCount => CountActiveChildren(birdsRoot);
        public int HumanCount => CountActiveChildren(humansRoot);
        public int TotalLifeElementCount => BirdCount + HumanCount;

        public void Bind(Transform birds, Transform humans)
        {
            birdsRoot = birds;
            humansRoot = humans;
        }

        private static int CountActiveChildren(Transform root)
        {
            if (root == null)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < root.childCount; i += 1)
            {
                if (root.GetChild(i).gameObject.activeInHierarchy)
                {
                    count += 1;
                }
            }

            return count;
        }
    }
}
