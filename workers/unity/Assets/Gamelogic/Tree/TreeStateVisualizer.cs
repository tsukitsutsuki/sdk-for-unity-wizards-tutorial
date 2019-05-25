using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.Tree
{
    public class TreeStateVisualizer : MonoBehaviour
    {
        [Require] private TreeState.Requirable.Reader treeState;
        public TreeState.Requirable.Reader CurrentState { get { return treeState; } }

        private void Awake()
        {
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
    }
}
