using UnityEngine;

namespace GorillaInfoWatch.Models.Logic
{
    [CreateAssetMenu(fileName = "Cosmetic", menuName = "GorillaInfoWatch/ItemPredicate", order = 1)]
    public class ItemPredicateObject : ScriptableObject
    {
        public InfoWatchSymbol Symbol;

        public string ItemId;

#if PLUGIN
        public static explicit operator ItemPredicate(ItemPredicateObject scriptableObject)
        {
            return new ItemPredicate(scriptableObject.Symbol, scriptableObject.ItemId);
        }
#endif
    }
}
