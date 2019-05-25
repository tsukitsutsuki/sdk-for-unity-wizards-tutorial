using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.Tree
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TreeBehaviour : MonoBehaviour
    {
        [Require] private TreeState.Requirable.Writer tree;
        [Require] private Flammable.Requirable.Writer flammable;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;
        [Require] private Health.Requirable.Writer health;

        [SerializeField] private FlammableBehaviour flammableInterface;

        private TreeStateMachine stateMachine;

        private void Awake()
        {
            flammableInterface = gameObject.GetComponentIfUnassigned(flammableInterface);
        }

        private void OnEnable()
        {
            stateMachine = new TreeStateMachine(this, 
                tree,
                health,
                flammableInterface,
                flammable,
                flammableRequestSender);

            stateMachine.OnEnable(tree.Data.CurrentState);
        }

        private void OnDisable()
        {
            stateMachine.OnDisable();
        }

    }
}
