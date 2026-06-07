using MyBike.Echappee3D.Ride;
using UnityEngine;
using UnityEngine.UI;

namespace MyBike.Echappee3D.UI
{
    public sealed class MockRideInput : MonoBehaviour
    {
        [SerializeField] private Slider effortSlider;

        private MockRideInputSource inputSource;

        public void Bind(MockRideInputSource source)
        {
            inputSource = source;

            if (effortSlider != null)
            {
                effortSlider.minValue = 0f;
                effortSlider.maxValue = 1f;
                effortSlider.value = inputSource.Effort01;
                effortSlider.onValueChanged.AddListener(SetEffort01);
            }
        }

        public void SetEffort01(float effort01)
        {
            inputSource?.SetEffort01(effort01);
        }
    }
}
