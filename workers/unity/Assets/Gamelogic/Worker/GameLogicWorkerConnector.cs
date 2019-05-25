using Assets.Gamelogic.Core;
using Improbable.Gdk.Core;
using UnityEngine;

namespace Assets.Gamelogic.Worker
{
    public class GameLogicWorkerConnector : DefaultWorkerConnector
    {
        private const string FlagMaxNpcWizards = "max_npc_wizards";

        private async void Start()
        {
            await Connect(WorkerPlatform.UnityGameLogic, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished()
        {
            WorkerUtils.AddGameLogicSystems(Worker.World);

            Time.fixedDeltaTime = 1.0f / SimulationSettings.FixedFramerate;

            Application.targetFrameRate = SimulationSettings.TargetFramerateUnityWorker;
        }

        public int GetMaxNpcWizards()
        {
            if (int.TryParse(Worker.Connection.GetWorkerFlag(FlagMaxNpcWizards), out var flagMaxNpcWizards))
            {
                return flagMaxNpcWizards;
            }
            return 50;
        }
    }
}
