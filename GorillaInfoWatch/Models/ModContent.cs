using GorillaInfoWatch.Models.Significance;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Content", menuName = "Info Watch/ModContent", order = -1)]
    public class ModContent : ScriptableObject
    {
        public GameObject WatchPrefab;

        public GameObject MenuPrefab;

        public FigureSignificanceObject[] Figures;

        public ItemSignificanceObject[] Cosmetics;
    }
}