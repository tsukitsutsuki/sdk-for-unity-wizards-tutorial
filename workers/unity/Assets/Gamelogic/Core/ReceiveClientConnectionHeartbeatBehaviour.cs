using UnityEngine;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core.Commands;
using Improbable.Worker.CInterop;

namespace Assets.Gamelogic.Core
{
    /// <summary>
    /// Regularly checks to see that the client has sent a heartbeat to indicate it's still connected.
    /// If no heartbeat is received after a certain time (in case of an event like a client crash), the entity is deleted from the world.
    /// </summary>
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class ReceiveClientConnectionHeartbeatBehaviour : MonoBehaviour
    {
        [Require] private ConnectionHeartbeat.Requirable.Writer heartbeat;
        [Require] private ConnectionHeartbeat.Requirable.CommandRequestHandler heartbeatRequestHandler;
        [Require] private WorldCommands.Requirable.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirable.WorldCommandResponseHandler worldCommandResponseHandler;

        private Coroutine heartbeatCoroutine;

        private void OnEnable()
        {
            heartbeatRequestHandler.OnHeartbeatRequest += OnHeartbeat;
            worldCommandResponseHandler.OnDeleteEntityResponse += OnDeleteEntityResponse;
            heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatCheckInterval, CheckHeartbeat));
        }

        private void OnDisable()
        {
            StopCoroutine(heartbeatCoroutine);
        }

        private void OnHeartbeat(ConnectionHeartbeat.Heartbeat.RequestResponder request)
        {
            SetHeartbeat(SimulationSettings.DefaultHeartbeatsBeforeTimeout);
            request.SendResponse(new Nothing());
        }

        private void CheckHeartbeat()
        {
            var heartbeatsRemainingBeforeTimeout = heartbeat.Data.TimeoutBeats;
            if (heartbeatsRemainingBeforeTimeout == 0)
            {
                StopCoroutine(heartbeatCoroutine);
                DeleteInactiveEntity();
                return;
            }
            SetHeartbeat(heartbeatsRemainingBeforeTimeout - 1);
        }

        private void SetHeartbeat(uint beats)
        {
            var update = new ConnectionHeartbeat.Update();
            update.TimeoutBeats = beats;
            heartbeat.Send(update);
        }

        private void DeleteInactiveEntity()
        {
            worldCommandRequestSender.DeleteEntity(gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId);
        }

        private void OnDeleteEntityResponse(WorldCommands.DeleteEntity.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogErrorFormat("Failed to Delete Inactive Entity (#{0}) on quit: {1}", response.RequestPayload.EntityId, response.Message);
            }
        }
    }
}