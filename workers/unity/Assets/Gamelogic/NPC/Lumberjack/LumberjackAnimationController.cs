using System;
using Assets.Gamelogic.Core;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Npc;
using UnityEngine;

namespace Assets.Gamelogic.NPC.Lumberjack
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class LumberjackAnimationController : MonoBehaviour
    {
        [Require] private NPCLumberjack.Requirable.Reader npcLumberjack;
        [Require] private TargetNavigation.Requirable.Reader targetNavigation;
        [Require] private Flammable.Requirable.Reader flammable;
        [Require] private Inventory.Requirable.Reader inventory;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Animator anim;
        [SerializeField] private ParticleSystem WoodChipsAnimation;
        [SerializeField] private GameObject HandSocket;
        [SerializeField] private GameObject Axe;
        [SerializeField] private GameObject Log;
        [SerializeField] private AudioClip[] ChoppingSounds;
        [SerializeField] private int cachedResourcesCount;
        [SerializeField] private LumberjackFSMState.StateEnum cachedFsmState;

        private void Awake()
        {
            audioSource = gameObject.GetComponentIfUnassigned(audioSource);
            anim = gameObject.GetComponentIfUnassigned(anim);
        }

        private void OnEnable()
        {
            ResetAllLocalState();
            npcLumberjack.ComponentUpdated += OnNpcLumberjackComponentUpdate;
            targetNavigation.ComponentUpdated += NavigationUpdated;
            flammable.ComponentUpdated += FlammableUpdated;
            inventory.ComponentUpdated += OnInventoryUpdated;
            SetAnimationState(npcLumberjack.Data.CurrentState);
            SetForwardSpeed(TargetNavigationBehaviour.IsInTransit(targetNavigation));
        }

        private void OnDisable()
        {
        }

        public void OnAxeConnect()
        {
            WoodChipsAnimation.Play();
            PlayRandomSound(ChoppingSounds);
        }

        private void OnInventoryUpdated(Inventory.Update update)
        {
            if (update.Resources.HasValue)
            {
                cachedResourcesCount = update.Resources.Value;
                SetAnimationState(cachedFsmState);
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

        private void OnNpcLumberjackComponentUpdate(NPCLumberjack.Update newState)
        {
            if (newState.CurrentState.HasValue)
            {
                if (cachedFsmState != newState.CurrentState.Value)
                {
                    cachedFsmState = newState.CurrentState.Value;
                    SetAnimationState(cachedFsmState);
                }
            }
            
        }

        private void SetAnimationState(LumberjackFSMState.StateEnum currentState)
        {
            StopCurrentAnimation();
            switch (currentState)
            {
                case LumberjackFSMState.StateEnum.STOCKPILING:
                    anim.SetBool("Dropping", true);
                    break;
                case LumberjackFSMState.StateEnum.HARVESTING:
                    anim.SetBool("Chopping", true);
                    Axe.SetActive(true);
                    HandSocket.GetComponent<FixedJoint>().connectedBody = Axe.GetComponent<Rigidbody>();
                    break;
                case LumberjackFSMState.StateEnum.MOVING_TO_TARGET:
                    var isCarryingLog = cachedResourcesCount > 0;

                    anim.SetBool("Carrying", isCarryingLog);
                    anim.SetBool("Walking", !isCarryingLog);
                    Log.SetActive(isCarryingLog);

                    HandSocket.GetComponent<FixedJoint>().connectedBody = Log.GetComponent<Rigidbody>();
                    break;
            }
        }

        private void StopCurrentAnimation()
        {
            anim.SetBool("Dropping", false);
            anim.SetBool("Chopping", false);
            anim.SetBool("Carrying", false);
            anim.SetBool("Walking", false);
            Axe.SetActive(false);
            Log.SetActive(false);
        }

        private void PlayRandomSound(AudioClip[] choppingSounds)
        {
            int soundToPlay = new System.Random().Next(choppingSounds.Length);
            audioSource.PlayOneShot(choppingSounds[soundToPlay]);
        }

        private void ResetAllLocalState()
        {
            anim.SetBool("Dropping", false);
            anim.SetBool("Chopping", false);
            anim.SetBool("Carrying", false);
            anim.SetBool("Walking", false);
            anim.SetBool("OnFire", false);
            Axe.SetActive(false);
            Log.SetActive(false);
            cachedResourcesCount = inventory.Data.Resources;
            cachedFsmState = npcLumberjack.Data.CurrentState;
        }
    }
}
