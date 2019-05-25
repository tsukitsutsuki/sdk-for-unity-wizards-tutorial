using Assets.Gamelogic.Abilities;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Assets.Gamelogic.Team;
using System.Collections.Generic;
using UnityEngine;
using Improbable;
using Improbable.Abilities;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Npc;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.NPC.Wizard
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class WizardBehaviour : MonoBehaviour, IFlammable
    {
        [Require] private NPCWizard.Requirable.Writer npcWizard;
        [Require] private TargetNavigation.Requirable.Writer targetNavigation;
        [Require] private Spells.Requirable.Writer spells;

        [SerializeField] private SpellsBehaviour spellsBehaviour;
        [SerializeField] private TeamAssignmentVisualizerUnityWorker teamAssignment;
        [SerializeField] private TargetNavigationBehaviour navigation;
        [SerializeField] private List<Coordinates> cachedTeamHqCoordinates;

        private WizardStateMachine stateMachine;

        private void Awake()
        {
            navigation = gameObject.GetComponentIfUnassigned(navigation);
            teamAssignment = gameObject.GetComponentIfUnassigned(teamAssignment);
            spellsBehaviour = gameObject.GetComponentIfUnassigned(spellsBehaviour);
        }

        private void OnEnable()
        {
            cachedTeamHqCoordinates = new List<Coordinates>(SimulationSettings.TeamHQLocations);
            stateMachine = new WizardStateMachine(this, npcWizard, navigation, teamAssignment, targetNavigation, spellsBehaviour, cachedTeamHqCoordinates);
            stateMachine.OnEnable(npcWizard.Data.CurrentState);
        }

        private void OnDisable()
        {
            stateMachine.OnDisable();
        }

        public void OnIgnite()
        {
            stateMachine.TriggerTransition(WizardFSMState.StateEnum.ON_FIRE, new EntityId(), SimulationSettings.InvalidPosition);
        }

        public void OnExtinguish()
        {
            stateMachine.TriggerTransition(WizardFSMState.StateEnum.IDLE, new EntityId(), SimulationSettings.InvalidPosition);
        }
    }
}
