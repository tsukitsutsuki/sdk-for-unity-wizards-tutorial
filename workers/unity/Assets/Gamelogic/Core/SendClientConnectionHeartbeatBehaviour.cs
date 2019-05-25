using UnityEngine;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using Improbable.Gdk.Core.Commands;

namespace Assets.Gamelogic.Core
{
    /// <summary>
    /// Regularly notifies the authoritative worker that this client is still alive and connected.
    /// When this times out (in case of an event like a client crash), the authoritative will delete the entity from the world.
    /// </summary>
    [WorkerType(WorkerPlatform.UnityClient)]
    public class SendClientConnectionHeartbeatBehaviour : MonoBehaviour
    {
        [Require] private ClientAuthorityCheck.Requirable.Writer clientAuthorityCheck;
        [Require] private ConnectionHeartbeat.Requirable.CommandRequestSender connectionHeartbeatRequestSender;
        [Require] private ConnectionHeartbeat.Requirable.CommandResponseHandler connectionHeartbeatResponseHandler;
        [Require] private WorldCommands.Requirable.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirable.WorldCommandResponseHandler worldCommandResponseHandler;

        private Coroutine heartbeatCoroutine;

        private void OnEnable()
        {
            connectionHeartbeatResponseHandler.OnHeartbeatResponse += OnHeartbeatResponse;
            worldCommandResponseHandler.OnDeleteEntityResponse += OnDeleteEntityResponse;
            heartbeatCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.HeartbeatCheckInterval, SendHeartbeat));
        }

        private void OnDisable()
        {
            StopCoroutine(heartbeatCoroutine);
        }

        private void SendHeartbeat()
        {
            connectionHeartbeatRequestSender.SendHeartbeatRequest(gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId, new Nothing());
        }

        private void OnHeartbeatResponse(ConnectionHeartbeat.Heartbeat.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogError("Player connection heartbeat failed to send. Player may timeout.");
            }
        }

        private void OnApplicationQuit()
        {
            worldCommandRequestSender.DeleteEntity(gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId);
        }

        private void OnDeleteEntityResponse(WorldCommands.DeleteEntity.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogErrorFormat("Failed to Delete Player Entity (#{0}) on quit: {1}", response.RequestPayload.EntityId, response.Message);
            }
        }
    }
}