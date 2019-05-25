using Improbable.Building;
using Improbable.Gdk.GameObjectRepresentation;
using UnityEngine;

namespace Assets.Gamelogic.Building
{
    public class BarracksInfoVisualizer : MonoBehaviour
    {
        [Require] BarracksInfo.Requirable.Reader barracksInfo;
        public BarracksState BarracksState
        {
            get
            {
                if (barracksInfo == null) return BarracksState.UNDER_CONSTRUCTION;
                return barracksInfo.Data.BarracksState;
            }
        }
    }
}
