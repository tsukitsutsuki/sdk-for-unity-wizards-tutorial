using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.Utils;
using Improbable.Gdk.Core;
using Improbable.Fire;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.Tree
{
    public class TreeStumpState : FsmBaseState<TreeStateMachine, TreeFSMState>
    {
        private readonly TreeBehaviour parentBehaviour;
        private readonly Flammable.Requirable.Writer flammable;
        private readonly FlammableBehaviour flammableInterface;

        private Coroutine regrowingCoroutine;

        public TreeStumpState(TreeStateMachine owner, TreeBehaviour inParentBehaviour, Flammable.Requirable.Writer inFlammable) : base(owner)
        {
            parentBehaviour = inParentBehaviour;
            flammable = inFlammable;
        }

        public override void Enter()
        {
            flammable.Send(new Flammable.Update() { CanBeIgnited = new Option<BlittableBool>(false) });

            if (regrowingCoroutine == null)
            {
                regrowingCoroutine = parentBehaviour.StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.TreeStumpRegrowthTimeSecs, Regrow));
            }
        }

        private void Regrow()
        {
            Owner.TriggerTransition(TreeFSMState.HEALTHY);
        }

        public override void Tick()
        {

        }

        public override void Exit(bool disabled)
        {
            if (regrowingCoroutine != null)
            {
                parentBehaviour.StopCoroutine(regrowingCoroutine);
                regrowingCoroutine = null;
            }
        }
    }
}
