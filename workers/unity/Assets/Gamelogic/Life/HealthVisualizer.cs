using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using UnityEngine;

namespace Assets.Gamelogic.Life
{
    public class HealthVisualizer : MonoBehaviour
    {
        [Require] private Health.Requirable.Reader health;

        public int CurrentHealth { get { return health != null ? health.Data.CurrentHealth : 0; } }
        public int MaxHealth { get { return health != null ? health.Data.MaxHealth : 0; } }

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
