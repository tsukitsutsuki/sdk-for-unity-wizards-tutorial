using Assets.Gamelogic.FSM;
using Improbable.Fire;
using Improbable.Gdk.Core;
using Improbable.Life;
using Improbable.Tree;

namespace Assets.Gamelogic.Tree
{
    public class TreeBurningState : FsmBaseState<TreeStateMachine, TreeFSMState>
    {
        private readonly Flammable.Requirable.Writer flammable;
        private readonly Health.Requirable.Writer health;
        private bool isEnter = false;

        public TreeBurningState(TreeStateMachine owner, Flammable.Requirable.Writer inFlammable, Health.Requirable.Writer inHealth) 
            : base(owner)
        {
            flammable = inFlammable;
            health = inHealth;

            flammable.ComponentUpdated += OnFlammableUpdated;
            health.ComponentUpdated += OnHealthUpdated;
        }

        public override void Enter()
        {
            flammable.Send(new Flammable.Update() { CanBeIgnited = new Option<BlittableBool>(false) });
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
                Owner.TriggerTransition(TreeFSMState.BURNT);
            }
        }

        private void OnFlammableUpdated(Flammable.Update update)
        {
            if (isEnter && HasBeenExtinguished(update))
            {
                Owner.TriggerTransition(TreeFSMState.HEALTHY);
            }
        }

        private bool HasBeenExtinguished(Flammable.Update flammableUpdate)
        {
            return flammableUpdate.IsOnFire.HasValue && !flammableUpdate.IsOnFire.Value;
        }
    }
}
