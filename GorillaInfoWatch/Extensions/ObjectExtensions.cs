using UnityEngine;

namespace GorillaInfoWatch.Extensions
{
    internal static class ObjectExtensions
    {
        // https://github.com/developer9998/Bark/blob/master/Extensions/GameObjectExtensions.cs
        // "Comfort in community, obliterated"

        public static void Obliterate(this GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }

        public static void Obliterate(this Component component)
        {
            Object.Destroy(component);
        }

        public static bool Exists(this Object obj) => obj == null || !obj;

        public static bool Null(this Object obj) => !obj.Exists();
    }
}
