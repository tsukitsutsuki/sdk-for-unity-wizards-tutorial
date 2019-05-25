using Assets.Gamelogic.Building.Barracks;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Fire;
using Improbable.Building;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Life;
using UnityEngine;

namespace Assets.Gamelogic.Building
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class BarracksBehaviour : MonoBehaviour, IFlammable
    {
        [Require] private BarracksInfo.Requirable.Writer barracksInfo;
        [Require] private StockpileDepository.Requirable.Writer stockpileDepository;
        [Require] private Health.Requirable.Writer health;
        [Require] private Flammable.Requirable.Writer flammable;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;
        [Require] private NPCSpawner.Requirable.Writer npcSpawner;
        
        [SerializeField] private FlammableBehaviour flammableBehaviour;
        [SerializeField] private NPCSpawnerBehaviour npcSpawnerBehaviour;

        private BarracksStateMachine barracksStateMachine;

        private void Awake()
        {
            flammableBehaviour = gameObject.GetComponentIfUnassigned(flammableBehaviour);
            npcSpawnerBehaviour = gameObject.GetComponentIfUnassigned(npcSpawnerBehaviour);
        }

        private void OnEnable()
        {
            barracksStateMachine = new BarracksStateMachine(barracksInfo, stockpileDepository, health, flammableBehaviour, flammableRequestSender, npcSpawnerBehaviour);
            barracksStateMachine.OnEnable(barracksInfo.Data.BarracksState);
        }

        private void OnDisable()
        {
            barracksStateMachine.OnDisable();
        }

        public void OnIgnite()
        {
            barracksStateMachine.SetCanAcceptResources(false);
        }

        public void OnExtinguish()
        {
            barracksStateMachine.SetCanAcceptResources(barracksStateMachine.EvaluateCanAcceptResources());
        }
    }
}
