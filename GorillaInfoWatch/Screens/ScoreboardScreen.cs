using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GorillaLibrary.GameModes.Utilities;
using GorillaLibrary.GameModes.Models;
using GorillaLibrary.Utilities;
using GorillaLibrary.Extensions;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    internal class ScoreboardScreen : InfoScreen
    {
        public override string Title => "Scoreboard";

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

            GorillaLibrary.Events.Room.OnRoomJoined.Subscribe(OnRoomJoined);
            GorillaLibrary.Events.Room.OnRoomLeft.Subscribe(OnRoomLeft);
            GorillaLibrary.Events.Player.OnPlayerEnteredRoom.Subscribe(OnPlayerJoined);
            GorillaLibrary.Events.Player.OnPlayerLeftRoom.Subscribe(OnPlayerLeft);
            GorillaLibrary.Events.Player.OnPlayerNameChanged.Subscribe(OnPlayerNameChanged);
        }

        public override void OnScreenUnload()
        {
            base.OnScreenUnload();

            GorillaLibrary.Events.Room.OnRoomJoined.Unsubscribe(OnRoomJoined);
            GorillaLibrary.Events.Room.OnRoomLeft.Unsubscribe(OnRoomLeft);
            GorillaLibrary.Events.Player.OnPlayerEnteredRoom.Unsubscribe(OnPlayerJoined);
            GorillaLibrary.Events.Player.OnPlayerLeftRoom.Unsubscribe(OnPlayerLeft);
            GorillaLibrary.Events.Player.OnPlayerNameChanged.Subscribe(OnPlayerNameChanged);
        }

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            if (!NetworkSystem.Instance.InRoom)
            {
                lines.Add("Room: -NOT IN ROOM-").Skip();
                lines.Add("You must be in a room to use the scoreboard.");
                lines.Add("From there, you can view and manage players!");
                return lines;
            }

            bool roomPrivacy = NetworkSystem.Instance.SessionIsPrivate;
            bool configuredPrivacy = (roomPrivacy ? Configuration.ShowPrivate : Configuration.ShowPublic).Value;
            lines.Append("In ").Append(configuredPrivacy ? $"Room {NetworkSystem.Instance.RoomName}" : $"{(roomPrivacy ? "Private" : "Public")} Room").Append(": ");

            string gameModeString = NetworkSystem.Instance.GameModeString;
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            lines.Append(NetworkSystem.Instance.RoomPlayerCount).Append("/").Append(maxPlayers).Append(" Players").Add(new Widget_PushButton(() => LoadScreen<RoomInspectorScreen>())
            {
                Colour = ColourPalette.Blue,
                Symbol = Content.Shared.Symbols["Info"]
            });

            lines.Append("Game Mode: ").AppendLine(GameModeUtility.CurrentGamemode is Gamemode gamemode ? gamemode.DisplayName : "Error").AppendLine();

            NetPlayer[] players = NetworkSystem.Instance.AllNetPlayers;
            Array.Sort(players, (x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players)
            {
                if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom)) continue;

                List<Widget_Base> widgets = [new Widget_Symbol()
                {
                    Alignment = new(47.5f),
                    ControllerType = typeof(WidgetController_PlayerSwatch),
                    ControllerParameters = [player]
                }, new Widget_Symbol()
                {
                    Alignment = new(47.5f),
                    ControllerType = typeof(WidgetController_PlayerIcon),
                    ControllerParameters = [player]
                }, new Widget_Symbol()
                {
                    Alignment = new(47.5f)
                    {
                        HorizontalOffset = 100
                    },
                    ControllerType = typeof(WidgetController_PlayerSpeaker),
                    ControllerParameters = [player]
                }, new Widget_PushButton(InspectPlayer, player)
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Content.Shared.Symbols["Info"]
                }];

                lines.AppendColour(player.GetName(), RigUtility.TryGetRig(player, out RigContainer rigContainer) ? rigContainer.Rig.playerText1.color : Color.white);
                lines.Add(widgets);
            }

            return lines;
        }

        public void InspectPlayer(object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player)
            {
                PlayerInspectorScreen.UserId = player.UserId;
                LoadScreen<PlayerInspectorScreen>();
            }
        }

        private void OnPlayerNameChanged(NetPlayer player, string name)
        {
            if (!player.IsLocal && (player == null || player.IsNull || player.InRoom)) return;

            SetContent();
        }

        private void OnRoomJoined() => SetContent();

        private void OnPlayerJoined(NetPlayer player)
        {
            if (player.IsLocal) return;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player) => SetContent();

        private void OnRoomLeft() => SetContent();
    }
}