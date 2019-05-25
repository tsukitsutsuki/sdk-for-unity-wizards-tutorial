using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class CharacterModelVisualizer : MonoBehaviour
    {
        [SerializeField] private GameObject Model;

        public void SetModelVisibility(bool isVisible)
        {
            Model.SetActive(isVisible);
        }
    }
}
