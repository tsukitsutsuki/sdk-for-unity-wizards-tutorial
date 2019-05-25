using System.Collections;
using Assets.Gamelogic.Abilities;
using Assets.Gamelogic.Core;
using Improbable.Abilities;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Player;
using UnityEngine;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerInputListener : MonoBehaviour
    {
        [Require] private ClientAuthorityCheck.Requirable.Writer clientAuthorityCheck;
        [Require] private PlayerInfo.Requirable.Reader playerInfo;

        private bool controlsEnabled;
        private Vector3 inputDirection = Vector3.zero;

        [SerializeField] private PlayerControlsSender playerControlsSender;
        [SerializeField] private SpellsRequester spellsBehaviour;

        private void Awake()
        {
            playerControlsSender = gameObject.GetComponentIfUnassigned(playerControlsSender);
            spellsBehaviour = gameObject.GetComponentIfUnassigned(spellsBehaviour);
        }

        private void OnEnable()
        {
            EnableControls();
            playerInfo.ComponentUpdated += OnPlayerInfoUpdated;
        }

        private void OnDisable()
        {
            DisableControls();
        }

        private void OnPlayerInfoUpdated(PlayerInfo.Update update)
        {
            if (update.IsAlive.HasValue)
            {
                if (update.IsAlive.Value == true)
                {
                    EnableControls();
                }
                else
                {
                    DisableControls();
                }
            }
        }

        public void EnableControls()
        {
            controlsEnabled = true;
        }

        public void DisableControls()
        {
            playerControlsSender.SetInputDirection(Vector3.zero);
            spellsBehaviour.DeactivateSpellCastingMode();
            controlsEnabled = false;
        }

        private void Update()
        {
            UpdateMovementDirection();
            UpdateSpellControls();
        }

        public void DisableInputForSpellcast()
        {
            playerControlsSender.SetInputDirection(Vector3.zero);
            enabled = false;
            StartCoroutine(EnableControlsAfter(SimulationSettings.PlayerCastAnimationTime));

        }

        private IEnumerator EnableControlsAfter(float playerCastAnimationTime)
        {
            yield return new WaitForSeconds(playerCastAnimationTime);
            enabled = true;
        }

        private void UpdateMovementDirection()
        {
            if (!controlsEnabled)
            {
                return;
            }
            inputDirection.x = Input.GetAxis("Horizontal");
            inputDirection.z = Input.GetAxis("Vertical");
            playerControlsSender.SetInputDirection(inputDirection);
        }

        private void UpdateSpellControls()
        {
            if (!controlsEnabled)
            {
                return;
            }
            if (Input.GetKeyDown(SimulationSettings.CastLightningKey))
            {
                if (!spellsBehaviour.SpellCastingModeActive && spellsBehaviour.GetLocalSpellCooldown(SpellType.LIGHTNING) <= 0f)
                {
                    spellsBehaviour.ActivateSpellCastingMode(SpellType.LIGHTNING);
                }
                else
                {
                    spellsBehaviour.DeactivateSpellCastingMode();
                }
            }
            if (Input.GetKeyDown(SimulationSettings.CastRainKey))
            {
                if (!spellsBehaviour.SpellCastingModeActive && spellsBehaviour.GetLocalSpellCooldown(SpellType.RAIN) <= 0f)
                {
                    spellsBehaviour.ActivateSpellCastingMode(SpellType.RAIN);
                }
                else
                {
                    spellsBehaviour.DeactivateSpellCastingMode();
                }
            }
            if (Input.GetMouseButtonDown(SimulationSettings.CastSpellMouseButton) && spellsBehaviour.SpellCastingModeActive) spellsBehaviour.AttemptToCastSpell();
            if (Input.GetKeyDown(SimulationSettings.AbortKey) && spellsBehaviour.SpellCastingModeActive) spellsBehaviour.DeactivateSpellCastingMode();
        }
    }
}
