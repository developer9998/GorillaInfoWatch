using BepInEx.Configuration;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class ScoreboardScreen : InfoWatchScreen
    {
        public override string Title => "Scoreboard";

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
                lines.Add("Room: -NOT IN ROOM-");
                lines.Skip();
                lines.Add("* Join a room to access the scoreboard");

                return lines;
            }

            ConfigEntry<bool> roomPrivacySetting = NetworkSystem.Instance.SessionIsPrivate ? Configuration.ShowPrivate : Configuration.ShowPublic;
            string roomNameProtected = roomPrivacySetting.Value ? NetworkSystem.Instance.RoomName : $"-{(NetworkSystem.Instance.SessionIsPrivate ? "PRIVATE" : "PUBLIC")}-";
            string gameModeString = NetworkSystem.Instance.GameModeString;

            lines.Add($"Room: {roomNameProtected} ({NetworkSystem.Instance.RoomPlayerCount} of {RoomSystem.GetRoomSize(gameModeString)})", new Widget_Switch(roomPrivacySetting.Value, (bool value) =>
            {
                roomPrivacySetting.Value = value;
                if (!roomPrivacySetting.ConfigFile.SaveOnConfigSet) roomPrivacySetting.ConfigFile.Save();
                SetContent();
            }));
            string gameTypeName = GameMode.FindGameModeInString(gameModeString);

            lines.Add($"Game Mode: {(GameMode.gameModeKeyByName.TryGetValue(gameTypeName, out int key) && GameMode.gameModeTable.TryGetValue(key, out GorillaGameManager manager) ? manager.GameModeName() : gameTypeName)}");
            lines.Skip();

            //Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{NetworkSystem.Instance.config.MaxPlayerCount} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";

            var players_in_room = NetworkSystem.Instance.AllNetPlayers.ToList();
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom)) continue;
                List<Widget_Base> widgets = [new Widget_PlayerSwatch(player), new Widget_PlayerSpeaker(player), new Widget_PlayerIcon(player, 520, new Vector2(70, 80))];
                if (!player.IsLocal) widgets.Add(new Widget_PushButton(InspectPlayer, player));
                lines.Add(string.IsNullOrEmpty(player.SanitizedNickName) ? player.NickName.SanitizeName() : player.SanitizedNickName, widgets);
            }

            return lines;
        }

        private void OnRoomJoined()
        {
            SetContent();
        }

        private void OnRoomLeft()
        {
            SetContent();
        }

        private void OnPlayerJoined(NetPlayer player)
        {
            if (player.IsLocal) return;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            SetContent();
        }

        public void InspectPlayer(object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player && VRRigCache.Instance.TryGetVrrig(player, out RigContainer playerRig))
            {
                PlayerInfoPage.Container = playerRig;
                SetScreen<PlayerInfoPage>();
            }
        }
    }
}
