using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.NPC.LumberJack;
using Improbable.Npc;

namespace Assets.Gamelogic.NPC.Lumberjack
{
    public class LumberjackOnFireState : FsmBaseState<LumberjackStateMachine, LumberjackFSMState.StateEnum>
    {
        private readonly TargetNavigationBehaviour navigation;
        private readonly TargetNavigation.Requirable.Writer targetNavigation;

        public LumberjackOnFireState(LumberjackStateMachine owner,
                                     TargetNavigationBehaviour inNavigation,
                                     TargetNavigation.Requirable.Writer inTargetNavigation)
            : base(owner)
        {
            navigation = inNavigation;
            targetNavigation = inTargetNavigation;
        }

        public override void Enter()
        {
            targetNavigation.OnNavigationFinished += OnTargetNavigationUpdated;
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
        }

        private void OnTargetNavigationUpdated(NavigationFinished update)
        {
            NPCUtils.NavigateToRandomNearbyPosition(navigation, navigation.transform.position, SimulationSettings.NPCOnFireWaypointDistance, SimulationSettings.NPCDefaultInteractionSqrDistance);
        }
    }
}
