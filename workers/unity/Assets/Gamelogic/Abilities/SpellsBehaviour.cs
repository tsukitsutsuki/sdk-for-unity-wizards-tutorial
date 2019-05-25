using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Abilities;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;
using System.Collections.Generic;

namespace Assets.Gamelogic.Abilities
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class SpellsBehaviour : MonoBehaviour
    {
        [Require] private Spells.Requirable.Writer spells;
        [Require] private Spells.Requirable.CommandRequestHandler spellsRequestHandler;
        [Require] private Flammable.Requirable.CommandRequestSender flammableRequestSender;

        private Collider[] spellTargets;
        private Coroutine reduceCooldownsCoroutine;

        private void OnEnable()
        {
            spellTargets = new Collider[SimulationSettings.MaxSpellTargets];
            spellsRequestHandler.OnSpellCastRequestRequest += OnSpellCastRequest;
            reduceCooldownsCoroutine = StartCoroutine(TimerUtils.CallRepeatedly(1f, ReduceCooldowns));
        }

        private void OnDisable()
        {
            CancelExistingReduceCooldownsCoroutine();
        }

        private void OnSpellCastRequest(Spells.SpellCastRequest.RequestResponder request)
        {
            CastSpell(request.Request.Payload.SpellType, request.Request.Payload.Position.ToVector3());
        }

        private void CancelExistingReduceCooldownsCoroutine()
        {
            if (reduceCooldownsCoroutine != null)
            {
                StopCoroutine(reduceCooldownsCoroutine);
                reduceCooldownsCoroutine = null;
            }
        }

        public void CastSpell(SpellType spellType, Vector3 position)
        {
            if (!spells.Data.CanCastSpells || spells.Data.Cooldowns[spellType] > 0f)
            {
                return;
            }
            var targetCount = FindSpellTargetEntities(position);
            ApplySpellEffectOnTargets(spellType, targetCount);
            spells.SendSpellAnimationEvent(new SpellAnimationEvent(spellType, position.ToCoordinates()));
            SetSpellCooldown(spellType, SimulationSettings.SpellCooldown);
        }

        private int FindSpellTargetEntities(Vector3 position)
        {
            return Physics.OverlapCapsuleNonAlloc(position, position + Vector3.up * 10f, SimulationSettings.PlayerSpellAOEDiameter * 0.5f, spellTargets);
        }

        private void ApplySpellEffectOnTargets(SpellType spellType, int targetCount)
        {
            for (var spellTargetIndex = 0; spellTargetIndex < targetCount; spellTargetIndex++)
            {
                var spatialComponent = spellTargets[spellTargetIndex].gameObject.GetComponent<SpatialOSComponent>();
                if (spatialComponent == null) continue;

                var targetEntityId = spatialComponent.SpatialEntityId ;
                if (targetEntityId.IsValid())
                {
                    switch (spellType)
                    {
                        case SpellType.LIGHTNING:
                            flammableRequestSender.SendIgniteRequest(targetEntityId, new Nothing());
                            break;
                        case SpellType.RAIN:
                            flammableRequestSender.SendExtinguishRequest(targetEntityId, new ExtinguishRequest(true));
                            break;
                    }
                }
            }
        }

        private void SetSpellCooldown(SpellType spellType, float value)
        {
            var cooldowns = new Dictionary<SpellType, float>(spells.Data.Cooldowns);
            cooldowns[spellType] = value;
            spells.Send(new Spells.Update() { Cooldowns = cooldowns });
        }

        private void ReduceCooldowns()
        {
            var cooldowns = new Dictionary<SpellType, float>();
            var componentNeedsUpdate = false;
            foreach (var cooldown in spells.Data.Cooldowns)
            {
                if (cooldown.Value > 0f)
                {
                    cooldowns[cooldown.Key] = Mathf.Max(cooldown.Value - 1f, 0f);
                    componentNeedsUpdate = true;
                }
                else
                {
                    cooldowns[cooldown.Key] = cooldown.Value;
                }
            }
            if (componentNeedsUpdate)
            {
                spells.Send(new Spells.Update() { Cooldowns = cooldowns });
            }
        }
    }
}
