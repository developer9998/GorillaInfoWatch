using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class Symbol(Sprite sprite)
    {
        public Sprite Sprite = sprite;

        public Color Colour = Color.white;

        public Color Color
        {
            get => Colour;
            set => Colour = value;
        }

        public Material Material;

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

        public static explicit operator Symbol(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sprite;
        }
    }
}