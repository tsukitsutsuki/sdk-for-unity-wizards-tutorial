using Improbable.Gdk.Core;
using UnityEngine;

namespace Assets.Gamelogic.Worker
{
    public class ClientWorkerHandler : MonoBehaviour
    {
        private static ClientWorkerHandler Instance;

        [SerializeField] private GameObject clientWorkerPrefab;
        [SerializeField] private UI.SplashScreenController ui;

        private GameObject currentClientWorker;
        private ConnectionController connectionController;
        private WorkerConnector workerConnector;

        public static WorkerConnector ClientWorkerConnector => Instance.workerConnector;
        public static ConnectionController ConnectionController => Instance.connectionController;

        public static void CreateClient()
        {
            Instance.CreateClientWorker();
        }

        private void Start()
        {
            Instance = this;
            CreateClientWorker();
        }

        private void Update()
        {
            // Check if the Client worker has been disconnected, and remove it if so.
            DisconnectCheck();
        }

        private void CreateClientWorker()
        {
            if (currentClientWorker != null)
            {
                Destroy(currentClientWorker);
            }

            currentClientWorker = Instantiate(clientWorkerPrefab);
            workerConnector = currentClientWorker.GetComponent<WorkerConnector>();
            connectionController = currentClientWorker.GetComponent<ConnectionController>();
            connectionController.InformOfUI(ui);
        }

        private void DisconnectCheck()
        {
            if (workerConnector != null
                && workerConnector.Worker != null
                && !workerConnector.Worker.Connection.IsConnected)
            {
                Destroy(currentClientWorker);
            }
        }


        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
