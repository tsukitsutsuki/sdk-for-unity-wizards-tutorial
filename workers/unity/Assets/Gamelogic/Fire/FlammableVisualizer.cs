using Assets.Gamelogic.UI;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Fire
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class FlammableVisualizer : MonoBehaviour
    {
        [Require] private Flammable.Requirable.Reader flammable;

        private GameObject fireEffectInstance;
        private ParticleSystem fireEffectparticleSystem;

        private void CreateFireEffectInstance()
        {
            switch (flammable.Data.EffectType)
            {
                case FireEffectType.BIG:
                    fireEffectInstance = (GameObject)Instantiate(ResourceRegistry.FirePrefab, transform);
                    break;
                case FireEffectType.SMALL:
                    fireEffectInstance = (GameObject)Instantiate(ResourceRegistry.SmallFirePrefab, transform);
                    break;
            }
            fireEffectInstance.transform.localPosition = Vector3.zero;
            fireEffectparticleSystem = fireEffectInstance.GetComponent<ParticleSystem>();
        }

        private void OnEnable()
        {
            if (fireEffectInstance == null)
            {
                CreateFireEffectInstance();
            }
            flammable.ComponentUpdated += OnComponentUpdated;
            UpdateParticleSystem(flammable.Data.IsOnFire);
        }

        private void OnDisable()
        {
        }

        private void OnComponentUpdated(Flammable.Update update)
        {
            if(update.IsOnFire.HasValue)
            {
                UpdateParticleSystem(update.IsOnFire.Value);
            }
        }

        private void UpdateParticleSystem(bool enabled)
        {
            if(enabled)
            {
                fireEffectparticleSystem.Play();
            }
            else
            {
                fireEffectparticleSystem.Stop();
            }
        }
    }
}
