using Improbable.Gdk.GameObjectRepresentation;
using Improbable.Team;
using UnityEngine;

namespace Assets.Gamelogic.Team
{
    public class TeamAssignmentVisualizerUnityWorker : MonoBehaviour
    {
        [Require] private TeamAssignment.Requirable.Reader teamAssignmentReader;
        public uint TeamId
        {
            get
            {
                if (teamAssignmentReader == null) return uint.MaxValue;
                return teamAssignmentReader.Data.TeamId;
            }
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }
    }
}
