using Assets.Gamelogic.Core;
using Improbable.Core;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using Improbable.Worker.CInterop;
using UnityEngine;

namespace Assets.Gamelogic.NPC
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class NPCDeathBehaviour : MonoBehaviour
    {
        [Require] private UnityWorkerAuthorityCheck.Requirable.Writer unityWorkerAuthorityCheck;
        [Require] private Health.Requirable.Reader health;
        [Require] private WorldCommands.Requirable.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirable.WorldCommandResponseHandler worldCommandResponseHandler;

        private bool npcDeathActive;

        private void OnEnable()
        {
            npcDeathActive = SimulationSettings.NPCDeathActive;
            health.ComponentUpdated += OnHealthUpdated;
            worldCommandResponseHandler.OnDeleteEntityResponse += OnDeleteEntityResponse;
        }

        private void OnDisable()
        {
        }

        private void OnHealthUpdated(Health.Update update)
        {
            if (update.CurrentHealth.HasValue)
            {
                DieUponHealthDepletion(update);
            }
        }

        private void OnDeleteEntityResponse(WorldCommands.DeleteEntity.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogErrorFormat("Failed to Delete Player Entity (#{0}) on quit: {1}", response.RequestPayload.EntityId, response.Message);
            }
        }

        private void DieUponHealthDepletion(Health.Update update)
        {
            if (npcDeathActive && update.CurrentHealth.Value <= 0)
            {
                worldCommandRequestSender.DeleteEntity(gameObject.GetComponent<SpatialOSComponent>().SpatialEntityId);
            }
        }
    }
}