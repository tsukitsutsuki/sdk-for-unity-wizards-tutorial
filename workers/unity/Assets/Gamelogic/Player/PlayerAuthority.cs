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

        public static bool IsAuth(GameObject player)
        {
            var auth = player.GetComponent<PlayerAuthority>();
            return (auth != null && auth.isActiveAndEnabled);
        }
    }
}
