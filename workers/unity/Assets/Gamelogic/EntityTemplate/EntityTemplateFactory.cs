using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Abilities;
using Improbable.Building;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Gdk.Core;
using Improbable.Global;
using Improbable.Life;
using Improbable.Npc;
using Improbable.Player;
using Improbable.Team;
using Improbable.Tree;
using Improbable.Gdk.PlayerLifecycle;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Gamelogic.EntityTemplate
{
    public static class EntityTemplateFactory
    {
        public static Improbable.Gdk.Core.EntityTemplate CreatePlayerTemplate(string clientWorkerId, Vector3f position)
        {
            var client = $"workerId:{clientWorkerId}";
            var teamId = (uint) Random.Range(0, SimulationSettings.TeamCount);
            var spawningOffset = new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * SimulationSettings.PlayerSpawnOffsetFactor;
            var hqPosition = SimulationSettings.TeamHQLocations[teamId].ToVector3();
            var spawnPosition = (hqPosition + spawningOffset).ToCoordinates();

            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.PlayerPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = spawnPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new ClientAuthorityCheck.Snapshot(), client);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new PlayerInfo.Snapshot { IsAlive = true, InitialSpawnPosition = spawnPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new PlayerControls.Snapshot { TargetPosition = spawnPosition }, client);
            template.AddComponent(new Health.Snapshot { CurrentHealth = SimulationSettings.PlayerMaxHealth, MaxHealth = SimulationSettings.PlayerMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = true, EffectType = FireEffectType.SMALL }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Spells.Snapshot { Cooldowns = new Dictionary<SpellType, float> { { SpellType.LIGHTNING, 0f }, { SpellType.RAIN, 0f } }, CanCastSpells = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Inventory.Snapshot { Resources = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new ConnectionHeartbeat.Snapshot { TimeoutBeats = SimulationSettings.DefaultHeartbeatsBeforeTimeout }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TeamAssignment.Snapshot { TeamId = teamId }, WorkerPlatform.UnityGameLogic);

            PlayerLifecycleHelper.AddPlayerLifecycleComponents(template, clientWorkerId, client, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreateBarracksTemplate(Coordinates initialPosition, BarracksState barracksState, uint teamId)
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.BarracksPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = initialPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = (uint)(UnityEngine.Random.value * 360) }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new BarracksInfo.Snapshot { BarracksState = barracksState }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Health.Snapshot { CurrentHealth = (barracksState == BarracksState.CONSTRUCTION_FINISHED ? SimulationSettings.BarracksMaxHealth : 0), MaxHealth = SimulationSettings.BarracksMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = false, EffectType = FireEffectType.BIG }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new StockpileDepository.Snapshot { CanAcceptResources = (barracksState == BarracksState.UNDER_CONSTRUCTION) }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new NPCSpawner.Snapshot { SpawningEnabled = (barracksState == BarracksState.CONSTRUCTION_FINISHED), Cooldowns = new Dictionary<NPCRole, float> { { NPCRole.LUMBERJACK, SimulationSettings.LumberjackSpawningCooldown }, { NPCRole.WIZARD, SimulationSettings.WizardSpawningCooldown } } }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TeamAssignment.Snapshot { TeamId = teamId }, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreateTreeTemplate(Coordinates initialPosition, uint initialRotation)
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.TreePrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = initialPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = initialRotation }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Harvestable.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Health.Snapshot { CurrentHealth = SimulationSettings.TreeMaxHealth, MaxHealth = SimulationSettings.TreeMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = true, EffectType = FireEffectType.BIG }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TreeState.Snapshot { TreeType = (TreeType)Random.Range(0, 2), CurrentState = TreeFSMState.HEALTHY }, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreateNPCLumberjackTemplate(Coordinates initialPosition, uint teamId)
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.NPCPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = initialPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Health.Snapshot { CurrentHealth = SimulationSettings.LumberjackMaxHealth, MaxHealth = SimulationSettings.LumberjackMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = true, EffectType = FireEffectType.SMALL }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TargetNavigation.Snapshot { NavigationState = NavigationState.INACTIVE, TargetPosition = new Vector3f(0, 0, 0), TargetEntityId = new EntityId(), InteractionSqrDistance = 0f }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Inventory.Snapshot { Resources = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new NPCLumberjack.Snapshot { CurrentState = LumberjackFSMState.StateEnum.IDLE, TargetEntityId = new EntityId(), TargetPosition = SimulationSettings.InvalidPosition.ToVector3f() }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TeamAssignment.Snapshot { TeamId = teamId }, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreateNPCWizardTemplate(Coordinates initialPosition, uint teamId)
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.NPCWizardPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = initialPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Health.Snapshot { CurrentHealth = SimulationSettings.WizardMaxHealth, MaxHealth = SimulationSettings.WizardMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = true, EffectType = FireEffectType.SMALL }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TargetNavigation.Snapshot { NavigationState = NavigationState.INACTIVE, TargetPosition = new Vector3f(0, 0, 0), TargetEntityId = new EntityId(), InteractionSqrDistance = 0f }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Spells.Snapshot { Cooldowns = new Dictionary<SpellType, float> { { SpellType.LIGHTNING, 0f }, { SpellType.RAIN, 0f } }, CanCastSpells = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new NPCWizard.Snapshot { CurrentState = WizardFSMState.StateEnum.IDLE, TargetEntityId = new EntityId(), TargetPosition = SimulationSettings.InvalidPosition.ToVector3f() }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TeamAssignment.Snapshot { TeamId = teamId }, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreateHQTemplate(Coordinates initialPosition, uint initialRotation, uint teamId)
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.HQPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = initialPosition }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new HQInfo.Snapshot { Barracks = new List<EntityId>() }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = initialRotation }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Health.Snapshot { CurrentHealth = SimulationSettings.HQMaxHealth, MaxHealth = SimulationSettings.HQMaxHealth, CanBeChanged = true }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TeamAssignment.Snapshot { TeamId = teamId }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Flammable.Snapshot { IsOnFire = false, CanBeIgnited = true, EffectType = FireEffectType.BIG }, WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }

        public static Improbable.Gdk.Core.EntityTemplate CreatePlayerSpawnerTemplate()
        {
            var metadata = new Metadata.Snapshot { EntityType = SimulationSettings.PlayerSpawnerPrefabName };

            var template = new Improbable.Gdk.Core.EntityTemplate();
            template.AddComponent(new Position.Snapshot { Coords = Coordinates.Zero }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(metadata, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new Persistence.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new TransformComponent.Snapshot { Rotation = 0 }, WorkerPlatform.UnityGameLogic);
            template.AddComponent(new UnityWorkerAuthorityCheck.Snapshot(), WorkerPlatform.UnityGameLogic);
            template.AddComponent(new PlayerSpawning.Snapshot(), WorkerPlatform.UnityGameLogic);

            template.SetReadAccess(WorkerPlatform.AllWorkerAttributes);
            template.SetComponentWriteAccess(EntityAcl.ComponentId, WorkerPlatform.UnityGameLogic);

            return template;
        }
    }
}
