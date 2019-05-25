using Improbable.Core;
using Improbable.Fire;
using UnityEngine;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Gdk.Core;

namespace Assets.Gamelogic.Fire
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class FlammableBehaviour : MonoBehaviour
    {
        [Require] private Flammable.Requirable.Writer flammable;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;
        [Require] private Flammable.Requirable.CommandRequestHandler flammableRequestHandler;

        public bool IsOnFire { get { return flammable != null && flammable.Data.IsOnFire; } }
        private Collider[] nearbyColliders = new Collider[8];
        private Coroutine spreadFireCoroutine;

        private IFlammable[] flammableInterfaces;

        private SpatialOSComponent spatialComponent;

        private void Awake()
        {
            flammableInterfaces = gameObject.GetComponents<IFlammable>();
        }

        private void OnEnable()
        {
            spatialComponent = GetComponent<SpatialOSComponent>();

            flammableRequestHandler.OnIgniteRequest += OnIgnite;
            flammableRequestHandler.OnExtinguishRequest += OnExtinguish;
            flammableRequestHandler.OnSetCanBeIgnitedRequest += OnSetCanBeIgnited;

            if (flammable.Data.IsOnFire)
            {
                StartSpreadingFire();
            }
        }

        private void OnDisable()
        {
            StopSpreadingFire();
        }

        private void OnIgnite(Flammable.Ignite.RequestResponder request)
        {
            Ignite();
        }

        private void OnExtinguish(Flammable.Extinguish.RequestResponder request)
        {
            Extinguish(request.Request.Payload.CanBeIgnited);
        }

        private void OnSetCanBeIgnited(Flammable.SetCanBeIgnited.RequestResponder request)
        {
            SetCanBeIgnited(request.Request.Payload.CanBeIgnited);
        }

        private void Ignite()
        {
            if (!flammable.Data.IsOnFire && flammable.Data.CanBeIgnited)
            {
                SendIgniteUpdate();
                StartSpreadingFire();
                for (var i = 0; i < flammableInterfaces.Length; i++)
                {
                    flammableInterfaces[i].OnIgnite();
                }
            }
        }

        private void Extinguish(bool canBeIgnited)
        {
            if (flammable.Data.IsOnFire)
            {
                SendExtinguishUpdate(canBeIgnited);
                StopSpreadingFire();
                for (var i = 0; i < flammableInterfaces.Length; i++)
                {
                    flammableInterfaces[i].OnExtinguish();
                }
            }
        }

        private void SetCanBeIgnited(bool canBeIgnited)
        {
            if (flammable.Data.CanBeIgnited != canBeIgnited)
            {
                flammable.Send(new Flammable.Update() { CanBeIgnited = new Option<BlittableBool>(canBeIgnited) });
            }
        }

        private void SelfIgnite(Flammable.Requirable.CommandRequestSender sender)
        {
            if (flammable == null)
            {
                sender.SendIgniteRequest(spatialComponent.SpatialEntityId, new Nothing());
                return;
            }
            Ignite();
        }

        public void SelfExtinguish(Flammable.Requirable.CommandRequestSender sender, bool canBeIgnited)
        {
            if (flammable == null)
            {
                sender.SendExtinguishRequest(spatialComponent.SpatialEntityId, new ExtinguishRequest(canBeIgnited));
                return;
            }
            Extinguish(canBeIgnited);
        }

        public void SelfSetCanBeIgnited(Flammable.Requirable.CommandRequestSender sender, bool canBeIgnited)
        {
            if (flammable == null)
            {
                sender.SendSetCanBeIgnitedRequest(spatialComponent.SpatialEntityId, new SetCanBeIgnitedRequest(canBeIgnited));
                return;
            }
            SetCanBeIgnited(canBeIgnited);
        }

        private void StartSpreadingFire()
        {
            spreadFireCoroutine = StartCoroutine(TimerUtils.WaitAndPerform(SimulationSettings.FireSpreadInterval, SpreadFire));
        }

        private void StopSpreadingFire()
        {
            if (spreadFireCoroutine != null)
            {
                StopCoroutine(spreadFireCoroutine);
            }
        }

        private void SpreadFire()
        {
            if (flammable == null)
            {
                return;
            }

            var count = Physics.OverlapSphereNonAlloc(transform.position, SimulationSettings.FireSpreadRadius, nearbyColliders);
            for (var i = 0; i < count; i++)
            {
                var otherFlammable = nearbyColliders[i].transform.GetComponentInParent<FlammableDataVisualizer>();
                if (otherFlammable != null && otherFlammable.canBeIgnited)
                {
                    // Cache local ignitable value, to avoid duplicated ignitions within 1 frame on an UnityWorker
                    otherFlammable.SetLocalCanBeIgnited(false);
                    otherFlammable.GetComponent<FlammableBehaviour>().SelfIgnite(flammableRequestSender);
                }
            }
        }

        private void SendIgniteUpdate()
        {
            var update = new Flammable.Update();
            update.IsOnFire = new Option<BlittableBool>(true);
            update.CanBeIgnited = new Option<BlittableBool>(false);
            flammable.Send(update);
        }

        private void SendExtinguishUpdate(bool canBeIgnited)
        {
            var update = new Flammable.Update();
            update.IsOnFire = new Option<BlittableBool>(false);
            update.CanBeIgnited = new Option<BlittableBool>(canBeIgnited);
            flammable.Send(update);
        }
    }
}
