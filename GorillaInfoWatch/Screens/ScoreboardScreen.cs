using BepInEx.Configuration;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilla.Models;
using Utilla.Utils;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class ScoreboardScreen : Models.Screen
    {
        public override string Title => "Scoreboard";

        public override void OnShow()
        {
            base.OnShow();

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
            RoomSystem.PlayerJoinedEvent += OnPlayerJoined;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
            Events.OnRigNameUpdate += OnRigNameUpdate;
        }

        public override void OnClose()
        {
            base.OnClose();

            RoomSystem.JoinedRoomEvent -= OnRoomJoined;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            RoomSystem.PlayerJoinedEvent -= OnPlayerJoined;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
            Events.OnRigNameUpdate -= OnRigNameUpdate;
        }

        public override ScreenLines GetContent()
        {
            LineBuilder lines = new();

            if (!NetworkSystem.Instance.InRoom)
            {
                lines.Add("Room: -NOT IN ROOM-").Skip();
                lines.Add("You must be in a room to use the scoreboard.");
                lines.Add("From there, you can view and manage players!");
                return lines;
            }

            ConfigEntry<bool> roomPrivacySetting = NetworkSystem.Instance.SessionIsPrivate ? Configuration.ShowPrivate : Configuration.ShowPublic;
            string roomNameProtected = roomPrivacySetting.Value ? NetworkSystem.Instance.RoomName : $"-{(NetworkSystem.Instance.SessionIsPrivate ? "PRIVATE" : "PUBLIC")}-";
            string gameModeString = NetworkSystem.Instance.GameModeString;

            lines.Add($"Room: {roomNameProtected} ({NetworkSystem.Instance.RoomPlayerCount} of {(/*PhotonNetwork.CurrentRoom is Room currentRoom ? (int)currentRoom.MaxPlayers : */RoomSystem.GetRoomSize(gameModeString))} players)", new Widget_Switch(roomPrivacySetting.Value, value =>
            {
                roomPrivacySetting.Value = value;
                if (!roomPrivacySetting.ConfigFile.SaveOnConfigSet) roomPrivacySetting.ConfigFile.Save();
                SetContent();
            })).Add($"Game Mode: {(GameModeUtils.CurrentGamemode is Gamemode gamemode ? gamemode.DisplayName : GorillaScoreBoard.error.ToTitleCase())}").Skip();

            NetPlayer[] players = NetworkSystem.Instance.AllNetPlayers;
            Array.Sort(players, (x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players)
            {
                if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom)) continue;

                List<Widget_Base> widgets = [new Widget_PlayerSwatch(player), new Widget_PlayerSpeaker(player), new Widget_PlayerIcon(player, 520, new Vector2(70, 80)), new Widget_PushButton(InspectPlayer, player)
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Symbols.Info
                }];

                lines.AppendColour(player.GetNameRef().EnforceLength(12), ColorUtility.ToHtmlStringRGB(GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig) ? rig.playerText1.color : Color.white));
                lines.Add(widgets);
            }

            return lines;
        }

        public void InspectPlayer(object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player)
            {
                PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                PlayerInfoPage.ActorNumber = player.ActorNumber;
                SetScreen<PlayerInfoPage>();
            }
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

        private void OnRigNameUpdate(VRRig targetRig)
        {
            if (targetRig.isLocal || targetRig.Creator is null || !targetRig.Creator.InRoom) return;
            SetContent();
        }
    }
}
