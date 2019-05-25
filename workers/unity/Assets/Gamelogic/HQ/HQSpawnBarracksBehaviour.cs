using Assets.Gamelogic.Core;
using Assets.Gamelogic.EntityTemplate;
using Assets.Gamelogic.Life;
using Improbable.Building;
using Improbable.Team;
using System.Collections.Generic;
using Assets.Gamelogic.Utils;
using UnityEngine;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core.Commands;
using Improbable.Gdk.Core;
using Improbable.Worker.CInterop;

namespace Assets.Gamelogic.HQ
{
    public class HQSpawnBarracksBehaviour : MonoBehaviour
    {
        [Require] private HQInfo.Requirable.Writer hqInfo;
        [Require] private TeamAssignment.Requirable.Reader teamAssignment;
        [Require] private WorldCommands.Requirable.WorldCommandRequestSender worldCommandRequestSender;
        [Require] private WorldCommands.Requirable.WorldCommandResponseHandler worldCommandResponseHandler;

        private Coroutine spawnBarracksPeriodicallyCoroutine;
        private readonly HashSet<GameObject> barracksSet = new HashSet<GameObject>();
        private float barracksSpawnRadius;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private Stack<Improbable.Gdk.Core.EntityTemplate> ReadyToSpawn = new Stack<Improbable.Gdk.Core.EntityTemplate>();
        private ILogDispatcher logDispatcher;

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            logDispatcher = GetComponent<SpatialOSComponent>().Worker.LogDispatcher;
            hqInfo.ComponentUpdated += OnComponentUpdated;
            worldCommandResponseHandler.OnReserveEntityIdsResponse += OnEntityIdsReserved;
            worldCommandResponseHandler.OnCreateEntityResponse += OnEntityCreated;

            barracksSpawnRadius = SimulationSettings.DefaultHQBarracksSpawnRadius;
            spawnBarracksPeriodicallyCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(SimulationSettings.SimulationTickInterval * 5f, SpawnBarracks));
            PopulateBarracksDictionary();
        }

        private void OnDisable()
        {
            CancelSpawnBarracksPeriodicallyCoroutine();
        }

        private void RegisterBarracks(EntityId barrackId)
        {
            var newBarracks = new List<EntityId>(hqInfo.Data.Barracks);
            newBarracks.Add(barrackId);
            hqInfo.Send(new HQInfo.Update() { Barracks = newBarracks });
        }

        private void PopulateBarracksDictionary()
        {
            var spatialOSComponent = gameObject.GetComponent<SpatialOSComponent>();
            if (spatialOSComponent == null) return;

            for (var i = 0; i < hqInfo.Data.Barracks.Count; i++)
            {
                if (!spatialOSComponent.IsEntityOnThisWorker(hqInfo.Data.Barracks[i])) continue;

                GameObject barracksGameObject;
                if (spatialOSComponent.TryGetGameObjectForSpatialOSEntityId(hqInfo.Data.Barracks[i], out barracksGameObject)
                    && barracksGameObject != null)
                {
                    if (!barracksSet.Contains(barracksGameObject))
                    {
                        barracksSet.Add(barracksGameObject);
                    }
                }
            }
        }

        private void CancelSpawnBarracksPeriodicallyCoroutine()
        {
            if (spawnBarracksPeriodicallyCoroutine != null)
            {
                StopCoroutine(spawnBarracksPeriodicallyCoroutine);
                spawnBarracksPeriodicallyCoroutine = null;
            }
        }

        private void OnComponentUpdated(HQInfo.Update update)
        {
            if (update.Barracks.HasValue)
            {
                PopulateBarracksDictionary();
            }
        }

        private void SpawnBarracks()
        {
            if (AllBarracksAtFullHealth())
            {
                SpawnUnconstructedBarracksAtRandomLocation();
            }
        }

        private bool AllBarracksAtFullHealth()
        {
            var barracksEnumerator = Physics.OverlapSphere(transform.position, barracksSpawnRadius);

            var allBarracksFullHealth = true;
            for(var i = 0; i < barracksEnumerator.Length; i++)
            { 
                if(barracksEnumerator[i].gameObject.name.Contains("Barracks"))
                {
                    var health = barracksEnumerator[i].gameObject.GetComponent<HealthVisualizer>();
                    if (health.CurrentHealth < health.MaxHealth)
                    {
                        allBarracksFullHealth = false;
                    }
                }
            }
            return allBarracksFullHealth;
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

                worldCommandRequestSender.ReserveEntityIds(1, OnEntityIdsReserved);
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

            if (response.EntityId.HasValue)
            {
                RegisterBarracks(response.EntityId.Value);
                PopulateBarracksDictionary();
            }
        }

        private void SpawnUnconstructedBarracksAtRandomLocation()
        {
            var spawnPosition = FindSpawnLocation();
            if (SpawnLocationInvalid(spawnPosition))
            {
                Debug.LogError("HQ failed to find place to spawn barracks.");
                return;
            }

            var teamId = teamAssignment.Data.TeamId;
            var template = EntityTemplateFactory.CreateBarracksTemplate(spawnPosition.ToCoordinates(), BarracksState.UNDER_CONSTRUCTION, teamId);
            ReadyToSpawn.Push(template);

            worldCommandRequestSender.ReserveEntityIds(1, context: this);
        }

        private bool SpawnLocationInvalid(Vector3 position)
        {
            return position.y < 0f;
        }

        private Vector3 FindSpawnLocation()
        {
            while (true)
            {
                for (var attemptNum = 0; attemptNum < SimulationSettings.HQBarracksSpawnPositionPickingRetries; attemptNum++)
                {
                    var spawnLocation = PickRandomLocationNearby();
                    if (NotCollidingWithAnything(spawnLocation))
                    {
                        return spawnLocation - origin;
                    }
                }
                if (barracksSpawnRadius > SimulationSettings.MaxHQBarracksSpawnRadius)
                {
                    return Vector3.down;
                }
                barracksSpawnRadius += SimulationSettings.HQBarracksSpawnRadiusIncrease;
            }
        }

        private Vector3 PickRandomLocationNearby()
        {
            var randomOffset = new Vector3(Random.Range(-barracksSpawnRadius, barracksSpawnRadius), 0f, Random.Range(-barracksSpawnRadius, barracksSpawnRadius));
            return transform.position + randomOffset;
        }

        private bool NotCollidingWithAnything(Vector3 spawnLocation)
        {
            return NotCollidingWithHQ(spawnLocation) && NotCollidingWithOtherBarracks(spawnLocation);
        }

        private bool NotCollidingWithHQ(Vector3 spawnLocation)
        {
            return Vector3.Distance(transform.position, spawnLocation) > SimulationSettings.HQBarracksSpawningSeparation;
        }

        private bool NotCollidingWithOtherBarracks(Vector3 spawnLocation)
        {
            foreach (GameObject barracks in barracksSet)
            {
                if (Vector3.Distance(barracks.transform.position, spawnLocation) <= SimulationSettings.HQBarracksSpawningSeparation)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
