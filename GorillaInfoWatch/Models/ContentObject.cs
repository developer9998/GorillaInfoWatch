using GorillaInfoWatch.Models.Significance;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Content", menuName = "GorillaInfoWatch/Content", order = -1)]
    public class ContentObject : ScriptableObject
    {
        public GameObject WatchPrefab;

        public GameObject MenuPrefab;

        public GameObject KeyboardPrefab;

        [Header("Significance")]

        public FigureSignificanceObject[] Figures;

        public ItemSignificanceObject[] Cosmetics;

        [Header("Symbols"), FormerlySerializedAs("SymbolArray")]

        public SymbolObject[] Symbols;
    }
}