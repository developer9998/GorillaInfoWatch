using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    [CreateAssetMenu(fileName = "Cosmetic", menuName = "GorillaInfoWatch/Item (significance)", order = 1)]
    public class ItemSignificanceObject : ScriptableObject
    {
        public InfoWatchSymbol Symbol;

        public string ItemId;

#if PLUGIN
        public static explicit operator ItemSignificance(ItemSignificanceObject scriptableObject)
        {
            return new ItemSignificance(scriptableObject.name, scriptableObject.Symbol, scriptableObject.ItemId);
        }
#endif
    }
}