using Improbable.Building;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Building
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class BarracksModelVisualizer : MonoBehaviour
    {
        [Require] BarracksInfo.Requirable.Reader barracksInfo;
        
        [SerializeField] private ParticleSystem transition;
        [SerializeField] private GameObject buildingModel;
        [SerializeField] private GameObject stockpileModel;

        private void OnEnable()
        {
            SwitchToBarracksState(barracksInfo.Data.BarracksState);
            barracksInfo.ComponentUpdated += OnComponentUpdated;
        }

        private void OnDisable()
        {
        }

        private void OnComponentUpdated(BarracksInfo.Update update)
        {
            if (update.BarracksState.HasValue)
            {
                transition.Play();
                SwitchToBarracksState(update.BarracksState.Value);
            }
        }

        private void SwitchToBarracksState(BarracksState barracksState)
        {
            switch (barracksState)
            {
                case BarracksState.UNDER_CONSTRUCTION:
                    {
                        buildingModel.SetActive(false);
                        stockpileModel.SetActive(true);
                    }
                    break;
                case BarracksState.CONSTRUCTION_FINISHED:
                    {
                        buildingModel.SetActive(true);
                        stockpileModel.SetActive(false);
                    }
                    break;
            }
        }
    }
}
