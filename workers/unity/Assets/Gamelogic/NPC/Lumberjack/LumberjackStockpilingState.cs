using Assets.Gamelogic.ComponentExtensions;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.NPC.Lumberjack;
using Assets.Gamelogic.Utils;
using Improbable.Building;
using Improbable.Core;
using Improbable.Gdk.Core;
using Improbable.Npc;
using Improbable.Worker.CInterop;
using UnityEngine;

namespace Assets.Gamelogic.NPC.LumberJack
{
    public class LumberjackStockpilingState : FsmBaseState<LumberjackStateMachine, LumberjackFSMState.StateEnum>
    {
        private readonly LumberjackBehaviour parentBehaviour;
        private readonly Inventory.Requirable.Writer inventory;
        private readonly StockpileDepository.Requirable.CommandRequestSender stockpileDepositoryRequestSender;
        private readonly StockpileDepository.Requirable.CommandResponseHandler stockpileDepositoryResponseHandler;

        private Coroutine addToStockpileDelayCoroutine;
        private Coroutine transitionToIdleDelayCoroutine;

        public LumberjackStockpilingState(LumberjackStateMachine owner,
                                          LumberjackBehaviour inParentBehaviour,
                                          Inventory.Requirable.Writer inInventory,
                                          StockpileDepository.Requirable.CommandRequestSender inStockpileDepositoryRequestSender,
                                          StockpileDepository.Requirable.CommandResponseHandler inStockpileDepositoryResponseHandler)
            : base(owner)
        {
            parentBehaviour = inParentBehaviour;
            inventory = inInventory;
            stockpileDepositoryRequestSender = inStockpileDepositoryRequestSender;
            stockpileDepositoryResponseHandler = inStockpileDepositoryResponseHandler;
            stockpileDepositoryResponseHandler.OnAddResourceResponse += OnStockpileResponse;
        }

        public override void Enter()
        {
            if (inventory.HasResources())
            {
                addToStockpileDelayCoroutine = parentBehaviour.StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.NPCStockpilingAnimationStartDelay, AddToStockpile));
            }
            else
            {
                TransitionToIdle();
            }
        }

        public override void Tick()
        {   
        }

        public override void Exit(bool disabled)
        {
            StopAddToStockpileDelayCoroutine();
            StopTransitionToIdleDelayCoroutine();
        }

        private void StopAddToStockpileDelayCoroutine()
        {
            if (addToStockpileDelayCoroutine != null)
            {
                parentBehaviour.StopCoroutine(addToStockpileDelayCoroutine);
                addToStockpileDelayCoroutine = null;
            }
        }

        private void StopTransitionToIdleDelayCoroutine()
        {
            if (transitionToIdleDelayCoroutine != null)
            {
                parentBehaviour.StopCoroutine(transitionToIdleDelayCoroutine);
                transitionToIdleDelayCoroutine = null;
            }
        }

        private void AddToStockpile()
        {
            var targetGameObject = NPCUtils.GetTargetGameObject(parentBehaviour.gameObject, Owner.Data.TargetEntityId);
            if (targetGameObject != null && NPCUtils.IsTargetATeamStockpile(parentBehaviour.gameObject, targetGameObject))
            {
                var resourcesToAdd = inventory.Data.Resources;
                stockpileDepositoryRequestSender.SendAddResourceRequest(Owner.Data.TargetEntityId, new AddResource(resourcesToAdd));
            }
            else
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
            }
        }

        private void OnStockpileResponse(StockpileDepository.AddResource.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogError("NPC failed to receive Stockpile response");
            }
            else
            {
                inventory.RemoveFromInventory(response.RequestPayload.Quantity);
            }
            TransitionToIdle();
        }

        private void TransitionToIdle()
        {
            var waitAndPerfromTransition = TimerUtils.WaitAndPerform(SimulationSettings.NPCStockpilingAnimationFinishdelay, () =>
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
            });
            transitionToIdleDelayCoroutine = parentBehaviour.StartCoroutine(waitAndPerfromTransition);
        }
    }
}
