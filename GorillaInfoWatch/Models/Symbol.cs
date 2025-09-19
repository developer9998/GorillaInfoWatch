using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models.Enumerations;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Symbol
    {
        public Sprite Sprite;

        public Color Colour = Color.white;

        public Material Material;

        private static readonly Dictionary<Symbols, Symbol> SymbolFromEnum = [];

        public Symbol(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Symbol(Symbols symbol)
        {
            if (Main.EnumToSprite.TryGetValue(symbol, out Sprite sprite)) Sprite = sprite;

            if (!SymbolFromEnum.ContainsKey(symbol)) SymbolFromEnum.Add(symbol, this);
        }

        public bool Equals(Symbol symbol)
        {
            return symbol.Sprite == Sprite && symbol.Colour == Colour && symbol.Material == Material;
        }

        public static implicit operator Symbol(Symbols enumeration) => SymbolFromEnum.TryGetValue(enumeration, out Symbol symbol) ? symbol : new(enumeration);

        public static implicit operator Symbol(Sprite sprite) => new(sprite);

        public static implicit operator Symbol(Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}