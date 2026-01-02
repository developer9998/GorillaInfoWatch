using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Utilities
{
    public static class PlayerUtility
    {
        private static readonly Dictionary<string, (GetAccountInfoResult accountInfo, DateTime cacheTime)> accountInfoCache = [];

        public static NetPlayer GetPlayer(string userId) => GetPlayer(player => player.UserId == userId);

        public static NetPlayer GetPlayer(Func<NetPlayer, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (NetworkSystem.Instance is NetworkSystem netSys && netSys.InRoom)
            {
                bool hasUpdatedPlayers = false;

                for (int i = 0; i < 2; i++)
                {
                    NetPlayer[] array = netSys.AllNetPlayers;

                    foreach (NetPlayer player in array)
                    {
                        if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom)) continue;

                        if (predicate(player)) return player;
                    }

                    if (!hasUpdatedPlayers)
                    {
                        hasUpdatedPlayers = true;
                        netSys.UpdatePlayers();
                    }
                }
            }

            return null;
        }

        public static bool HasInfoWatch(NetPlayer player) => CheckNetworkedPlayer(player, component => component.HasInfoWatch, defaultValue: false, localValue: true);

        public static SignificanceVisibility GetConsent(NetPlayer player) => CheckNetworkedPlayer(player, component => component.Consent, defaultValue: NetworkedPlayer.GetTemporaryConsent(player.UserId), localValue: SignificanceManager.Instance?.Visibility ?? SignificanceVisibility.None);

        public static T CheckNetworkedPlayer<T>(NetPlayer player, Func<NetworkedPlayer, T> predicate, T defaultValue, T localValue)
        {
            if (player == null || player.IsNull) throw new ArgumentNullException(nameof(player));

            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (player.IsLocal) return localValue;

            if (NetworkSystem.Instance.InRoom && VRRigCache.rigsInUse.TryGetValue(player, out RigContainer playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component))
                return predicate(component);

            return defaultValue;
        }

        public static void ProcessScoreboardLines(NetPlayer player, Action<GorillaPlayerScoreboardLine, bool> action)
        {
            if (player == null || player.IsNull) throw new ArgumentNullException(nameof(player));

            if (action == null) throw new ArgumentNullException(nameof(action));

            GorillaPlayerScoreboardLine[] lines = [.. GorillaScoreboardTotalUpdater.allScoreboards
                .Select(scoreboard => scoreboard.lines).SelectMany(lines => lines)
                .Where(line => player.ActorNumber == line.playerActorNumber || player.Equals(line.linePlayer))];

            if (lines.Length > 0)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    action.Invoke(lines[i], i == 0);
                }
                return;
            }

            Logging.Warning($"No scoreboard lines detected for player {player.GetName().EnforcePlayerNameLength()} (what the fuck map are you in??)");
        }

        public static bool HasActiveCosmetic(VRRig rig, string itemId)
        {
            if (!rig.InitializedCosmetics) return false;

            CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
            return cosmeticSet.items is CosmeticsController.CosmeticItem[] items && items.Where(item => !item.isNullItem).Any(item => item.itemName == itemId);
        }

        public static void MutePlayer(NetPlayer player, bool value)
        {
            ProcessScoreboardLines(player, (scoreboardLine, isPrimaryLine) =>
            {
                if (isPrimaryLine)
                {
                    scoreboardLine.muteButton.isOn = value;
                    scoreboardLine.PressButton(value, GorillaPlayerLineButton.ButtonType.Mute);
                    return;
                }
                scoreboardLine.InitializeLine();
            });
        }

        public static void FriendPlayer(NetPlayer player, bool value)
        {
            bool isFriend = GFriends.IsFriend(player.UserId);

            if (!value && isFriend) GFriends.RemoveFriend(player.UserId);
            else if (value && !isFriend) GFriends.AddFriend(player.UserId);
        }

        public static GetAccountInfoResult GetAccountInfo(string userId, Action<GetAccountInfoResult> onAccountInfoRecieved, double maxCacheTime = double.MaxValue)
        {
            if (accountInfoCache.ContainsKey(userId) && (DateTime.Now - accountInfoCache[userId].cacheTime).TotalMinutes < maxCacheTime)
                return accountInfoCache[userId].accountInfo;

            if (!PlayFabClientAPI.IsClientLoggedIn())
                throw new InvalidOperationException("PlayFab Client must be logged in to post the account info request");

            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
            {
                PlayFabId = userId
            }, accountInfo =>
            {
                if (accountInfoCache.ContainsKey(userId)) accountInfoCache[userId] = (accountInfo, DateTime.Now);
                else accountInfoCache.Add(userId, (accountInfo, DateTime.Now));
                onAccountInfoRecieved?.Invoke(accountInfo);
            }, error =>
            {
                Logging.Fatal($"PlayFabClientAPI.GetAccountInfo for {userId}");
                Logging.Error(error.GenerateErrorReport());
            });

            return null;
        }
    }
}