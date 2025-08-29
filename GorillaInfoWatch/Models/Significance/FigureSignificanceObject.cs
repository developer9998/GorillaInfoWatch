using GorillaInfoWatch.Models.Enumerations;
using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    [CreateAssetMenu(fileName = "Figure", menuName = "Info Watch/Figure (significance)", order = 0)]
    public class FigureSignificanceObject : ScriptableObject
    {
        public Symbols Symbol;

        public string Description;

        public string[] UserIds;

#if PLUGIN
        public static explicit operator FigureSignificance(FigureSignificanceObject scriptableObject)
            => new(scriptableObject.name, scriptableObject.Symbol, scriptableObject.Description, scriptableObject.UserIds);
#endif
    }
}
