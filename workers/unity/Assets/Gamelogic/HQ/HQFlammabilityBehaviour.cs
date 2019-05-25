using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using UnityEngine;

namespace Assets.Gamelogic.HQ
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class HQFlammabilityBehaviour : MonoBehaviour
    {
        [Require] private Health.Requirable.Writer health;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;
        [Require] private Flammable.Requirable.Writer flammable;
        [SerializeField] private FlammableBehaviour flammableBehaviour;

        private void Awake()
        {
            flammableBehaviour = gameObject.GetComponentIfUnassigned(flammableBehaviour);
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
                UpdateHQFlammablility(update.CurrentHealth.Value);
            }
        }

        private void UpdateHQFlammablility(int healthValue)
        {
            if (healthValue <= 0)
            {
                flammableBehaviour.SelfExtinguish(flammableRequestSender, false);
            }
            else
            {
                var canBeIgnited = healthValue > 0;
                flammableBehaviour.SelfSetCanBeIgnited(flammableRequestSender, canBeIgnited);
            }
        }
    }
}
