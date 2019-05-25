using Assets.Gamelogic.ComponentExtensions;
using Improbable.Building;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using UnityEngine;

namespace Assets.Gamelogic.Building
{
    public class StockpileDepositoryBehaviour : MonoBehaviour
    {
        [Require] private StockpileDepository.Requirable.Writer stockpileDepository;
        [Require] private StockpileDepository.Requirable.CommandRequestHandler stockpileDepositoryRequestHandler;
        [Require] private Health.Requirable.Writer health;

        private void OnEnable ()
        {
            stockpileDepositoryRequestHandler.OnAddResourceRequest += OnAddResource;
        }

        private void OnDisable()
        {
        }

        private void OnAddResource(StockpileDepository.AddResource.RequestResponder request)
        {
            if (stockpileDepository.Data.CanAcceptResources)
            {
                health.AddCurrentHealthDelta(request.Request.Payload.Quantity);
            }
            request.SendResponse(new Improbable.Core.Nothing());
        }
    }
}
