using GorillaInfoWatch.Models.Logic;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Data", menuName = "GorillaInfoWatch/InfoWatchData", order = -1)]
    public class InfoWatchData : ScriptableObject
    {
        public FigurePredicateObject[] Figures;

        public ItemPredicateObject[] Cosmetics;
    }
}
