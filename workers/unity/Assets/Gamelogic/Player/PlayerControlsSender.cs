using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Player;
using UnityEngine;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerControlsSender : MonoBehaviour
    {
        [Require] private PlayerControls.Requirable.Writer playerControls;

        private Vector3 movementDirection = Vector3.zero;

        [SerializeField] private TransformReceiverClientControllableAuthoritative transformReceiverClientControllableAuthoritative;
        [SerializeField] private Rigidbody playerRigidbody;
         
        private void Awake()
        {
            playerRigidbody = gameObject.GetComponent<Rigidbody>();
            transformReceiverClientControllableAuthoritative = gameObject.GetComponentIfUnassigned(transformReceiverClientControllableAuthoritative);
        }

        private void FixedUpdate()
        {
            UpdatePlayerControls();
        }

        public void SetInputDirection(Vector3 inputDirection)
        {
            movementDirection = Vector3.ClampMagnitude((Camera.main.transform.rotation * inputDirection).FlattenVector(), 1f);
            transformReceiverClientControllableAuthoritative.SetTargetVelocity(movementDirection);
        }

        private void UpdatePlayerControls()
        { 
            var targetPosition = playerRigidbody.position;
            if (ShouldUpdatePlayerControls(targetPosition))
            {
                playerControls.Send(new PlayerControls.Update() { TargetPosition = targetPosition.ToCoordinates() });
            }
        }

        private bool ShouldUpdatePlayerControls(Vector3 newPosition)
        {
            return !MathUtils.CompareEqualityEpsilon(newPosition, playerControls.Data.TargetPosition.ToVector3());
        }
    }
}
