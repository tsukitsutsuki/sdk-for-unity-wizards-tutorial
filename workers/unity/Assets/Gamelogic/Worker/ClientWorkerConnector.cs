using Assets.Gamelogic.Core;
using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectCreation;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.PlayerLifecycle;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Gamelogic.Worker
{
    public class ClientWorkerConnector : DefaultWorkerConnector
    {
        private const string AuthPlayer = "Prefabs/UnityClient/Authoritative/Player";
        private const string NonAuthPlayer = "Prefabs/UnityClient/NonAuthoritative/Player";

        private async void Start()
        {
            await Connect(WorkerPlatform.UnityClient, new ForwardingDispatcher()).ConfigureAwait(false);
        }

        protected override void HandleWorkerConnectionEstablished()
        {
            var world = Worker.World;

            // Only take the Heartbeat from the PlayerLifecycleConfig Client Systems.
            world.GetOrCreateManager<HandlePlayerHeartbeatRequestSystem>();

            GameObjectRepresentationHelper.AddSystems(world);
            var fallback = new GameObjectCreatorFromMetadata(Worker.WorkerType, Worker.Origin, Worker.LogDispatcher);

            // Set the Worker gameObject to the ClientWorker so it can access PlayerCreater reader/writers
            GameObjectCreationHelper.EnableStandardGameObjectCreation(
                world,
                new AdvancedEntityPipeline(Worker, AuthPlayer, NonAuthPlayer, fallback),
                gameObject);

            Time.fixedDeltaTime = 1.0f / SimulationSettings.FixedFramerate;

            Application.targetFrameRate = SimulationSettings.TargetFramerate;
        }
    }
}
