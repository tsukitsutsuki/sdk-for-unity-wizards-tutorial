using UnityEngine;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerAuthority : MonoBehaviour
    {
        [Require] private ClientAuthorityCheck.Requirable.Writer authority;

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        public bool IsAuth()
        {
            return (authority != null);
        }
    }
}
