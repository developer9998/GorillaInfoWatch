using System;
using UnityEngine;

namespace GorillaInfoWatch.Models;

public class Symbol
{
    public Sprite Sprite
    {
        get => _sprite;
        set
        {
            if (_isReadOnly) throw new InvalidOperationException();
            _sprite = value;
        }
    }

    public Color Colour
    {
        get => _colour;
        set
        {
            if (_isReadOnly) throw new InvalidOperationException();
            _colour = value;
        }
    }

    public Material Material
    {
        get => _material;
        set
        {
            if (_isReadOnly) throw new InvalidOperationException();
            _material = value;
        }
    }

    private Sprite _sprite;

    private Color _colour = Color.white;

    private Material _material;

    private readonly bool _isReadOnly;

    public Symbol(Sprite sprite)
    {
        _sprite = sprite;
    }

    public Symbol(SymbolObject symbol, bool isReadOnly = true)
    {
        _sprite = symbol?.Asset;
        _isReadOnly = isReadOnly;
    }

    public Symbol(Symbol symbol)
    {
        _sprite = symbol._sprite;
        _material = symbol._material;
        _colour = symbol._colour;
    }

    public bool Equals(Symbol symbol) => symbol._sprite == _sprite && symbol._colour == _colour && symbol._material == _material;


    public static implicit operator Symbol(Sprite sprite) => new(sprite);

    public static implicit operator Symbol(Texture2D texture) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
}