using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    public class DirectTransformVisualizer : MonoBehaviour
    {
        [Require] private Position.Requirable.Reader positionComponent;
        [Require] private TransformComponent.Requirable.Reader transformComponent;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            positionComponent.ComponentUpdated += VisualizePosition;
            transformComponent.ComponentUpdated += VisualizeTransform;
            SetPosition(positionComponent.Data.Coords);
            SetRotation(transformComponent.Data.Rotation);
        }

        private void OnDisable()
        {
        }

        private void VisualizePosition(Position.Update update)
        {
            if(update.Coords.HasValue)
            {
                SetPosition(update.Coords.Value);
            }
        }

        private void VisualizeTransform(TransformComponent.Update update)
        {
            if (update.Rotation.HasValue)
            {
                SetRotation(update.Rotation.Value);
            }
        }

        private void SetPosition(Coordinates position)
        {
            transform.position = (position.ToVector3() + origin);
        }

        private void SetRotation(uint rotation)
        {
            transform.rotation = Quaternion.Euler(0f, QuantizationUtils.DequantizeAngle(rotation), 0f);
        }
    }
}
