using GorillaInfoWatch.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Symbol
    {
        public Sprite Sprite;

        public Color Colour = Color.white;

        public Material Material;

        private static readonly Dictionary<Symbols, Symbol> _sharedSymbolCache = [];

        public Symbol(Sprite sprite)
        {
            Sprite = sprite;
        }

        public Symbol(Symbols symbol)
        {
            Sprite = symbol.AsSprite();
        }

        public static Symbol GetSharedSymbol(Symbols symbol)
        {
            if (!_sharedSymbolCache.ContainsKey(symbol)) _sharedSymbolCache.Add(symbol, new Symbol(symbol));
            return _sharedSymbolCache[symbol];
        }

        public bool Equals(Symbol symbol) => symbol.Sprite == Sprite && symbol.Colour == Colour && symbol.Material == Material;


        public static implicit operator Symbol(Sprite sprite) => new(sprite);

        public static implicit operator Symbol(Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }
}