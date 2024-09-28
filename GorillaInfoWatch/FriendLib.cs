using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GorillaInfoWatch
{
    public static class FriendLib
    {
        private static List<PluginInfo> _pluginInfos = [];
        private static BaseUnityPlugin _gorillaFriendsPlugin;

        private static Color _friendColour, _verifiedColour, _recentlyPlayedColour;
        private static MethodInfo _isVerified, _isFriend, _isInFriendList, _hasPlayedWithUsRecently, _needToCheckRecently;
        private static List<string> _currentSessionFriends;

        public static void InitializeLib(Dictionary<string, PluginInfo> loadedPlugins)
        {
            _pluginInfos = [.. loadedPlugins.Values];
            PluginInfo friendPluginInfo = _pluginInfos.FirstOrDefault(plugin => plugin.Metadata.GUID == "net.rusjj.gorillafriends");

            if (friendPluginInfo != null)
            {
                _gorillaFriendsPlugin = friendPluginInfo.Instance;

                _friendColour = (Color)AccessTools.Property(_gorillaFriendsPlugin.GetType(), "m_clrFriend").GetValue(_gorillaFriendsPlugin);
                _verifiedColour = (Color)AccessTools.Property(_gorillaFriendsPlugin.GetType(), "m_clrVerified").GetValue(_gorillaFriendsPlugin);
                _recentlyPlayedColour = (Color)AccessTools.Property(_gorillaFriendsPlugin.GetType(), "m_clrPlayedRecently").GetValue(_gorillaFriendsPlugin);

                _isVerified = AccessTools.Method(_gorillaFriendsPlugin.GetType(), "IsVerified");
                _isFriend = AccessTools.Method(_gorillaFriendsPlugin.GetType(), "IsFriend");
                _isInFriendList = AccessTools.Method(_gorillaFriendsPlugin.GetType(), "IsInFriendList");
                _hasPlayedWithUsRecently = AccessTools.Method(_gorillaFriendsPlugin.GetType(), "IsVerified");
                _needToCheckRecently = AccessTools.Method(_gorillaFriendsPlugin.GetType(), "NeedToCheckRecently");
            }
        }

        public static bool FriendCompatible => _gorillaFriendsPlugin != null;

        public static Color FriendColour => _friendColour;

        public static Color VerifiedColour => _verifiedColour;

        public static Color RecentlyPlayedColour => _recentlyPlayedColour;

        public static bool IsVerified(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isVerified.Invoke(_gorillaFriendsPlugin, [userId]);
        }

        public static bool IsFriend(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isFriend.Invoke(_gorillaFriendsPlugin, [userId]);
        }

        public static bool IsInFriendList(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isInFriendList.Invoke(_gorillaFriendsPlugin, [userId]);
        }

        public static bool NeedToCheckRecently(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_needToCheckRecently.Invoke(_gorillaFriendsPlugin, [userId]);
        }

        public static byte HasPlayedWithUsRecently(string userId)
        {
            if (!FriendCompatible) return 0; // Never = 0
            return (byte)_hasPlayedWithUsRecently.Invoke(_gorillaFriendsPlugin, [userId]);
        }

        public static void AddFriend(NetPlayer player)
        {
            if (!FriendCompatible) return;

            _currentSessionFriends = (List<string>)AccessTools.Field(_gorillaFriendsPlugin.GetType(), "m_listCurrentSessionFriends").GetValue(_gorillaFriendsPlugin);
            if (!IsInFriendList(player.UserId))
            {
                _currentSessionFriends.Add(player.UserId);
                AccessTools.Field(_gorillaFriendsPlugin.GetType(), "m_listCurrentSessionFriends").SetValue(_gorillaFriendsPlugin, _currentSessionFriends);
                PlayerPrefs.SetInt(player.UserId + "_friend", 1);

                GorillaPlayerScoreboardLine line = GorillaScoreboardTotalUpdater.allScoreboardLines.First(line => line.linePlayer.ActorNumber == player.ActorNumber && line.gameObject.activeInHierarchy);

                line.playerName.color = FriendColour;
                line.playerVRRig.playerText1.color = FriendColour;
            }
        }

        public static void RemoveFriend(NetPlayer player)
        {
            if (!FriendCompatible) return;

            _currentSessionFriends = (List<string>)AccessTools.Field(_gorillaFriendsPlugin.GetType(), "m_listCurrentSessionFriends").GetValue(_gorillaFriendsPlugin);
            if (IsInFriendList(player.UserId))
            {
                _currentSessionFriends.Remove(player.UserId);
                AccessTools.Field(_gorillaFriendsPlugin.GetType(), "m_listCurrentSessionFriends").SetValue(_gorillaFriendsPlugin, _currentSessionFriends);
                PlayerPrefs.DeleteKey(player.UserId + "_friend");

                GorillaPlayerScoreboardLine line = GorillaScoreboardTotalUpdater.allScoreboardLines.First(line => line.linePlayer.ActorNumber == player.ActorNumber && line.gameObject.activeInHierarchy);

                if (IsVerified(player.UserId))
                {
                    line.playerName.color = VerifiedColour;
                    line.playerVRRig.playerText1.color = VerifiedColour;
                }
                else
                {
                    line.playerName.color = Color.white;
                    line.playerVRRig.playerText1.color = Color.white;
                }
            }
        }
    }
}
