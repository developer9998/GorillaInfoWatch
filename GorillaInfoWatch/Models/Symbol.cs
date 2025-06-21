using GorillaInfoWatch.Behaviours;
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

        public Symbol(InfoWatchSymbol symbol)
        {
            if (Main.Sprites.TryGetValue(symbol, out Sprite sprite))
            {
                Sprite = sprite;
            }
        }

        public static implicit operator Symbol(Sprite sprite)
        {
            return new Symbol(sprite);
        }

        public static implicit operator Symbol(InfoWatchSymbol enumSymbol)
        {
            return new Symbol(enumSymbol);
        }

        public static implicit operator Symbol(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sprite;
        }
    }
}