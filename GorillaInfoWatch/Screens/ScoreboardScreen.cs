using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class ScoreboardScreen : InfoWatchScreen
    {
        public override string Title => "Scoreboard";

        public HashSet<NetPlayer> includedPlayers = [];

        public override void OnScreenOpen()
        {
            base.OnScreenOpen();

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
            RoomSystem.PlayerJoinedEvent += OnPlayerJoined;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
        }

        public override void OnScreenClose()
        {
            base.OnScreenClose();

            RoomSystem.JoinedRoomEvent -= OnRoomJoined;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            RoomSystem.PlayerJoinedEvent -= OnPlayerJoined;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
        }

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            if (!NetworkSystem.Instance.InRoom)
            {
                Description = string.Empty;

                lines.Add("You aren't connected to a room!");
                lines.Skip();
                lines.Add("Join a room to view scoreboard.");

                return lines;
            }

            Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{NetworkSystem.Instance.config.MaxPlayerCount} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";

            var hashSet = new HashSet<NetPlayer>(NetworkSystem.Instance.AllNetPlayers);
            hashSet.UnionWith(includedPlayers);

            var players_in_room = hashSet.ToList();
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull) continue;
                lines.Add((GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) || !VRRigCache.Instance.TryGetVrrig(player, out RigContainer container)) ? player.NickName.SanitizeName() : container.Rig.playerNameVisible, new WidgetPlayerSwatch(player), new WidgetPlayerSpeaker(player), new WidgetSpecialPlayerSwatch(player), new PushButton(TryInspectPlayer, player));
            }

            return lines;
        }

        private void OnRoomJoined()
        {
            includedPlayers = [.. NetworkSystem.Instance.AllNetPlayers];
            SetContent();
        }

        private void OnRoomLeft()
        {
            includedPlayers.Clear();
            SetContent();
        }

        private void OnPlayerJoined(NetPlayer player)
        {
            if (includedPlayers.Add(player))
                SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (includedPlayers.Remove(player))
                SetContent();
        }

        public void TryInspectPlayer(object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player && VRRigCache.Instance.TryGetVrrig(player, out RigContainer playerRig))
            {
                PlayerInfoPage.Container = playerRig;
                SetScreen<PlayerInfoPage>();
            }
        }
    }
}
