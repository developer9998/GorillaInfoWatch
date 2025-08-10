using GorillaInfoWatch.Models.Significance;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Content", menuName = "Info Watch/Content", order = -1)]
    public class Assets : ScriptableObject
    {
        public GameObject WatchPrefab;

        public GameObject MenuPrefab;

        public FigureSignificanceObject[] Figures;

        public ItemSignificanceObject[] Cosmetics;
    }
}
