using MyBike.Echappee3D.Rendering;
using NUnit.Framework;
using UnityEngine;

namespace MyBike.Echappee3D.Tests
{
    public sealed class LightweightSceneLifeTests
    {
        [Test]
        public void CountsActiveBirdsAndHumans()
        {
            var root = new GameObject("SceneLifeTest");

            try
            {
                var birds = new GameObject(LightweightSceneLife.BirdsRootName).transform;
                var humans = new GameObject(LightweightSceneLife.HumansRootName).transform;
                birds.SetParent(root.transform, false);
                humans.SetParent(root.transform, false);
                new GameObject("Bird_01").transform.SetParent(birds, false);
                new GameObject("Bird_02").transform.SetParent(birds, false);
                new GameObject("HumanSilhouette_01").transform.SetParent(humans, false);

                var life = root.AddComponent<LightweightSceneLife>();
                life.Bind(birds, humans);

                Assert.That(life.BirdCount, Is.EqualTo(2));
                Assert.That(life.HumanCount, Is.EqualTo(1));
                Assert.That(life.TotalLifeElementCount, Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
