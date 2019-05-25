using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Improbable.Npc;

namespace Assets.Gamelogic.NPC.Wizard
{
    public class WizardOnFireState : FsmBaseState<WizardStateMachine, WizardFSMState.StateEnum>
    {
        private readonly TargetNavigation.Requirable.Writer targetNavigation;
        private readonly TargetNavigationBehaviour navigation;
        private bool isEnter = false;

        public WizardOnFireState(WizardStateMachine owner,
                                 TargetNavigationBehaviour inNavigation,
                                 TargetNavigation.Requirable.Writer inTargetNavigation) 
            : base(owner)
        {
            navigation = inNavigation;
            targetNavigation = inTargetNavigation;
            targetNavigation.OnNavigationFinished += OnTargetNavigationComponentUpdate;
        }

        public override void Enter()
        {
            isEnter = true;
            NPCUtils.NavigateToRandomNearbyPosition(navigation, navigation.transform.position, SimulationSettings.NPCOnFireWaypointDistance, SimulationSettings.NPCDefaultInteractionSqrDistance);
        }

        public override void Tick()
        {
        }

        public override void Exit(bool disabled)
        {
            if (!disabled)
            {
                navigation.StopNavigation();
            }
            isEnter = false;
        }

        private void OnTargetNavigationComponentUpdate(NavigationFinished update)
        {
            if (!isEnter) return;
            NPCUtils.NavigateToRandomNearbyPosition(navigation, navigation.transform.position, SimulationSettings.NPCOnFireWaypointDistance, SimulationSettings.NPCDefaultInteractionSqrDistance);
        }
    }
}
