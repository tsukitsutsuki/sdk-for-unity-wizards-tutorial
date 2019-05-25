using Assets.Gamelogic.Core;
using Assets.Gamelogic.UI;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using Improbable.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerInfoVisualizer : MonoBehaviour
    {
        [Require] private ClientAuthorityCheck.Requirable.Writer clientAuthorityCheck;
        [Require] private PlayerInfo.Requirable.Reader playerInfo;
        [Require] private Health.Requirable.Reader health;
        [Require] private Flammable.Requirable.Reader flammable;

        private float healthLocalCopy;

        [SerializeField] private CharacterModelVisualizer characterModelVisualizer;

        private void Awake()
        {
            characterModelVisualizer = gameObject.GetComponentIfUnassigned(characterModelVisualizer);
        }

        private void OnEnable()
        {
            playerInfo.ComponentUpdated += OnPlayerInfoUpdated;
            MainCameraController.SetTarget(gameObject);
            UIController.ShowUI();
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        private void OnDisable()
        {
        }

        private void OnPlayerInfoUpdated(PlayerInfo.Update update)
        {
            if (update.IsAlive.HasValue)
            {
                if (update.IsAlive.Value == true)
                {
                    Resurrect();
                }
                else
                {
                    Die();
                }
            }
        }

        private void Resurrect()
        {
            GameNotificationsPanelController.SetText("");
            characterModelVisualizer.SetModelVisibility(true);
        }

        private void Die()
        {
            GameNotificationsPanelController.SetText("You have died.");
        }
    }
}
