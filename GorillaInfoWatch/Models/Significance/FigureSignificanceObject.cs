using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    [CreateAssetMenu(fileName = "Figure", menuName = "GorillaInfoWatch/Figure (significance)", order = 0)]
    public class FigureSignificanceObject : ScriptableObject
    {
        public bool UseSymbol = true;

        public InfoWatchSymbol Symbol;

        public string[] UserIds;

#if PLUGIN
        public static explicit operator FigureSignificance(FigureSignificanceObject scriptableObject)
        {
            return new FigureSignificance(scriptableObject.Symbol, scriptableObject.UserIds);
        }
#endif
    }
}
