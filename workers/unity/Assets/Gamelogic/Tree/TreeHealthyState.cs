using Assets.Gamelogic.Core;
using Assets.Gamelogic.FSM;
using Improbable.Tree;
using Improbable.Fire;
using Improbable.Life;
using Assets.Gamelogic.ComponentExtensions;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.Tree
{
    public class TreeHealthyState : FsmBaseState<TreeStateMachine, TreeFSMState>
    {
        private readonly Flammable.Requirable.Writer flammable;
        private readonly Health.Requirable.Writer health;
        private bool isEnter = false;

        public TreeHealthyState(TreeStateMachine owner, Flammable.Requirable.Writer inFlammable, Health.Requirable.Writer inHealth) 
            : base(owner)
        {
            flammable = inFlammable;
            health = inHealth;

            flammable.ComponentUpdated += OnFlammableUpdated;
            health.ComponentUpdated += OnHealthUpdated;
        }

        public override void Enter()
        {
            health.SetCurrentHealth(SimulationSettings.TreeMaxHealth);
            flammable.Send(new Flammable.Update() { CanBeIgnited = new Option<BlittableBool>(true) });
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
            if (isEnter && update.CurrentHealth.HasValue && update.CurrentHealth.Value <= 0) 
            {
                Owner.TriggerTransition(TreeFSMState.STUMP);
            }
        }

        private void OnFlammableUpdated(Flammable.Update update)
        {
            if (isEnter && HasBeenIgnited(update))
            {
                Owner.TriggerTransition(TreeFSMState.BURNING);
            }
        }

        private bool HasBeenIgnited(Flammable.Update flammableUpdate)
        {
            return flammableUpdate.IsOnFire.HasValue && flammableUpdate.IsOnFire.Value;
        }
    }
}
