using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch
{
    public static class FriendLib
    {
        private const string GorillaFriendsGUID = "net.rusjj.gorillafriends";

        private static BaseUnityPlugin gorilla_friends;

        private static Color _friendColour, _verifiedColour, _recentlyPlayedColour;
        private static MethodInfo _isVerified, _isFriend, _isInFriendList, _hasPlayedWithUsRecently, _needToCheckRecently;
        private static List<string> _currentSessionFriends;

        public static void InitializeLib(Dictionary<string, PluginInfo> loadedPlugins)
        {
            if (loadedPlugins.TryGetValue(GorillaFriendsGUID, out PluginInfo pluginInfo))
            {
                Logging.Info($"Identified GorillaFriends Plugin");

                gorilla_friends = pluginInfo.Instance;

                _friendColour = (Color)AccessTools.Property(gorilla_friends.GetType(), "m_clrFriend").GetValue(gorilla_friends);
                _verifiedColour = (Color)AccessTools.Property(gorilla_friends.GetType(), "m_clrVerified").GetValue(gorilla_friends);
                _recentlyPlayedColour = (Color)AccessTools.Property(gorilla_friends.GetType(), "m_clrPlayedRecently").GetValue(gorilla_friends);

                _isVerified = AccessTools.Method(gorilla_friends.GetType(), "IsVerified");
                _isFriend = AccessTools.Method(gorilla_friends.GetType(), "IsFriend");
                _isInFriendList = AccessTools.Method(gorilla_friends.GetType(), "IsInFriendList");
                _hasPlayedWithUsRecently = AccessTools.Method(gorilla_friends.GetType(), "IsVerified");
                _needToCheckRecently = AccessTools.Method(gorilla_friends.GetType(), "NeedToCheckRecently");
                return;
            }

            Logging.Warning("Missing GorillaFriends Plugin");
        }

        public static bool FriendCompatible => gorilla_friends != null;

        public static Color FriendColour => _friendColour;

        public static Color VerifiedColour => _verifiedColour;

        public static Color RecentlyPlayedColour => _recentlyPlayedColour;

        public static bool IsVerified(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isVerified.Invoke(gorilla_friends, [userId]);
        }

        public static bool IsFriend(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isFriend.Invoke(gorilla_friends, [userId]);
        }

        public static bool IsInFriendList(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_isInFriendList.Invoke(gorilla_friends, [userId]);
        }

        public static bool NeedToCheckRecently(string userId)
        {
            if (!FriendCompatible) return false;
            return (bool)_needToCheckRecently.Invoke(gorilla_friends, [userId]);
        }

        public static byte HasPlayedWithUsRecently(string userId)
        {
            if (!FriendCompatible) return 0; // Never = 0
            return (byte)_hasPlayedWithUsRecently.Invoke(gorilla_friends, [userId]);
        }

        public static void AddFriend(NetPlayer player)
        {
            if (!FriendCompatible) return;

            _currentSessionFriends = (List<string>)AccessTools.Field(gorilla_friends.GetType(), "m_listCurrentSessionFriends").GetValue(gorilla_friends);
            if (!IsInFriendList(player.UserId))
            {
                _currentSessionFriends.Add(player.UserId);
                AccessTools.Field(gorilla_friends.GetType(), "m_listCurrentSessionFriends").SetValue(gorilla_friends, _currentSessionFriends);
                PlayerPrefs.SetInt(player.UserId + "_friend", 1);

                GorillaPlayerScoreboardLine line = GorillaScoreboardTotalUpdater.allScoreboardLines.First(line => line.linePlayer.ActorNumber == player.ActorNumber && line.gameObject.activeInHierarchy);

                line.playerName.color = FriendColour;
                line.playerVRRig.playerText1.color = FriendColour;
            }
        }

        public static void RemoveFriend(NetPlayer player)
        {
            if (!FriendCompatible) return;

            _currentSessionFriends = (List<string>)AccessTools.Field(gorilla_friends.GetType(), "m_listCurrentSessionFriends").GetValue(gorilla_friends);
            if (IsInFriendList(player.UserId))
            {
                _currentSessionFriends.Remove(player.UserId);
                AccessTools.Field(gorilla_friends.GetType(), "m_listCurrentSessionFriends").SetValue(gorilla_friends, _currentSessionFriends);
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
