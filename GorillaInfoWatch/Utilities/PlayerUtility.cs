using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using GorillaLibrary.Extensions;
using GorillaLibrary.Utilities;
using GorillaNetworking;
using HarmonyLib;
using System;
using System.Linq;

namespace GorillaInfoWatch.Utilities
{
    public static class PlayerUtility
    {
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

        public static SignificanceVisibility GetSignificanceVisibility(NetPlayer player) => CheckNetworkedPlayer(player, component => component.Consent, defaultValue: SignificanceVisibility.Figure, localValue: SignificanceManager.Instance?.Visibility ?? SignificanceVisibility.Figure);

        public static T CheckNetworkedPlayer<T>(NetPlayer player, Func<NetworkedPlayer, T> predicate, T defaultValue, T localValue)
        {
            if (player == null || player.IsNull) throw new ArgumentNullException(nameof(player));

            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (player.IsLocal) return localValue;

            if (NetworkSystem.Instance.InRoom && RigUtility.Rigs.TryGetValue(player, out RigContainer playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component))
                return predicate(component);

            return defaultValue;
        }

        public static void FindScoreBoardLines(NetPlayer player, Action<GorillaPlayerScoreboardLine, bool> result)
        {
            if (player == null || player.IsNull) throw new ArgumentNullException(nameof(player));

            if (result == null) throw new ArgumentNullException(nameof(result));

            GorillaPlayerScoreboardLine[] lines = [.. GorillaScoreboardTotalUpdater.allScoreboards
                .Select(scoreboard => scoreboard.lines).SelectMany(lines => lines)
                .Where(line => player.ActorNumber == line.playerActorNumber || player.Equals(line.linePlayer))];

            if (lines.Length > 0)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    result.Invoke(lines[i], i == 0);
                }
                return;
            }

            Logging.Warning($"No scoreboard lines detected for player {player.GetName()}");
        }

        public static bool HasActiveCosmetic(VRRig rig, string itemId)
        {
            if (!(bool)AccessTools.Field(typeof(VRRig), "initializedCosmetics").GetValue(rig)) return false;

            CosmeticsController.CosmeticSet cosmeticSet = rig.cosmeticSet;
            return cosmeticSet.items is CosmeticsController.CosmeticItem[] items && items.Where(item => !item.isNullItem).Any(item => item.itemName == itemId);
        }

        public static void MutePlayer(NetPlayer player, bool value)
        {
            FindScoreBoardLines(player, (scoreboardLine, isPrimaryLine) =>
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
            bool isFriend = FriendUtility.IsFriend(player.UserId);
            if (!value && isFriend) FriendUtility.RemoveFriend(player.UserId);
            else if (value && !isFriend) FriendUtility.AddFriend(player.UserId);
        }
    }
}