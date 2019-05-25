using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.NPC.LumberJack;
using Assets.Gamelogic.Utils;
using Improbable.Gdk.Core;
using Improbable.Npc;
using UnityEngine;

namespace Assets.Gamelogic.NPC.Lumberjack
{
    public class LumberjackMoveToTargetState : FsmBaseState<LumberjackStateMachine, LumberjackFSMState.StateEnum>
    {
        private readonly TargetNavigation.Requirable.Writer targetNavigation;
        private readonly LumberjackBehaviour parentBehaviour;
        private readonly TargetNavigationBehaviour navigation;
        
        private Coroutine interactionWithTargetDelayCoroutine;
        private bool isEnter = false;

        public LumberjackMoveToTargetState(LumberjackStateMachine owner,
                                           LumberjackBehaviour inParentBehaviour,
                                           TargetNavigation.Requirable.Writer inTargetNavigation,
                                           TargetNavigationBehaviour inNavigation)
            : base(owner)
        {
            targetNavigation = inTargetNavigation;
            parentBehaviour = inParentBehaviour;
            navigation = inNavigation;

            targetNavigation.OnNavigationFinished += OnTargetNavigationUpdated;
        }

        public override void Enter()
        {
            isEnter = true;
            StartMovingTowardsTarget();
        }

        public override void Tick()
        {
        }

        public override void Exit(bool disabled)
        {
            StopInteractionWithTargetDelayCoroutine();
            isEnter = false;
        }

        private void StopInteractionWithTargetDelayCoroutine()
        {
            if (interactionWithTargetDelayCoroutine != null)
            {
                parentBehaviour.StopCoroutine(interactionWithTargetDelayCoroutine);
                interactionWithTargetDelayCoroutine = null;
            }
        }

        private void StartMovingTowardsTarget()
        {
            if (TargetIsEntity())
            {
                StartMovingTowardsTargetEntity();
            }
            else
            {
                StartMovingTowardsTargetPosition();
            }
        }

        private void StartMovingTowardsTargetEntity()
        {
            var targetGameObject = NPCUtils.GetTargetGameObject(parentBehaviour.gameObject, Owner.Data.TargetEntityId);
            if (targetGameObject == null)
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
                return;
            }
            if (NPCUtils.IsWithinInteractionRange(parentBehaviour.transform.position, targetGameObject.transform.position, SimulationSettings.NPCDefaultInteractionSqrDistance))
            {
                InitiateInteractionWithTarget();
                return;
            }
            navigation.StartNavigation(Owner.Data.TargetEntityId, SimulationSettings.NPCDefaultInteractionSqrDistance);
        }

        private void InitiateInteractionWithTarget()
        {
            StopInteractionWithTargetDelayCoroutine();
            interactionWithTargetDelayCoroutine = parentBehaviour.StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.NPCInteractionDelay, AttemptInteractionWithTarget));
        }

        private void StartMovingTowardsTargetPosition()
        {
            var targetPosition = Owner.Data.TargetPosition.ToVector3();
            if (MathUtils.CompareEqualityEpsilon(targetPosition, SimulationSettings.InvalidPosition))
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
                return;
            }
            if (NPCUtils.IsWithinInteractionRange(parentBehaviour.transform.position, targetPosition, SimulationSettings.NPCDefaultInteractionSqrDistance))
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
                return;
            }
            navigation.StartNavigation(targetPosition, SimulationSettings.NPCDefaultInteractionSqrDistance);
        }

        private void OnTargetNavigationUpdated(NavigationFinished update)
        {
            if (!isEnter) return;

            var success = update.Success;
            if (success)
            {
                if (TargetIsEntity())
                {
                    InitiateInteractionWithTarget();
                }
                else
                {
                    Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
                }
            }
            else
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
            }
        }

        private void AttemptInteractionWithTarget()
        {
            var targetGameObject = NPCUtils.GetTargetGameObject(parentBehaviour.gameObject, Owner.Data.TargetEntityId);
            if (targetGameObject == null)
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
                return;
            }
            if (NPCUtils.IsTargetAHealthyTree(parentBehaviour.gameObject, targetGameObject) &&
                NPCUtils.IsWithinInteractionRange(parentBehaviour.transform.position, targetGameObject.transform.position, SimulationSettings.NPCDefaultInteractionSqrDistance))
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.HARVESTING, Owner.Data.TargetEntityId, SimulationSettings.InvalidPosition);
                return;
            }
            if (NPCUtils.IsTargetATeamStockpile(parentBehaviour.gameObject, targetGameObject) &&
                NPCUtils.IsWithinInteractionRange(parentBehaviour.transform.position, targetGameObject.transform.position, SimulationSettings.NPCDefaultInteractionSqrDistance))
            {
                Owner.TriggerTransition(LumberjackFSMState.StateEnum.STOCKPILING, Owner.Data.TargetEntityId, SimulationSettings.InvalidPosition);
                return;
            }
            Owner.TriggerTransition(LumberjackFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
        }

        private bool TargetIsEntity()
        {
            return Owner.Data.TargetEntityId.IsValid();
        }
    }
}
