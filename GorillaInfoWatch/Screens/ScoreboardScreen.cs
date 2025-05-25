using System.Collections.Generic;
using System.Linq;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class ScoreboardScreen : WatchScreen
    {
        public override string Title => "Scoreboard";

        public override ScreenContent GetContent()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                Description = string.Empty;
                return new LineBuilder("<align=\"center\">Not in Room</align>");
            }

            Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";

            LineBuilder lines = new();

            var players_in_room = new List<NetPlayer>(NetworkSystem.Instance.AllNetPlayers);
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull) continue;
                lines.AddLine((GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) || !RigUtils.TryGetVRRig(player, out RigContainer container)) ? player.NickName.NormalizeName() : container.Rig.playerNameVisible, new WidgetPlayerSwatch(player), new WidgetPlayerSpeaker(player), new WidgetSpecialPlayerSwatch(player), new WidgetButton(TryInspectPlayer, player));
            }

            return lines;
        }

        public void TryInspectPlayer(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player && RigUtils.TryGetVRRig(player, out RigContainer playerRig))
            {
                PlayerInfoPage.Container = playerRig;
                SetScreen<PlayerInfoPage>();
            }
        }
    }
}
