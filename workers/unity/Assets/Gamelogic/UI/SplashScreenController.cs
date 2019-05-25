using Assets.Gamelogic.Worker;
using Improbable.PlayerLifecycle;
using Improbable.Worker.CInterop;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.UI
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private GameObject NotReadyWarning;
        [SerializeField] private Button ConnectButton;
        [SerializeField] private GameObject Spinner;

        private void OnEnable()
        {
            NotReadyWarning.SetActive(true);
            ConnectButton.interactable = false;
        }

        public void OnCreatePlayerResponse(PlayerCreator.CreatePlayer.ReceivedResponse obj)
        {
            if (obj.StatusCode != StatusCode.Success)
            {
                NotReadyWarning.SetActive(true);
                Spinner.SetActive(false);
                ConnectButton.interactable = true;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void ReadyToConnect()
        {
            NotReadyWarning.SetActive(false);
            Spinner.SetActive(false);
            ConnectButton.interactable = true;
        }

        public void Disconnected()
        {
            gameObject.SetActive(true);
            NotReadyWarning.SetActive(true);
            Spinner.SetActive(false);
            ConnectButton.interactable = true;
        }

        public void AttemptToConnect()
        {
            ConnectButton.interactable = false;
            Spinner.SetActive(true);
            NotReadyWarning.SetActive(false);

            AttemptConnection();
        }

        private void AttemptConnection()
        {
            ClientWorkerHandler.ConnectionController.ConnectAction();
        }
    }
}
