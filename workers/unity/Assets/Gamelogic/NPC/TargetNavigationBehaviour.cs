using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Fire;
using Improbable.Gdk.Core;
using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Npc;
using UnityEngine;

namespace Assets.Gamelogic.NPC
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TargetNavigationBehaviour : MonoBehaviour
    {
        [Require] private TargetNavigation.Requirable.Writer targetNavigation;
        [Require] private Flammable.Requirable.Reader flammable;

        [SerializeField] private Rigidbody myRigidbody;
        [SerializeField] private Transform myTransform;

        private Vector3 targetPosition = SimulationSettings.InvalidPosition;

        private SpatialOSComponent spatialOSComponent;
        private Vector3 origin;

        private void Awake()
        {
            myRigidbody = gameObject.GetComponentIfUnassigned(myRigidbody);
            myTransform = gameObject.GetComponentIfUnassigned(myTransform);
        }

        private void OnEnable()
        {
            spatialOSComponent = GetComponent<SpatialOSComponent>();
            origin = spatialOSComponent.Worker.Origin;
        }

        public static bool IsInTransit(TargetNavigation.Requirable.Reader targetNavigation)
        {
            return targetNavigation.Data.NavigationState != NavigationState.INACTIVE;
        }

        public void StartNavigation(Vector3 position, float interactionSqrDistance)
        {
            var flatPosition = position.FlattenVector();
            targetNavigation.Send(new TargetNavigation.Update()
            {
                NavigationState = NavigationState.POSITION,
                TargetPosition = flatPosition.ToVector3f(),
                TargetEntityId = new EntityId(),
                InteractionSqrDistance = interactionSqrDistance,
            });
        }

        public void StartNavigation(EntityId targetEntityId, float interactionSqrDistance)
        {
            targetNavigation.Send(new TargetNavigation.Update()
            {
                NavigationState = NavigationState.ENTITY,
                TargetPosition = SimulationSettings.InvalidPosition.ToVector3f(),
                TargetEntityId = targetEntityId,
                InteractionSqrDistance = interactionSqrDistance,
            });
        }

        public void StopNavigation()
        {
            if (IsInTransit(targetNavigation))
            {
                targetNavigation.Send(new TargetNavigation.Update()
                {
                    NavigationState = NavigationState.INACTIVE,
                    TargetPosition = SimulationSettings.InvalidPosition.ToVector3f(),
                    TargetEntityId = new EntityId(),
                    InteractionSqrDistance = 0f,
                });
            }
        }

        public void FinishNavigation(bool success)
        {
            StopNavigation();
            targetNavigation.SendNavigationFinished(new NavigationFinished(success));
        }

        private void Update()
        {
            TargetNavigationTick();
        }

        private void TargetNavigationTick()
        {
            if (!IsInTransit(targetNavigation))
            {
                return;
            }
            
            if (targetNavigation.Data.NavigationState == NavigationState.ENTITY)
            {
                var targetGameObject = NPCUtils.GetTargetGameObject(gameObject, targetNavigation.Data.TargetEntityId);
                if (targetGameObject != null)
                {
                    targetPosition = targetGameObject.transform.position.FlattenVector() + origin;
                }
                else
                {
                    targetPosition = SimulationSettings.InvalidPosition;
                }
            }

            if (targetNavigation.Data.NavigationState == NavigationState.POSITION)
            {
                targetPosition = targetNavigation.Data.TargetPosition.ToVector3() + origin;
            }

            if (MathUtils.CompareEqualityEpsilon(targetPosition, SimulationSettings.InvalidPosition))
            {
                FinishNavigation(false);
            }

            if (TargetPositionReached())
            {
                FinishNavigation(true);
            }

            MoveTowardsTargetPosition(Time.deltaTime);
        }

        private bool TargetPositionReached()
        {
            return MathUtils.SqrDistance(myTransform.position, targetPosition) < targetNavigation.Data.InteractionSqrDistance;
        }

        private void MoveTowardsTargetPosition(float deltaTime)
        {
            var movementSpeed = SimulationSettings.NPCMovementSpeed * (flammable.Data.IsOnFire ? SimulationSettings.OnFireMovementSpeedIncreaseFactor : 1f);
            var sqrDistanceToTarget = MathUtils.SqrDistance(targetPosition, myTransform.position);
            var distanceToTravel = movementSpeed * deltaTime;
            if ((distanceToTravel * distanceToTravel) < sqrDistanceToTarget)
            {
                myRigidbody.MovePosition(myTransform.position + (targetPosition - myTransform.position).normalized*distanceToTravel);
            }
            else
            {
                myRigidbody.MovePosition(targetPosition);
            }
            if (sqrDistanceToTarget > 0.01f)
            {
                myTransform.LookAt(targetPosition, Vector3.up);
            }
        }
    }
}
