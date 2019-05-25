using Assets.Gamelogic.Utils;
using Improbable.Abilities;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Abilities
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class SpellsVisualizer : MonoBehaviour
    {
        [Require] private Spells.Requirable.Reader spells;

        private void OnEnable()
        {
            spells.OnSpellAnimationEvent += OnSpellAnimation;
        }

        private void OnDisable()
        {
        }

        private void OnSpellAnimation(SpellAnimationEvent update)
        {
            SpellsVisualizerPool.ShowSpellEffect(update.Position.ToVector3(), update.SpellType);
        }
    }
}
