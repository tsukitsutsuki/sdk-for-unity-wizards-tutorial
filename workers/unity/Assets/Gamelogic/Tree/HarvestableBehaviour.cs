using Assets.Gamelogic.ComponentExtensions;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Life;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using Improbable.Tree;
using UnityEngine;

namespace Assets.Gamelogic.Tree
{
    public class HarvestableBehaviour : MonoBehaviour
    {
        [Require] private Harvestable.Requirable.CommandRequestHandler harvestable;
        [Require] private Health.Requirable.Writer health;

        private void OnEnable()
        {
            harvestable.OnHarvestRequest += OnHarvest;
        }

        private void OnDisable()
        {
        }

        private void OnHarvest(Harvestable.Harvest.RequestResponder request)
        {
            var resourcesToGive = Mathf.Min(SimulationSettings.HarvestReturnQuantity, health.Data.CurrentHealth);
            health.AddCurrentHealthDelta(-resourcesToGive);
            request.SendResponse(new HarvestResponse(resourcesToGive));
        }
    }
}
