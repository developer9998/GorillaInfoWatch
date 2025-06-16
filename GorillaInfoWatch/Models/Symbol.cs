using GorillaInfoWatch.Behaviours;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class Symbol
    {
        public Sprite Sprite;

        public Color Colour = Color.white;

        public Color Color
        {
            get => Colour;
            set => Colour = value;
        }

        public Material Material;

        public Symbol(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Symbol(InfoWatchSymbol symbol)
        {
            if (Main.Instance is Main main && main.Sprites.TryGetValue(symbol, out Sprite sprite))
            {
                Sprite = sprite;
            }
        }

        public void Set(Image image)
        {
            image.sprite = Sprite;
            image.color = Colour;
            image.material = Material;
        }

        public static implicit operator Symbol(Sprite sprite)
        {
            return new Symbol(sprite);
        }

        public static implicit operator Symbol(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sprite;
        }
    }
}