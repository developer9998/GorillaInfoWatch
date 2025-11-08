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
using Utilla.Models;
using Utilla.Utils;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    internal class ScoreboardScreen : InfoScreen
    {
        public override string Title => "Scoreboard";

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

            Events.OnRigNameUpdate += OnRigNameUpdate;

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.PlayerJoinedEvent += OnPlayerJoined;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
        }

        public override void OnScreenUnload()
        {
            base.OnScreenUnload();

            Events.OnRigNameUpdate -= OnRigNameUpdate;

            RoomSystem.JoinedRoomEvent -= OnRoomJoined;
            RoomSystem.PlayerJoinedEvent -= OnPlayerJoined;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
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
            int maxPlayers = (RoomSystem.UseRoomSizeOverride || NetworkSystem.Instance is not NetworkSystemPUN) ? RoomSystem.GetRoomSize(gameModeString) : PhotonNetwork.CurrentRoom.MaxPlayers;
            lines.Append(NetworkSystem.Instance.RoomPlayerCount).Append("/").Append(maxPlayers).Append(" Players").Add(new Widget_PushButton(() => LoadScreen<RoomInspectorScreen>())
            {
                Colour = ColourPalette.Blue,
                Symbol = Symbol.GetSharedSymbol(Symbols.Info)
            });

            lines.Append("Game Mode: ").AppendLine(GameModeUtils.CurrentGamemode is Gamemode gamemode ? gamemode.DisplayName : GorillaScoreBoard.error.ToTitleCase()).AppendLine();

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
                    Symbol = Symbol.GetSharedSymbol(Symbols.Info)
                }];

                lines.AppendColour(player.GetName().EnforcePlayerNameLength(), GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig) ? rig.playerText1.color : Color.white);
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

        private void OnRigNameUpdate(VRRig rig)
        {
            if (!rig.isLocal)
            {
                NetPlayer player = rig.Creator ?? rig.OwningNetPlayer;
                if (player == null || player.IsNull || player.InRoom) return;
            }

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