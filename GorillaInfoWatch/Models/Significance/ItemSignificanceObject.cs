using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    [CreateAssetMenu(fileName = "Cosmetic", menuName = "Info Watch/Item (significance)", order = 1)]
    public class ItemSignificanceObject : ScriptableObject
    {
        public Symbols Symbol;

        public string ItemId;

#if PLUGIN
        public static explicit operator ItemSignificance(ItemSignificanceObject scriptableObject)
            => new(scriptableObject.name, scriptableObject.Symbol, scriptableObject.ItemId);
#endif
    }
}