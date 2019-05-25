using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Worker.CInterop;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TransformReceiverUnityWorker : MonoBehaviour
    {
        [Require] private Position.Requirable.Reader positionComponent;
        [Require] private TransformComponent.Requirable.Reader rotationComponent;

        [SerializeField] private Rigidbody myRigidbody;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private void Awake()
        {
            myRigidbody = gameObject.GetComponentIfUnassigned(myRigidbody);
        }

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            positionComponent.CoordsUpdated += OnPositionUpdated;
            OnPositionUpdated(positionComponent.Data.Coords);

            rotationComponent.RotationUpdated += OnRotationUpdated;
            OnRotationUpdated(rotationComponent.Data.Rotation);
        }

        private void OnDisable()
        {
        }

        private void OnPositionUpdated(Coordinates coords)
        {
            if (positionComponent.Authority == Authority.NotAuthoritative)
            {
                myRigidbody.MovePosition(coords.ToVector3() - origin);
            }
        }

        private void OnRotationUpdated(uint rotation)
        {
            if (rotationComponent.Authority == Authority.NotAuthoritative)
            {
                myRigidbody.MoveRotation(Quaternion.Euler(0f, QuantizationUtils.DequantizeAngle(rotation), 0f));
            }
        }
    }
}
