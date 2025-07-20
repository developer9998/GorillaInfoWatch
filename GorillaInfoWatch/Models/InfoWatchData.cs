using GorillaInfoWatch.Models.Significance;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Data", menuName = "GorillaInfoWatch/InfoWatchData", order = -1)]
    public class InfoWatchData : ScriptableObject
    {
        public FigureSignificanceObject[] Figures;

        public ItemSignificanceObject[] Cosmetics;

        public GameObject WatchPrefab;

        public GameObject MenuPrefab;
    }
}
