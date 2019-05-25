using System.Collections;
using System.Collections.Generic;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Assets.Gamelogic.Fire;
using UnityEngine;
using Improbable.Abilities;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Life;
using Improbable.Player;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PlayerInfoBehaviour : MonoBehaviour
    {
        [Require] private PlayerInfo.Requirable.Writer playerInfo;
        [Require] private Health.Requirable.Writer health;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;
        [Require] private Spells.Requirable.Writer spells;
        [Require] private Inventory.Requirable.Writer inventory;

        [SerializeField] private TransformSender transformSender;
        [SerializeField] private FlammableBehaviour flammableInterface;

        private void Awake()
        {
            transformSender = gameObject.GetComponentIfUnassigned(transformSender);
            flammableInterface = gameObject.GetComponentIfUnassigned(flammableInterface);
        }

        private void OnEnable()
        {
            health.ComponentUpdated += OnHealthUpdated;
        }

        private void OnDisable()
        {
        }

        private void OnHealthUpdated(Health.Update update)
        {
            if (update.CurrentHealth.HasValue)
            {
                DieUponHealthDepletion(update);
            }
        }

        private void DieUponHealthDepletion(Health.Update update)
        {
            if (update.CurrentHealth.Value <= 0)
            {
                Die();
                StartCoroutine(RespawnDelayed(SimulationSettings.PlayerRespawnDelay));
            }
        }

        private void Die()
        {
            playerInfo.Send(new PlayerInfo.Update() { IsAlive = new Option<BlittableBool>(false) });
            health.Send(new Health.Update() { CanBeChanged = new Option<BlittableBool>(false) });
            flammableInterface.SelfExtinguish(flammableRequestSender, false);
            spells.Send(new Spells.Update()
            {
                Cooldowns = new Dictionary<SpellType, float> {
                    { SpellType.LIGHTNING, 0f },
                    { SpellType.RAIN, 0f }
                },
                CanCastSpells = new Option<BlittableBool>(false)
            });
        }

        private IEnumerator RespawnDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Respawn();
        }

        private void Respawn()
        {
            transformSender.TriggerTeleport(playerInfo.Data.InitialSpawnPosition.ToVector3());
            health.Send(new Health.Update()
            {
                CurrentHealth = SimulationSettings.PlayerMaxHealth,
                CanBeChanged = new Option<BlittableBool>(true),
            });
            flammableInterface.SelfSetCanBeIgnited(flammableRequestSender, true);
            spells.Send(new Spells.Update() { CanCastSpells = new Option<BlittableBool>(true) });
            inventory.Send(new Inventory.Update() { Resources = 0 });
            playerInfo.Send(new PlayerInfo.Update() { IsAlive = new Option<BlittableBool>(true) });
        }
    }
}
