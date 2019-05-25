using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Building;
using Improbable.Npc;
using Improbable.Team;
using System.Collections.Generic;
using Assets.Gamelogic.Team;
using Improbable;
using UnityEngine;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core;
using Assets.Gamelogic.Worker;
using Improbable.Gdk.Core.Commands;
using Assets.Gamelogic.EntityTemplate;
using Improbable.Worker.CInterop;

namespace Assets.Gamelogic.Building
{
    public class NPCSpawnerBehaviour : MonoBehaviour
    {
        [Require] private NPCSpawner.Requirable.Writer npcSpawner;
        [Require] private TeamAssignment.Requirable.Reader teamAssignment;
        [Require] private WorldCommands.Requirable.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirable.WorldCommandResponseHandler worldCommandResponseHandler;

        private Coroutine spawnNPCsReduceCooldownCoroutine;
        private GameLogicWorkerConnector coordinator;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private Stack<Improbable.Gdk.Core.EntityTemplate> ReadyToSpawn = new Stack<Improbable.Gdk.Core.EntityTemplate>();
        private ILogDispatcher logDispatcher;

        private static readonly IDictionary<NPCRole, float> npcRolesToCooldownDictionary = new Dictionary<NPCRole, float>
        {
            { NPCRole.LUMBERJACK, SimulationSettings.LumberjackSpawningCooldown },
            { NPCRole.WIZARD, SimulationSettings.WizardSpawningCooldown }
        };

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            coordinator = FindObjectOfType<GameLogicWorkerConnector>();

            var npcRoles = new System.Collections.Generic.List<NPCRole>(npcRolesToCooldownDictionary.Keys);

            spawnNPCsReduceCooldownCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.SimulationTickInterval, () =>
            {
                ReduceSpawnCooldown(npcRoles, SimulationSettings.SimulationTickInterval);
            }));

            logDispatcher = GetComponent<SpatialOSComponent>().Worker.LogDispatcher;
            worldCommandResponseHandler.OnReserveEntityIdsResponse += OnEntityIdsReserved;
            worldCommandResponseHandler.OnCreateEntityResponse += OnEntityCreated;
        }

        private void OnDisable()
        {
            CancelSpawnNPCsReduceCooldownCoroutine();
        }

        private void CancelSpawnNPCsReduceCooldownCoroutine()
        {
            if (spawnNPCsReduceCooldownCoroutine != null)
            {
                StopCoroutine(spawnNPCsReduceCooldownCoroutine);
                spawnNPCsReduceCooldownCoroutine = null;
            }
        }

        private void ReduceSpawnCooldown(IList<NPCRole> npcRoles, float interval)
        {
            if (!npcSpawner.Data.SpawningEnabled)
            {
                return;
            }

            var newCooldowns = new Dictionary<NPCRole, float>(npcSpawner.Data.Cooldowns);

            for (var i = 0; i < npcRoles.Count; i++)
            {
                var role = npcRoles[i];
                if (newCooldowns[role] <= 0f) // todo: this is a workaround for WIT-1374
                {
                    var spawningOffset = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * SimulationSettings.SpawnOffsetFactor;
                    var spawnPosition = (gameObject.transform.position - origin + spawningOffset).ToCoordinates();
                    SpawnNpc(role, spawnPosition);
                    newCooldowns[role] = npcRolesToCooldownDictionary[role];
                }
                else
                {
                    newCooldowns[role] = Mathf.Max(newCooldowns[role] - interval, 0f);
                }
            }
            npcSpawner.Send(new NPCSpawner.Update() { Cooldowns = newCooldowns });
        }

        public void SetSpawningEnabled(bool spawningEnabled)
        {
            if (spawningEnabled != npcSpawner.Data.SpawningEnabled)
            {
                npcSpawner.Send(new NPCSpawner.Update() { SpawningEnabled = new Option<BlittableBool>(spawningEnabled) });
            }
        }

        private void SpawnNpc(NPCRole npcRoleEnum, Coordinates position)
        {
            switch (npcRoleEnum)
            {
                case NPCRole.LUMBERJACK:
                    SpawnLumberjack(position);
                    break;
                case NPCRole.WIZARD:
                    SpawnWizard(position);
                    break;
            }
        }

        private int GetLumberjackCount()
        {
            var lumberjacks = GameObject.FindGameObjectsWithTag("NPCLumberjack");
            var count = 0;
            for (var i = 0; i < lumberjacks.Length; ++i)
            {
                var teamAssignmentVisualizer = lumberjacks[i].GetComponent<TeamAssignmentVisualizerUnityWorker>();
                if (teamAssignmentVisualizer != null && teamAssignmentVisualizer.TeamId == teamAssignment.Data.TeamId)
                {
                    ++count;
                }
            }
            return count;
        }

        private void SpawnLumberjack(Coordinates position)
        {
            var lumberjackCount = GetLumberjackCount();
            if (lumberjackCount >= 20)
            {
                return;
            }
            var template = EntityTemplateFactory.CreateNPCLumberjackTemplate(position, teamAssignment.Data.TeamId);
            CreateEntity(template);
        }

        private int GetWizardCount()
        {
            var wizards = GameObject.FindGameObjectsWithTag("NPCWizard");
            var count = 0;
            for (var i = 0; i < wizards.Length; ++i)
            {
                var teamAssignmentVisualizer = wizards[i].GetComponent<TeamAssignmentVisualizerUnityWorker>();
                if (teamAssignmentVisualizer != null && teamAssignmentVisualizer.TeamId == teamAssignment.Data.TeamId)
                {
                    ++count;
                }
            }
            return count;
        }

        private void SpawnWizard(Coordinates position)
        {
            var wizardCount = GetWizardCount();
            int maxNpcWizards = 0;

            if (coordinator != null)
            {
                maxNpcWizards = coordinator.GetMaxNpcWizards();
            }
            if (wizardCount >= maxNpcWizards)
            {
                return;
            }
            var template = EntityTemplateFactory.CreateNPCWizardTemplate(position, teamAssignment.Data.TeamId);
            CreateEntity(template);
        }

        private void CreateEntity(Improbable.Gdk.Core.EntityTemplate entityTemplate)
        {
            ReadyToSpawn.Push(entityTemplate);
            worldCommandRequestSender.ReserveEntityIds(1, context: this);
        }

        private void OnEntityIdsReserved(WorldCommands.ReserveEntityIds.ReceivedResponse response)
        {
            if (!ReferenceEquals(this, response.Context))
            {
                // This response was not for a command from this behaviour.
                return;
            }

            if (response.StatusCode != StatusCode.Success)
            {
                logDispatcher.HandleLog(
                    LogType.Error,
                    new LogEvent("ReserveEntityIds failed.")
                        .WithField("Reason", response.Message)
                );
                return;
            }

            var entityTemplate = ReadyToSpawn.Pop();
            var expectedEntityId = response.FirstEntityId.Value;
            worldCommandRequestSender.CreateEntity(entityTemplate, expectedEntityId, context: this);
        }

        private void OnEntityCreated(WorldCommands.CreateEntity.ReceivedResponse response)
        {
            if (!ReferenceEquals(this, response.Context))
            {
                // This response was not for a command from this behaviour.
                return;
            }

            if (response.StatusCode != StatusCode.Success)
            {
                logDispatcher.HandleLog(
                    LogType.Error,
                    new LogEvent("CreateEntity failed.")
                        .WithField(LoggingUtils.EntityId, response.RequestPayload.EntityId)
                        .WithField("Reason", response.Message)
                );

                return;
            }
        }

        public void ResetCooldowns()
        {
			npcSpawner.Send(new NPCSpawner.Update() { Cooldowns = new Dictionary<NPCRole, float> { { NPCRole.LUMBERJACK, SimulationSettings.LumberjackSpawningCooldown }, { NPCRole.WIZARD, SimulationSettings.WizardSpawningCooldown } } });
        }
    }
}
