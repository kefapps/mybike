using MyBike.Echappee3D.Bootstrap;
using MyBike.Echappee3D.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MyBike.Echappee3D.UI
{
    public sealed class RideControlPanel : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button finishButton;

        public void Bind(RideSessionController controller)
        {
            BindButton(startButton, controller.StartRide);
            BindButton(pauseButton, controller.PauseRide);
            BindButton(resumeButton, controller.ResumeRide);
            BindButton(finishButton, controller.FinishRide);
            Render(controller.Phase);
        }

        public void Render(RidePhase phase)
        {
            if (startButton != null)
            {
                startButton.interactable = phase == RidePhase.Idle || phase == RidePhase.Finished || phase == RidePhase.Error;
            }

            if (pauseButton != null)
            {
                pauseButton.interactable = phase == RidePhase.Running;
            }

            if (resumeButton != null)
            {
                resumeButton.interactable = phase == RidePhase.Paused;
            }

            if (finishButton != null)
            {
                finishButton.interactable = phase == RidePhase.Running || phase == RidePhase.Paused;
            }
        }

        private static void BindButton(Button button, UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }
    }
}
