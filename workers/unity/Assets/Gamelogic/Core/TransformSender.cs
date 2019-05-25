using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TransformSender : MonoBehaviour
    {
        [Require] private Position.Requirable.Writer positionComponent;
        [Require] private TransformComponent.Requirable.Writer transformComponent;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;
        private int fixedFramesSinceLastUpdate = 0;

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            transform.position = (positionComponent.Data.Coords.ToVector3() + origin);
        }

        public void TriggerTeleport(Vector3 position)
        {
            transform.position = position + origin;
            positionComponent.Send(new Position.Update() { Coords = position.ToCoordinates() });
            transformComponent.SendTeleportEvent(new TeleportEvent(position.ToCoordinates()));
        }

        private void FixedUpdate()
        {
            var newPosition = (transform.position - origin).ToCoordinates();
            var newRotation = QuantizationUtils.QuantizeAngle(transform.rotation.eulerAngles.y);
            fixedFramesSinceLastUpdate++;
            if ((PositionNeedsUpdate(newPosition) || RotationNeedsUpdate(newRotation)) && fixedFramesSinceLastUpdate > SimulationSettings.TransformUpdatesToSkipBetweenSends)
            {
                fixedFramesSinceLastUpdate = 0;
                positionComponent.Send(new Position.Update() { Coords = newPosition });
                transformComponent.Send(new TransformComponent.Update() { Rotation = newRotation });
            }
        }

        private bool PositionNeedsUpdate(Coordinates newPosition)
        {
            return !MathUtils.CompareEqualityEpsilon(newPosition, positionComponent.Data.Coords);
        }

        private bool RotationNeedsUpdate(float newRotation)
        {
            return !MathUtils.CompareEqualityEpsilon(newRotation, transformComponent.Data.Rotation);
        }
    }
}
