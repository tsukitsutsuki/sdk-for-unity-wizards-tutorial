using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Improbable.Building;
using Improbable.Life;

namespace Assets.Gamelogic.Building.Barracks
{
    public class BarracksUnderConstructionState : FsmBaseState<BarracksStateMachine, BarracksState>
    {
        private readonly Health.Requirable.Reader health;
        private readonly NPCSpawnerBehaviour npcSpawnerBehaviour;
        private bool isEnter = false;

        public BarracksUnderConstructionState(BarracksStateMachine owner, 
                                              Health.Requirable.Reader inHealth, 
                                              NPCSpawnerBehaviour inNPCSpawnerBehaviour) : base(owner)
        {
            health = inHealth;
            npcSpawnerBehaviour = inNPCSpawnerBehaviour;
            health.ComponentUpdated += OnHealthUpdated;
        }

        public override void Enter()
        {
            Owner.SetCanAcceptResources(true);
            npcSpawnerBehaviour.SetSpawningEnabled(false);

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
                EvaluateAndTransitionToConstructionFinishedState(update);
            }
        }

        private void EvaluateAndTransitionToConstructionFinishedState(Health.Update update)
        {
            if (update.CurrentHealth.Value == SimulationSettings.BarracksMaxHealth)
            {
                Owner.TriggerTransition(BarracksState.CONSTRUCTION_FINISHED);
            }
        }
    }
}
