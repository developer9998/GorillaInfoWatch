using BepInEx.Configuration;
using GorillaGameModes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine;
using Utilla.Models;
using Utilla.Utils;
using Screen = GorillaInfoWatch.Models.Screen;

namespace GorillaInfoWatch.Screens
{
    internal class RoomInspectorPage : Screen
    {
        public override string Title => "Room Inspector";
        public override Type ReturnType => typeof(ScoreboardScreen);

        public override void OnShow()
        {
            base.OnShow();
            RoomSystem.LeftRoomEvent += OnRoomLeft;
        }

        public override void OnClose()
        {
            base.OnClose();
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
        }

        public override ScreenLines GetContent()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                SetScreen<ScoreboardScreen>();
                return null;
            }

            LineBuilder lines = new();

            bool roomPrivacy = NetworkSystem.Instance.SessionIsPrivate;
            string privacyString = roomPrivacy ? "Private" : "Public";
            ConfigEntry<bool> privacyConfiguration = roomPrivacy ? Configuration.ShowPrivate : Configuration.ShowPublic;

            string roomName = privacyConfiguration.Value ? NetworkSystem.Instance.RoomName : $"-{privacyString.ToUpper()}-";
            lines.Append("Room Name: ").Append(roomName).Add(new Widget_Switch(privacyConfiguration.Value, value =>
            {
                privacyConfiguration.Value = value;
                SetContent();
            }));

            lines.Skip();

            bool isPUNInstance = NetworkSystem.Instance is NetworkSystemPUN;
            lines.Append("Privacy: ").AppendLine((isPUNInstance && !PhotonNetwork.CurrentRoom.IsOpen) ? "Closed" : privacyString);

            int playerCount = NetworkSystem.Instance.RoomPlayerCount;
            int maxPlayers = (!RoomSystem.UseRoomSizeOverride && isPUNInstance) ? PhotonNetwork.CurrentRoom.MaxPlayers : RoomSystem.GetRoomSize(NetworkSystem.Instance.GameModeString);
            lines.Append("Capacity: ").BeginColour(playerCount == maxPlayers ? Color.green : Color.white).Append(playerCount).Append(" out of ").Append(maxPlayers).EndColour().AppendLine();

            lines.Skip();

            lines.Append("Host: ").Append(NetworkSystem.Instance.MasterClient.GetName().EnforcePlayerNameLength()).Add(new Widget_PushButton(() =>
            {
                PlayerInspectorScreen.RoomName = NetworkSystem.Instance.RoomName;
                PlayerInspectorScreen.UserId = NetworkSystem.Instance.MasterClient.UserId;
                SetContent();
            })
            {
                Colour = ColourPalette.Blue,
                Symbol = Symbols.Info
            });

            lines.Append("Ping: ").Append(isPUNInstance ? PhotonNetwork.GetPing() : "N/A").AppendLine(" ms");

            // https://doc.photonengine.com/pun/current/connection-and-authentication/regions

            string region = PhotonNetwork.CloudRegion.Replace("/*", "").ToLower();
            lines.Append("Region: ").AppendLine(region switch
            {
                "us" => "United States (Eastern)",
                "usw" => "United States (Western)",
                "eu" => "Europe",
                /*
                "asia" => "Asia",
                "au" => "Australia",
                "cae" => "Canada",
                "cn" => "China",
                "hk" => "Hong Kong",
                "in" => "India",
                "jp" => "Japan",
                "za" => "South Africa",
                "sa" => "South America",
                "kr" => "South Korea",
                "tr" => "Turkey",
                "uae" => "United Arab Emirates",
                "ussc" => "United States (South Central)",
                */
                _ => region
            });

            lines.Skip();

            lines.Append("Game Mode: ").AppendLine(GameModeUtils.CurrentGamemode is Gamemode gamemode ? gamemode.DisplayName : GorillaScoreBoard.error.ToTitleCase());
            int participantCount = NetworkSystem.Instance.AllNetPlayers.Where(player => player != null && !player.IsNull).Count(GameMode.CanParticipate);
            lines.Append("Participating: ").BeginColour(participantCount == maxPlayers ? Color.green : Color.red).Append(participantCount).Append(" out of ").Append(maxPlayers).EndColour().AppendLine();

            return lines;
        }

        private void OnRoomLeft() => SetScreen<ScoreboardScreen>();
    }
}
