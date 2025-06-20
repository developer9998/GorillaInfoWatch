using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    [CreateAssetMenu(fileName = "Figure", menuName = "GorillaInfoWatch/Figure (significance)", order = 0)]
    public class FigureSignificanceObject : ScriptableObject
    {
        public Sprite Sprite;

        public string[] UserIds;

#if PLUGIN
        public static explicit operator FigureSignificance(FigureSignificanceObject scriptableObject)
        {
            return new FigureSignificance(scriptableObject.Sprite, scriptableObject.UserIds);
        }
#endif
    }
}
