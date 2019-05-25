using Assets.Gamelogic.Core;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Npc;
using UnityEngine;

namespace Assets.Gamelogic.NPC.Wizard
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class WizardAnimationController : MonoBehaviour {

        [Require] private NPCWizard.Requirable.Reader npcWizard;
        [Require] private TargetNavigation.Requirable.Reader targetNavigation;
        [Require] private Flammable.Requirable.Reader flammable;

        [SerializeField] private Animator anim;
        public ParticleSystem CastAnim;

        private void Awake()
        {
            anim = gameObject.GetComponentIfUnassigned(anim);
        }

        private void OnEnable()
        {
            npcWizard.ComponentUpdated += StateUpdated;
            targetNavigation.ComponentUpdated += NavigationUpdated;
            flammable.ComponentUpdated += FlammableUpdated;
            ResetAllAnimationState();
            SetAnimationState(npcWizard.Data.CurrentState);
            SetForwardSpeed(TargetNavigationBehaviour.IsInTransit(targetNavigation));
        }

        private void OnDisable()
        {
        }

        public void StateUpdated(NPCWizard.Update stateUpdate)
        {
            if (stateUpdate.CurrentState.HasValue)
            {
                SetAnimationState(stateUpdate.CurrentState.Value);
            }
        }

        private void NavigationUpdated(TargetNavigation.Update navigationUpdate)
        {
            if (navigationUpdate.NavigationState.HasValue)
            {
                SetForwardSpeed(TargetNavigationBehaviour.IsInTransit(targetNavigation));
            }
        }

        private void FlammableUpdated(Flammable.Update update)
        {
            if (update.IsOnFire.HasValue)
            {
                anim.SetBool("OnFire", update.IsOnFire.Value);
            }
        }

        private void SetForwardSpeed(bool hasTarget)
        {
            if (hasTarget)
            {
                anim.SetFloat("ForwardSpeed", 1);
            }
            else
            {
                anim.SetFloat("ForwardSpeed", 0);
            }
        }


        private void SetAnimationState(WizardFSMState.StateEnum currentState)
        {
            if (currentState.Equals(WizardFSMState.StateEnum.ATTACKING_TARGET) ||
                currentState.Equals(WizardFSMState.StateEnum.DEFENDING_TARGET))
            {
                anim.SetTrigger("Casting");
                CastAnim.Play();
            }
            else
            {
                CastAnim.Stop();
            }
        }

        private void ResetAllAnimationState()
        {
            anim.SetBool("Casting", false);
            anim.SetBool("OnFire", false);
        }
    }
}
