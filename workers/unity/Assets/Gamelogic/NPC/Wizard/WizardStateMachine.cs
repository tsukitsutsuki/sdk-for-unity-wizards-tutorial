using Assets.Gamelogic.Abilities;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.Team;
using Assets.Gamelogic.Utils;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Npc;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.NPC.Wizard
{
    public class WizardStateMachine : FiniteStateMachine<WizardFSMState.StateEnum>
    {
        private readonly NPCWizard.Requirable.Writer npcWizard;
        public NPCWizardData Data;

        public WizardStateMachine(WizardBehaviour behaviour,
                                  NPCWizard.Requirable.Writer inNpcWizard,
                                  TargetNavigationBehaviour navigation,
                                  TeamAssignmentVisualizerUnityWorker teamAssignment,
                                  TargetNavigation.Requirable.Writer targetNavigation,
                                  SpellsBehaviour spellsBehaviour,
                                  IList<Coordinates> cachedTeamHqCoordinates)
        {
            npcWizard = inNpcWizard;

            var idleState = new WizardIdleState(this, teamAssignment, cachedTeamHqCoordinates);
            var moveToTargetState = new WizardMoveToTargetState(this, behaviour, targetNavigation, navigation);
            var attackingState = new WizardAttackingState(this, behaviour, spellsBehaviour);
            var defendingState = new WizardDefendingState(this, behaviour, spellsBehaviour);
            var onFireState = new WizardOnFireState(this, navigation, targetNavigation);

            var stateList = new Dictionary<WizardFSMState.StateEnum, IFsmState>()
            {
                { WizardFSMState.StateEnum.IDLE, idleState },
                { WizardFSMState.StateEnum.MOVING_TO_TARGET, moveToTargetState },
                { WizardFSMState.StateEnum.ATTACKING_TARGET, attackingState },
                { WizardFSMState.StateEnum.DEFENDING_TARGET, defendingState },
                { WizardFSMState.StateEnum.ON_FIRE, onFireState }
            };

            SetStates(stateList);

            var allowedTransitions = new Dictionary<WizardFSMState.StateEnum, IList<WizardFSMState.StateEnum>>();

            allowedTransitions.Add(WizardFSMState.StateEnum.IDLE, new List<WizardFSMState.StateEnum>
                                   {
                                       WizardFSMState.StateEnum.IDLE,
                                       WizardFSMState.StateEnum.MOVING_TO_TARGET,
                                       WizardFSMState.StateEnum.ON_FIRE
                                   });

            allowedTransitions.Add(WizardFSMState.StateEnum.MOVING_TO_TARGET, new List<WizardFSMState.StateEnum>
                                   {
                                       WizardFSMState.StateEnum.IDLE,
                                       WizardFSMState.StateEnum.MOVING_TO_TARGET,
                                       WizardFSMState.StateEnum.ATTACKING_TARGET,
                                       WizardFSMState.StateEnum.DEFENDING_TARGET,
                                       WizardFSMState.StateEnum.ON_FIRE
                                   });

            allowedTransitions.Add(WizardFSMState.StateEnum.ATTACKING_TARGET, new List<WizardFSMState.StateEnum>
                                   {
                                       WizardFSMState.StateEnum.IDLE,
                                       WizardFSMState.StateEnum.ON_FIRE
                                   });

            allowedTransitions.Add(WizardFSMState.StateEnum.DEFENDING_TARGET, new List<WizardFSMState.StateEnum>
                                   {
                                       WizardFSMState.StateEnum.IDLE,
                                       WizardFSMState.StateEnum.ON_FIRE
                                   });

            allowedTransitions.Add(WizardFSMState.StateEnum.ON_FIRE, new List<WizardFSMState.StateEnum>
                                   {
                                       WizardFSMState.StateEnum.IDLE,
                                       WizardFSMState.StateEnum.ON_FIRE
                                   });

            SetTransitions(allowedTransitions);
        }

        public void TriggerTransition(WizardFSMState.StateEnum newState, EntityId targetEntityId, Vector3 targetPosition)
        {
            if (npcWizard == null)
            {
                Debug.LogError("Trying to change state without authority.");
                return;
            }

            if (IsValidTransition(newState))
            {
                Data.CurrentState = newState;
                Data.TargetEntityId = targetEntityId;
                Data.TargetPosition = targetPosition.FlattenVector().ToVector3f();

                var update = new NPCWizard.Update();
                update.CurrentState = Data.CurrentState;
                update.TargetEntityId = Data.TargetEntityId;
                update.TargetPosition = Data.TargetPosition;
                npcWizard.Send(update);

                TransitionTo(newState);
            }
            else
            {
                Debug.LogErrorFormat("NPCWizard: Invalid transition from {0} to {1} detected.", Data.CurrentState, newState);
            }
        }

        protected override void OnEnableImpl()
        {
            Data = new NPCWizardData(npcWizard.Data.CurrentState, npcWizard.Data.TargetEntityId, npcWizard.Data.TargetPosition);
        }
    }
}
