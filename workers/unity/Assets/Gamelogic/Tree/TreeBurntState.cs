using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Assets.Gamelogic.FSM;
using Assets.Gamelogic.Utils;
using Improbable.Fire;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.Tree
{
    public class TreeBurntState : FsmBaseState<TreeStateMachine, TreeFSMState>
    {
        private readonly TreeBehaviour parentBehaviour;
        private readonly Flammable.Requirable.CommandRequestSender flammableRequestSender;
        private readonly FlammableBehaviour flammableInterface;

        private Coroutine regrowingCoroutine;

        public TreeBurntState(TreeStateMachine owner, TreeBehaviour inParentBehaviour, Flammable.Requirable.CommandRequestSender inFlammableRequestSender, FlammableBehaviour inFlammableInterface) : base(owner)
        {
            parentBehaviour = inParentBehaviour;
            flammableRequestSender = inFlammableRequestSender;
            flammableInterface = inFlammableInterface;
        }

        public override void Enter()
        {
            flammableInterface.SelfExtinguish(flammableRequestSender, false);
            if (regrowingCoroutine == null)
            {
                regrowingCoroutine = parentBehaviour.StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.BurntTreeRegrowthTimeSecs, Regrow));
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
