using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using System;
using System.Linq;
using GFriends = GorillaFriends.Main;

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

        public static bool HasInfoWatch(NetPlayer player)
        {
            if (player == null || player.IsNull) throw new ArgumentNullException(nameof(player));

            if (player.IsLocal) return true;

            if (NetworkSystem.Instance.InRoom && VRRigCache.rigsInUse.TryGetValue(player, out RigContainer playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component))
                return component.HasInfoWatch;

            return false;
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
    }
}