using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class TransformReceiverClient : MonoBehaviour
    {
        [Require] private Position.Requirable.Reader positionComponent;
        [Require] private TransformComponent.Requirable.Reader transformComponent;

        private bool isRemote;

        [SerializeField] private Rigidbody myRigidbody;

        private bool? isAuthoritativePlayer;

        private void Awake()
        {
            myRigidbody = gameObject.GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            transformComponent.OnTeleportEvent += OnTransformComponentUpdated;
            if (IsNotAnAuthoritativePlayer())
            {
                SetUpRemoteTransform();
            }
        }

        private void OnDisable()
        {
            if (isRemote)
            {
                TearDownRemoveTransform();
            }
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

        private bool IsNotAnAuthoritativePlayer()
        {
            if (!isAuthoritativePlayer.HasValue)
            {
                isAuthoritativePlayer = Player.PlayerAuthority.IsAuth(gameObject);
            }
            return !isAuthoritativePlayer.Value;
        }

        private void Update()
        {
            if (IsNotAnAuthoritativePlayer())
            {
                myRigidbody.MovePosition(Vector3.Lerp(myRigidbody.position, positionComponent.Data.Coords.ToVector3(), 0.2f));
                myRigidbody.MoveRotation(Quaternion.Euler(0f, QuantizationUtils.DequantizeAngle(transformComponent.Data.Rotation), 0f));
            }
            else if(isRemote)
            {
                TearDownRemoveTransform();
            }
        }

        private void SetUpRemoteTransform()
        {
            isRemote = true;
            myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            myRigidbody.isKinematic = true;
        }

        private void TearDownRemoveTransform()
        {
            isRemote = false;
            myRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            myRigidbody.isKinematic = false;
        }
    }
}
