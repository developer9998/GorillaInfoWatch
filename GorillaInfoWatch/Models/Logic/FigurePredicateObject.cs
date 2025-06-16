using UnityEngine;

namespace GorillaInfoWatch.Models.Logic
{
    [CreateAssetMenu(fileName = "Figure", menuName = "GorillaInfoWatch/FigurePredicate", order = 0)]
    public class FigurePredicateObject : ScriptableObject
    {
        public InfoWatchSymbol Symbol;

        public string[] UserIds;

#if PLUGIN
        public static explicit operator FigurePredicate(FigurePredicateObject scriptableObject)
        {
            return new FigurePredicate(scriptableObject.Symbol, scriptableObject.UserIds);
        }
#endif
    }
}
