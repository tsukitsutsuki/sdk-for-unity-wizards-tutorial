using Assets.Gamelogic.ComponentExtensions;
using UnityEngine;
using Assets.Gamelogic.FSM;
using Improbable.Npc;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.NPC.Lumberjack;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Tree;
using Improbable.Worker.CInterop;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.NPC.LumberJack
{
    public class LumberjackHarvestingState : FsmBaseState<LumberjackStateMachine, LumberjackFSMState.StateEnum>
    {
        private readonly LumberjackBehaviour parentBehaviour;
        private readonly Inventory.Requirable.Writer inventory;
        private readonly Harvestable.Requirable.CommandRequestSender harvestableRequestSender;
        private readonly Harvestable.Requirable.CommandResponseHandler harvestableResponseHandler;

        private Coroutine harvestTreeDelayCoroutine;
        private Coroutine transitionToIdleDelayCoroutine;

        public LumberjackHarvestingState(LumberjackStateMachine owner,
                                         LumberjackBehaviour inParentBehaviour,
                                         Inventory.Requirable.Writer inInventory,
                                         Harvestable.Requirable.CommandRequestSender inHarvestableRequestSender,
                                         Harvestable.Requirable.CommandResponseHandler inHarvestableResponseHandler)
            : base(owner)
        {
            parentBehaviour = inParentBehaviour;
            inventory = inInventory;
            harvestableRequestSender = inHarvestableRequestSender;
            harvestableResponseHandler = inHarvestableResponseHandler;
            harvestableResponseHandler.OnHarvestResponse += OnHarvestResponse;
        }

        public override void Enter()
        {
            if (!inventory.HasResources())
            {
                harvestTreeDelayCoroutine = parentBehaviour.StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.NPCChoppingAnimationStartDelay, AttemptToHarvestTree));
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
            StopHarvestTreeDelayRoutine();
            StopTransitionToRoutine();
        }

        private void StopHarvestTreeDelayRoutine()
        {
            if (harvestTreeDelayCoroutine != null)
            {
                parentBehaviour.StopCoroutine(harvestTreeDelayCoroutine);
                harvestTreeDelayCoroutine = null;
            }
        }

        private void StopTransitionToRoutine()
        {
            if (transitionToIdleDelayCoroutine != null)
            {
                parentBehaviour.StopCoroutine(transitionToIdleDelayCoroutine);
                transitionToIdleDelayCoroutine = null;
            }
        }

        private void AttemptToHarvestTree()
        {
            var targetGameObject = NPCUtils.GetTargetGameObject(parentBehaviour.gameObject, Owner.Data.TargetEntityId);
            if (targetGameObject != null && NPCUtils.IsTargetAHealthyTree(parentBehaviour.gameObject, targetGameObject))
            {
                var parentComponent = parentBehaviour.gameObject.GetComponent<SpatialOSComponent>();
                harvestableRequestSender.SendHarvestRequest(Owner.Data.TargetEntityId,
                                               new HarvestRequest(parentComponent.SpatialEntityId),
                                               (uint)new System.TimeSpan(0, 0, 5).TotalMilliseconds);
            }
            else
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
            }
        }

        private void OnHarvestResponse(Harvestable.Harvest.ReceivedResponse response)
        {
            if (response.StatusCode != StatusCode.Success)
            {
                Debug.LogWarning("NPC failed to receive Harvest response");
            }
            else
            {
                if (response.ResponsePayload.HasValue)
                {
                    inventory.AddToInventory(response.ResponsePayload.Value.ResourcesTaken);
                }
            }
            TransitionToIdle();
        }

        private void TransitionToIdle()
        {
            var waitAndPerfromTransition = TimerUtils.WaitAndPerform(SimulationSettings.NPCChoppingAnimationFinishDelay, () =>
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
            });
            transitionToIdleDelayCoroutine = parentBehaviour.StartCoroutine(waitAndPerfromTransition);
        }
    }
}
