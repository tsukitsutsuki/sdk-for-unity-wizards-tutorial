using Assets.Gamelogic.UI;
using Improbable;
using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.PlayerLifecycle;
using Improbable.Worker.CInterop;
using UnityEngine;

namespace Assets.Gamelogic.Worker
{
    public class ConnectionController : MonoBehaviour
    {
        [Require] private PlayerCreator.Requirable.CommandRequestSender commandSender;
        [Require] private PlayerCreator.Requirable.CommandResponseHandler responseHandler;

        private SplashScreenController screenUIController;
        private WorkerConnector clientWorkerConnector;

        private void Start()
        {
            clientWorkerConnector = gameObject.GetComponent<WorkerConnector>();
            clientWorkerConnector.OnWorkerCreationFinished += OnWorkerCreationFinished;
        }

        private void OnEnable()
        {
            if (responseHandler != null)
            {
                responseHandler.OnCreatePlayerResponse += OnCreatePlayerResponse;
            }
        }

        public void InformOfUI(SplashScreenController screenUIController)
        {
            this.screenUIController = screenUIController;
        }

        private void OnCreatePlayerResponse(PlayerCreator.CreatePlayer.ReceivedResponse obj)
        {
            if (screenUIController == null) return;
            screenUIController.OnCreatePlayerResponse(obj);
        }

        private void OnWorkerCreationFinished(Improbable.Gdk.Core.Worker worker)
        {
            if (worker != null)
            {
                worker.OnDisconnect += OnDisconnect;
            }

            if (screenUIController != null)
            {
                screenUIController.ReadyToConnect();
            }
        }

        private void OnDisconnect(string status)
        {
            if (screenUIController == null) return;
            screenUIController.Disconnected();
        }

        private void SpawnPlayer()
        {
            var request = new CreatePlayerRequestType(new Vector3f { X = 0, Y = 0, Z = 0 });
            commandSender.SendCreatePlayerRequest(new EntityId(1), request);
        }

        public void ConnectAction()
        {
            if (clientWorkerConnector == null || clientWorkerConnector.Worker == null) return;

            if (clientWorkerConnector.Worker.Connection.GetConnectionStatusCode() == ConnectionStatusCode.Success)
            {
                SpawnPlayer();
            }
            else
            {
                ClientWorkerHandler.CreateClient();
            }
        }
    }
}
