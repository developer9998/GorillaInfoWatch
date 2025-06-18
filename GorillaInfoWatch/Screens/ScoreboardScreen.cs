using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class ScoreboardScreen : InfoWatchScreen
    {
        public override string Title => "Scoreboard";

        public HashSet<NetPlayer> includedPlayers = [];

        public override void OnShow()
        {
            base.OnShow();

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
            RoomSystem.PlayerJoinedEvent += OnPlayerJoined;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
        }

        public override void OnClose()
        {
            base.OnClose();

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
                lines.Add("<align=center>You are not in a Room!</align>");
                lines.Skip();
                lines.Add("Join a Room to access the <u>Scoreboard</u>.");

                return lines;
            }

            string gameModeString = NetworkSystem.Instance.GameModeString;
            string gameTypeName = GameMode.FindGameModeInString(gameModeString);

            lines.Add($"Room: {(NetworkSystem.Instance.SessionIsPrivate ? "-PRIVATE-" : NetworkSystem.Instance.RoomName)} ({NetworkSystem.Instance.RoomPlayerCount} of {RoomSystem.GetRoomSize(gameModeString)})");
            lines.Add($"Game Mode: {(GameMode.gameModeKeyByName.TryGetValue(gameTypeName, out int key) && GameMode.gameModeTable.TryGetValue(key, out GorillaGameManager manager) ? manager.GameModeName() : gameTypeName)}");
            lines.Skip();

            //Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{NetworkSystem.Instance.config.MaxPlayerCount} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";

            var players_in_room = NetworkSystem.Instance.AllNetPlayers.ToHashSet().ToList();
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull) continue;
                lines.Add(string.IsNullOrEmpty(player.SanitizedNickName) ? player.NickName.SanitizeName() : player.SanitizedNickName, new Widget_PlayerSymbol(player), new Widget_PlayerSpeaker(player), new Widget_SignificantSymbol(player), new Widget_PushButton(TryInspectPlayer, player));
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
