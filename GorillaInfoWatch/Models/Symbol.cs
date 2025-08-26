using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Enumerations;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Symbol
    {
        public Sprite Sprite;

        public Color Colour = Color.white;

        public Material Material;

        public Symbol(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Symbol(Symbols symbol)
        {
            if (Main.EnumToSprite.TryGetValue(symbol, out Sprite sprite))
            {
                Sprite = sprite;
            }
        }

        public static implicit operator Symbol(Symbols enumeration) => new(enumeration);

        public static implicit operator Symbol(Sprite sprite) => new(sprite);

        public static implicit operator Symbol(Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}