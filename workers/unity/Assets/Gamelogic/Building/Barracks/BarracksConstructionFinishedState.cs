using Assets.Gamelogic.FSM;
using Improbable.Building;
using Improbable.Life;

namespace Assets.Gamelogic.Building.Barracks
{
    public class BarracksConstructionFinishedState : FsmBaseState<BarracksStateMachine, BarracksState>
    {
        private readonly Health.Requirable.Writer health;
        private readonly NPCSpawnerBehaviour npcSpawnerBehaviour;
        private bool isEnter = false;

        public BarracksConstructionFinishedState(BarracksStateMachine owner, 
                                                 Health.Requirable.Writer inHealth,
                                                 NPCSpawnerBehaviour inNPCSpawnerBehaviour) : base(owner)
        {
            health = inHealth;
            npcSpawnerBehaviour = inNPCSpawnerBehaviour;
            health.ComponentUpdated += OnHealthUpdated;
        }

        public override void Enter()
        {
            Owner.SetCanAcceptResources(false);
            npcSpawnerBehaviour.SetSpawningEnabled(true);
            npcSpawnerBehaviour.ResetCooldowns();

            isEnter = true;
        }

        public override void Tick()
        {
        }

        public override void Exit(bool disabled)
        {
            isEnter = false;
        }

        private void OnHealthUpdated(Health.Update update)
        {
            if (isEnter && update.CurrentHealth.HasValue)
            {
                Owner.EvaluateAndSetFlammability(update);
                EvaluateAndTransitionToUnderConstructionState(update);
            }
        }
        
        private void EvaluateAndTransitionToUnderConstructionState(Health.Update update)
        {
            if (isEnter && update.CurrentHealth.Value <= 0)
            {
                Owner.TriggerTransition(BarracksState.UNDER_CONSTRUCTION);
            }
        }
    }
}
