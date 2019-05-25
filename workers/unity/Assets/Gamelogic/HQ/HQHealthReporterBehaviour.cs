using Assets.Gamelogic.UI;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using Improbable.Team;
using UnityEngine;

namespace Assets.Gamelogic.HQ
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class HQHealthReporterBehaviour : MonoBehaviour
    {
        [Require] private Health.Requirable.Reader health;
        [Require] private TeamAssignment.Requirable.Reader teamAssignment;

        private void OnEnable()
        {
            health.ComponentUpdated += OnHealthUpdated;
            UpdateHQHealthBar(teamAssignment.Data.TeamId, health.Data.CurrentHealth);
        }

        private void OnDisable()
        {
        }

        private void OnHealthUpdated(Health.Update update)
        {
            if (update.CurrentHealth.HasValue)
            {
                UpdateHQHealthBar(teamAssignment.Data.TeamId, update.CurrentHealth.Value);
            }
        }

        private void UpdateHQHealthBar(uint teamId, float healthValue)
        {
            HQsPanelController.SetHQHealth(teamId, healthValue);
        }
    }
}
