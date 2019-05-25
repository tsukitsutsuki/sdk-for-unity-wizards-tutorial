using Improbable.Gdk.Core;
using Improbable.Life;
using UnityEngine;

namespace Assets.Gamelogic.ComponentExtensions
{
    static class HealthExtensions
    {
        public static void SetCanBeChanged(this Health.Requirable.Writer health, bool canBeChanged)
        {
            health.Send(new Health.Update() { CanBeChanged = new Option<BlittableBool>(canBeChanged) });
        }

        public static void SetCurrentHealth(this Health.Requirable.Writer health, int newHealth)
        {
            if (health.Data.CanBeChanged)
            {
                health.Send(new Health.Update() { CurrentHealth = Mathf.Max(newHealth, 0) });
            }
        }

        public static void AddCurrentHealthDelta(this Health.Requirable.Writer health, int delta)
        {
            if (health.Data.CanBeChanged)
            {
                if (health.TryingToDecreaseHealthBelowZero(delta))
                {
                    return;
                }
                health.Send(new Health.Update() { CurrentHealth = Mathf.Max(health.Data.CurrentHealth + delta, 0) });
            }
        }

        private static bool TryingToDecreaseHealthBelowZero(this Health.Requirable.Reader health, int delta)
        {
            return health.Data.CurrentHealth == 0 && delta < 0;
        }
    }
}
