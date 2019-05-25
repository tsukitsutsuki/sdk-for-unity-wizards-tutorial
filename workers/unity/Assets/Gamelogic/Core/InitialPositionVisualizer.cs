using Assets.Gamelogic.Utils;
using Improbable;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Core
{
    public class InitialPositionVisualizer : MonoBehaviour
    {
        [Require] private Position.Requirable.Reader positionComponent;
        [Require] private TransformComponent.Requirable.Reader transformComponent;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private void OnEnable ()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;

            InitializeTransform();
        }

        private void InitializeTransform()
        {
            transform.position = positionComponent.Data.Coords.ToVector3() + origin;
            transform.rotation = Quaternion.Euler(0f, transformComponent.Data.Rotation, 0f);
        }
    }
}
