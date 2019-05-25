using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Assets.Gamelogic.NPC.LumberJack;
using Improbable.Building;
using Improbable.Core;
using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Npc;
using Improbable.Team;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.NPC.Lumberjack
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class LumberjackBehaviour : MonoBehaviour, IFlammable
    {
        [Require] private NPCLumberjack.Requirable.Writer npcLumberjack;
        [Require] private Harvestable.Requirable.CommandRequestSender harvestableRequestSender;
        [Require] private Harvestable.Requirable.CommandResponseHandler harvestableResponseHandler;
        [Require] private StockpileDepository.Requirable.CommandRequestSender stockpileDepositoryRequestSender;
        [Require] private StockpileDepository.Requirable.CommandResponseHandler stockpileDepositoryResponseHandler;
        [Require] private TargetNavigation.Requirable.Writer targetNavigation;
        [Require] private Inventory.Requirable.Writer inventory;
        [Require] private TeamAssignment.Requirable.Reader teamAssignment;

        [SerializeField] private TargetNavigationBehaviour navigation;

        private LumberjackStateMachine stateMachine;

        private void Awake()
        {
            navigation = gameObject.GetComponentIfUnassigned(navigation);
        }

        private void OnEnable()
        {
            stateMachine = new LumberjackStateMachine(this, navigation, inventory, targetNavigation, npcLumberjack, harvestableRequestSender, harvestableResponseHandler, stockpileDepositoryRequestSender, stockpileDepositoryResponseHandler, teamAssignment);
            stateMachine.OnEnable(npcLumberjack.Data.CurrentState);
        }

        private void OnDisable()
        {
            stateMachine.OnDisable();
        }

        public void OnIgnite()
        {
			stateMachine.TriggerTransition(LumberjackFSMState.StateEnum.ON_FIRE, new EntityId(), SimulationSettings.InvalidPosition);
        }

        public void OnExtinguish()
        {
			stateMachine.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
        }
    }
}
