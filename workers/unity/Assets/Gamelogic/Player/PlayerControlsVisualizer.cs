using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Player;
using UnityEngine;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PlayerControlsVisualizer : MonoBehaviour
    {
        [Require] private UnityWorkerAuthorityCheck.Requirable.Writer unityworkerAuthorityCheck;
        [Require] private PlayerInfo.Requirable.Reader playerInfo;
        [Require] private PlayerControls.Requirable.Reader playerControls;

        public Vector3 TargetPosition { get { return playerControls.Data.TargetPosition.ToVector3(); } }

        [SerializeField] private Rigidbody myRigidbody;
        [SerializeField] private TransformSender transformSender;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private void Awake()
        {
            transformSender = gameObject.GetComponentIfUnassigned(transformSender);
            myRigidbody = gameObject.GetComponentIfUnassigned(myRigidbody);
        }

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }
        
        private void MovePlayer()
        {
            var targetPositionWithOffset = TargetPosition + origin;
            if (ShouldMovePlayerUnityWorker(targetPositionWithOffset, myRigidbody.position))
            {
                if (PlayerMovementCheatSafeguardPassedUnityWorker(targetPositionWithOffset, myRigidbody.position))
                {
                    transform.LookAt(targetPositionWithOffset);
                    myRigidbody.MovePosition(targetPositionWithOffset);
                }
                else
                {
                    transformSender.TriggerTeleport(myRigidbody.position - origin);
                }
            }
        }

        private bool ShouldMovePlayerUnityWorker(Vector3 targetPosition, Vector3 currentPosition)
        {
            return playerInfo.Data.IsAlive && (targetPosition - currentPosition).FlattenVector().sqrMagnitude > SimulationSettings.PlayerPositionUpdateMinSqrDistance;
        }

        private bool PlayerMovementCheatSafeguardPassedUnityWorker(Vector3 targetPosition, Vector3 currentPosition)
        {
            return (targetPosition - currentPosition).sqrMagnitude < SimulationSettings.PlayerPositionUpdateMaxSqrDistance;
        }
    }
}
