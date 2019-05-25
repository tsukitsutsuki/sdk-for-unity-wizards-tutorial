using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Fire;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class TransformReceiverClientControllableAuthoritative : MonoBehaviour
    {
        [Require] private ClientAuthorityCheck.Requirable.Writer clientAuthorityCheck;
        [Require] private TransformComponent.Requirable.Reader transformComponent;
        [Require] private Flammable.Requirable.Reader flammable;

        private Vector3 targetVelocity;

        [SerializeField] private Rigidbody myRigidbody;

        private void Awake()
        {
            myRigidbody = gameObject.GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            transformComponent.OnTeleportEvent += OnTransformComponentUpdated;
        }

        private void OnDisable()
        {
        }

        private void OnTransformComponentUpdated(TeleportEvent update)
        {
            TeleportTo(update.TargetPosition.ToVector3());
        }

        private void TeleportTo(Vector3 position)
        {
            myRigidbody.velocity = Vector3.zero;
            myRigidbody.MovePosition(position);
        }

        public void SetTargetVelocity(Vector3 direction)
        {
            bool isOnFire = flammable != null && flammable.Data.IsOnFire;
            var movementSpeed = SimulationSettings.PlayerMovementSpeed * (isOnFire ? SimulationSettings.OnFireMovementSpeedIncreaseFactor : 1f);
            targetVelocity = direction * movementSpeed;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        public void MovePlayer()
        {
            var currentVelocity = myRigidbody.velocity;
            var velocityChange = targetVelocity - currentVelocity;
            if (ShouldMovePlayerAuthoritativeClient(velocityChange))
            {
                transform.LookAt(myRigidbody.position + targetVelocity);
                myRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        private bool ShouldMovePlayerAuthoritativeClient(Vector3 velocityChange)
        {
            return velocityChange.sqrMagnitude > Mathf.Epsilon && PlayerMovementCheatSafeguardPassedAuthoritativeClient(velocityChange);
        }

        private bool PlayerMovementCheatSafeguardPassedAuthoritativeClient(Vector3 velocityChange)
        {
            var result = velocityChange.sqrMagnitude < SimulationSettings.PlayerPositionUpdateMaxSqrDistance;
            if (!result)
            {
                Debug.LogError("Player movement cheat safeguard failed on Client. " + velocityChange.sqrMagnitude);
            }
            return result;
        }
    }
}
