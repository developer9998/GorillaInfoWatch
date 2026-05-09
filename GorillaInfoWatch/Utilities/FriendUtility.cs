using BepInEx;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GorillaInfoWatch.Utilities;

public static class FriendUtility
{
    public static bool HasFriendSupport => _friendPlugin?.Exists() ?? false;
    public static Color FriendColour => _friendColour;
    public static Color VerifiedColour => _verifiedColour;
    public static Color RecentlyPlayedColour => _recentlyPlayedColour;

    private static BaseUnityPlugin _friendPlugin;

    private static Color _friendColour, _verifiedColour, _recentlyPlayedColour;

    private static MethodInfo _isVerified, _isFriend, _isInFriendList, _hasPlayedWithUsRecently, _needToCheckRecently, _addFriend, _removeFriend;

    public static void ScanPlugins(Dictionary<string, PluginInfo> loadedPlugins)
    {
        Logging.Message("FriendUtility: ScanPlugins");

        if (loadedPlugins.TryGetValue(Constants.GorillaFriendsGUID, out PluginInfo plugin))
        {
            Initialize(plugin.Instance);
        }
    }

    private static void Initialize(BaseUnityPlugin plugin)
    {
        Logging.Message("FriendUtility: Initialize");

        _friendPlugin = plugin;

        Traverse traverse = Traverse.Create(plugin);
        _friendColour = traverse.Property("m_clrFriend").GetValue<Color>();
        _verifiedColour = traverse.Property("m_clrVerified").GetValue<Color>();
        _recentlyPlayedColour = traverse.Property("m_clrPlayedRecently").GetValue<Color>();

        Type type = plugin.GetType();
        _isVerified = AccessTools.Method(type, "IsVerified");
        _isFriend = AccessTools.Method(type, "IsFriend");
        _isInFriendList = AccessTools.Method(type, "IsInFriendList");
        _hasPlayedWithUsRecently = AccessTools.Method(type, "HasPlayedWithUsRecently");
        _needToCheckRecently = AccessTools.Method(type, "NeedToCheckRecently");
        _addFriend = AccessTools.Method(type, "AddFriend");
        _removeFriend = AccessTools.Method(type, "RemoveFriend");
    }

    public static bool IsVerified(string userId)
    {
        if (!HasFriendSupport) return false;
        return (bool)_isVerified.Invoke(_friendPlugin, [userId]);
    }

    public static bool IsFriend(string userId)
    {
        if (!HasFriendSupport) return false;
        return (bool)_isFriend.Invoke(_friendPlugin, [userId]);
    }

    public static bool IsInFriendList(string userId)
    {
        if (!HasFriendSupport) return false;
        return (bool)_isInFriendList.Invoke(_friendPlugin, [userId]);
    }

    public static void AddFriend(string userId)
    {
        _addFriend?.Invoke(_friendPlugin, [userId]);
    }

    public static void RemoveFriend(string userId)
    {
        _removeFriend?.Invoke(_friendPlugin, [userId]);
    }

    public static bool NeedToCheckRecently(string userId)
    {
        if (!HasFriendSupport) return false;
        return (bool)_needToCheckRecently.Invoke(_friendPlugin, [userId]);
    }

    public static (RecentlyPlayed recentlyPlayed, float value) HasPlayedWithUsRecently(string userId)
    {
        if (!HasFriendSupport) return (RecentlyPlayed.Never, 0);
        object result = _hasPlayedWithUsRecently.Invoke(_friendPlugin, [userId]);
        Traverse traverse = Traverse.Create(result);
        RecentlyPlayed recentlyPlayed = (RecentlyPlayed)traverse.Field("Item1").GetValue<byte>();
        float value = traverse.Field("Item2").GetValue<float>();
        return (recentlyPlayed, value);
    }

    public enum RecentlyPlayed : byte
    {
        Never = 0,
        Before = 1,
        Now = 2,
    }
}
