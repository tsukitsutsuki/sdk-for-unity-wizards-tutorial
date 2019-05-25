using System;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Fire
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class FlammableDataVisualizer : MonoBehaviour
    {
        [Require] private Flammable.Requirable.Reader flammable;
        public bool canBeIgnited { get; private set; }

        void OnEnable()
        {
            flammable.ComponentUpdated += FlammableOnComponentUpdated;
            canBeIgnited = flammable.Data.CanBeIgnited;
        }

        void OnDisable()
        {
            canBeIgnited = false;
        }

        private void FlammableOnComponentUpdated(Flammable.Update update)
        {
            canBeIgnited = flammable.Data.CanBeIgnited;
        }

        public void SetLocalCanBeIgnited(bool ignitable)
        {
            canBeIgnited = ignitable;
        }
    }
}
