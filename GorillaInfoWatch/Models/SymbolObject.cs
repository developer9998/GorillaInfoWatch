using UnityEngine;

namespace GorillaInfoWatch.Models
{
    [CreateAssetMenu(fileName = "Sprite", menuName = "GorillaInfoWatch/Sprite")]
    public class SymbolObject : ScriptableObject
    {
        public string Label;

        public Sprite Asset;
    }
}
