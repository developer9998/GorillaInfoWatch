using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using UnityEngine;

namespace GorillaInfoWatch.Extensions;

public static class EnumExtensions
{
    public static AudioClip AsAudioClip(this Sounds sound) => Main.UnityObjectDictionary[sound] as AudioClip;

    public static Sprite AsSprite(this Symbols symbol) => Main.UnityObjectDictionary[symbol] as Sprite;
}
