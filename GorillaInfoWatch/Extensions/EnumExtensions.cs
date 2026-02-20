using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.UserInput;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Extensions;

public static class EnumExtensions
{
    private static Dictionary<UserInputBinding, char> _specialCharacterVariants = new()
    {
        { UserInputBinding.One, '!' },
        { UserInputBinding.Two, '@' },
        { UserInputBinding.Three, '#' },
        { UserInputBinding.Four, '$' },
        { UserInputBinding.Five, '%' },
        { UserInputBinding.Six, '^' },
        { UserInputBinding.Seven, '&' },
        { UserInputBinding.Eight, '*' },
        { UserInputBinding.Nine, '(' },
        { UserInputBinding.Zero, ')' },
        { UserInputBinding.Minus, '_' },
        { UserInputBinding.Equal, '+' },
        { UserInputBinding.LeftBracket, '{' },
        { UserInputBinding.RightBracket, '}' },
        { UserInputBinding.SemiColon, ':' },
        { UserInputBinding.Apostrophe, '\"' },
        { UserInputBinding.Comma, '<' },
        { UserInputBinding.Period, '>' },
        { UserInputBinding.Slash, '?' }
    };

    public static AudioClip AsAudioClip(this Sounds sound) => Main.UnityObjectDictionary[sound] as AudioClip;

    public static Sprite AsSprite(this Symbols symbol) => Main.UnityObjectDictionary[symbol] as Sprite;

    public static bool IsNumericKey(this UserInputBinding binding) => binding >= UserInputBinding.Zero && binding <= UserInputBinding.Nine;

    public static bool IsFunctionKey(this UserInputBinding binding) => binding >= UserInputBinding.Return && binding <= UserInputBinding.Shift;

    public static bool IsLetterKey(this UserInputBinding binding) => binding >= UserInputBinding.Q && binding <= UserInputBinding.M;

    public static bool TryParseNumber(this UserInputBinding binding, out int number)
    {
        if (IsNumericKey(binding))
        {
            number = (int)binding;
            return true;
        }

        number = -1;
        return false;
    }

    public static char ToChar(this UserInputBinding binding) => binding.TryParseNumber(out int number) ? char.Parse(number.ToString()) : binding switch
    {
        UserInputBinding.Space => ' ',
        UserInputBinding.Minus => '-',
        UserInputBinding.Equal => '=',
        UserInputBinding.LeftBracket => '[',
        UserInputBinding.RightBracket => ']',
        UserInputBinding.SemiColon => ';',
        UserInputBinding.Apostrophe => '\'',
        UserInputBinding.Comma => ',',
        UserInputBinding.Period => '.',
        UserInputBinding.Slash => '/',
        _ => Convert.ToChar(binding.ToString())
    };

    public static char ToSpecialChar(this UserInputBinding binding) => _specialCharacterVariants.TryGetValue(binding, out char value) ? value : ToChar(binding);
}
