using BepInEx.Configuration;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Patches;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using UnityEngine;
using Utilla.Models;
using Utilla.Utils;
using Screen = GorillaInfoWatch.Models.Screen;

namespace GorillaInfoWatch.Screens
{
    internal class RoomInspectorPage : Screen, IInRoomCallbacks
    {
        public override string Title => "Room Inspector";
        public override Type ReturnType => typeof(ScoreboardScreen);

        private const float RefreshRate = 2.5f;

        private float refreshTime;

        public override void OnShow()
        {
            base.OnShow();

            refreshTime = RefreshRate;

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnClose()
        {
            base.OnClose();

            RoomSystem.JoinedRoomEvent -= OnRoomJoined;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            PhotonNetwork.RemoveCallbackTarget(this);
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

            lines.Append("Privacy: ").AppendLine(!PhotonNetwork.CurrentRoom.IsOpen ? "Closed" : privacyString);

            int playerCount = NetworkSystem.Instance.RoomPlayerCount;
            int maxPlayers = RoomSystem.UseRoomSizeOverride ? RoomSystem.GetRoomSize(NetworkSystem.Instance.GameModeString) : PhotonNetwork.CurrentRoom.MaxPlayers;
            lines.Append("Capacity: ").BeginColour(playerCount == maxPlayers ? Color.green : Color.white).Append(playerCount).Append(" out of ").Append(maxPlayers).EndColour().AppendLine();

            lines.Skip();

            if (NetworkSystem.Instance.MasterClient is NetPlayer host)
            {
                lines.Append("Host: ").Append(host.GetName().EnforcePlayerNameLength()).Add(new Widget_PushButton(() =>
                {
                    PlayerInspectorScreen.RoomName = NetworkSystem.Instance.RoomName;
                    PlayerInspectorScreen.UserId = host.UserId;
                    SetScreen<PlayerInspectorScreen>();
                })
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Symbols.Info
                });
            }

            int ping = -1;

            try
            {
                ping = PhotonNetwork.GetPing();
            }
            catch (Exception ex)
            {
                Logging.Fatal("Ping (via PhotonNetwork.GetPing) could not be retrieved");
                Logging.Error(ex);
            }

            lines.Append("Ping: ").Append(ping != -1 ? ping : "N/A").AppendLine(" ms");

            // https://doc.photonengine.com/pun/current/connection-and-authentication/regions

            string region = PhotonNetwork.CloudRegion.Replace("/*", "").ToLower();
            lines.Append("Region: ").AppendLine(region switch
            {
                "us" => "United States (East)",
                "usw" => "United States (West)",
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

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("queueName", out object queueObject) && queueObject is string queueName)
            {
                bool isNativeQueue = ComputerQueuePatch.baseGameQueueNames.Contains(queueName);
                string queueTitle = isNativeQueue ? "Queue" : "Troop";
                lines.Append(queueTitle).Append(": ").AppendLine(isNativeQueue ? queueName.ToTitleCase() : queueName.ToUpper());
            }

            int participantCount = NetworkSystem.Instance.AllNetPlayers.Where(player => player != null && !player.IsNull).Count(GameMode.CanParticipate);
            lines.Append("Participating: ").BeginColour(participantCount == playerCount ? Color.green : Color.red).Append(participantCount).Append(" out of ").Append(playerCount).EndColour().AppendLine();

            return lines;
        }

        private void OnRoomJoined() => SetContent();

        private void OnRoomLeft() => SetScreen<ScoreboardScreen>();

        public void OnPlayerEnteredRoom(Player newPlayer) => SetContent();

        public void OnPlayerLeftRoom(Player otherPlayer) => SetContent();

        public void OnMasterClientSwitched(Player newMasterClient) => SetContent();

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            if (!propertiesThatChanged.ContainsKey("gameMode") && !propertiesThatChanged.ContainsKey("queueName")) return;
            SetContent();
        }

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (!changedProps.ContainsKey("didTutorial")) return;
            SetContent();
        }

        public void Update()
        {
            refreshTime -= Time.deltaTime;
            if (refreshTime <= 0)
            {
                refreshTime = RefreshRate;
                SetContent();
            }
        }
    }
}
