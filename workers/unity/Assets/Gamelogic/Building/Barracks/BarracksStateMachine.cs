using Assets.Gamelogic.FSM;
using Improbable.Building;
using System.Collections.Generic;
using Assets.Gamelogic.Fire;
using Improbable.Life;
using UnityEngine;
using Improbable.Gdk.Core;
using Improbable.Fire;

namespace Assets.Gamelogic.Building.Barracks
{
    public class BarracksStateMachine : FiniteStateMachine<BarracksState>
    {
        private readonly BarracksInfo.Requirable.Writer barracksInfo;
        public BarracksState BarracksState; 

        private readonly StockpileDepository.Requirable.Writer stockpile;
        private readonly Health.Requirable.Writer health;
        private readonly FlammableBehaviour flammableBehaviour;
        private readonly Flammable.Requirable.CommandRequestSender flammableRequestSender;

        public BarracksStateMachine(BarracksInfo.Requirable.Writer inBarracksInfo,
                                    StockpileDepository.Requirable.Writer inStockpile,
                                    Health.Requirable.Writer inHealth, 
                                    FlammableBehaviour inFlammableBehaviour,
                                    Flammable.Requirable.CommandRequestSender inFlammableRequestSender,
                                    NPCSpawnerBehaviour npcSpawnerBehaviour)
        {
            barracksInfo = inBarracksInfo;
            stockpile = inStockpile;
            health = inHealth;
            flammableBehaviour = inFlammableBehaviour;
            flammableRequestSender = inFlammableRequestSender;

            var stateList = new Dictionary<BarracksState, IFsmState>
            {
                { BarracksState.UNDER_CONSTRUCTION, new BarracksUnderConstructionState(this, inHealth, npcSpawnerBehaviour) },
                { BarracksState.CONSTRUCTION_FINISHED, new BarracksConstructionFinishedState(this, inHealth, npcSpawnerBehaviour) }
            };
            SetStates(stateList);

            var allowedTransitions = new Dictionary<BarracksState, IList<BarracksState>>()
            {
                { BarracksState.UNDER_CONSTRUCTION, new List<BarracksState> { BarracksState.CONSTRUCTION_FINISHED } },
                { BarracksState.CONSTRUCTION_FINISHED, new List<BarracksState> { BarracksState.UNDER_CONSTRUCTION } }
            };
            SetTransitions(allowedTransitions);
        }

        public void TriggerTransition(BarracksState newState)
        {
            if (barracksInfo == null)
            {
                Debug.LogError("Trying to change state without authority.");
                return;
            }

            if (IsValidTransition(newState))
            {
                BarracksState = newState;

                var update = new BarracksInfo.Update();
                update.BarracksState = BarracksState;
                barracksInfo.Send(update);

                TransitionTo(newState);
            }
            else
            {
                Debug.LogErrorFormat("Barracks: Invalid transition from {0} to {1} detected.", BarracksState, newState);
            }
        }

        protected override void OnEnableImpl()
        {
            BarracksState = barracksInfo.Data.BarracksState;
        }

        public bool EvaluateCanAcceptResources()
        {
            return CurrentState == BarracksState.UNDER_CONSTRUCTION && health.Data.CanBeChanged && health.Data.CurrentHealth < health.Data.MaxHealth;
        }

        public void SetCanAcceptResources(bool canAcceptResources)
        {
            if (stockpile == null)
            {
                Debug.LogError("stockpile is null in BarracksStateMachine.");
                return;
            }
            if (stockpile.Data.CanAcceptResources != canAcceptResources)
            {
                stockpile.Send(new StockpileDepository.Update() { CanAcceptResources = new Option<BlittableBool>(canAcceptResources) });
            }
        }

        public void EvaluateAndSetFlammability(Health.Update update)
        {
            if (barracksInfo == null)
            {
                Debug.LogError("barracksInfo is null in BarracksStateMachine.");
                return;
            }

            if (flammableBehaviour == null)
            {
                Debug.LogError("flammableBehaviour is null in BarracksStateMachine.");
                return;
            }

            if (update.CurrentHealth.Value <= 0)
            {
                flammableBehaviour.SelfExtinguish(flammableRequestSender, false);
            }
            else
            {
                var canBeIgnited = update.CurrentHealth.Value > 0;
                flammableBehaviour.SelfSetCanBeIgnited(flammableRequestSender, canBeIgnited);
            }
        }
    }
}
