using BepInEx.Configuration;
using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
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
using InfoScreen = GorillaInfoWatch.Models.InfoScreen;

namespace GorillaInfoWatch.Screens
{
    internal class RoomInspectorScreen : InfoScreen, IInRoomCallbacks
    {
        public override string Title => "Room Inspector";
        public override Type ReturnType => typeof(ScoreboardScreen);

        private const float RefreshRate = 2f;

        private float refreshTime;

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

            refreshTime = RefreshRate;

            RoomSystem.JoinedRoomEvent += OnRoomJoined;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnScreenUnload()
        {
            base.OnScreenUnload();

            RoomSystem.JoinedRoomEvent -= OnRoomJoined;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override InfoContent GetContent()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                ReturnScreen();
                return null;
            }

            LineBuilder lines = new();

            bool roomPrivacy = NetworkSystem.Instance.SessionIsPrivate;
            string privacyString = roomPrivacy ? "Private" : "Public";
            ConfigEntry<bool> privacyConfiguration = roomPrivacy ? Configuration.ShowPrivate : Configuration.ShowPublic;

            string roomName = privacyConfiguration.Value ? NetworkSystem.Instance.RoomName : $"-{privacyString.ToUpper()}-";
            lines.Append("Room ID: ").Append(roomName).Add(new Widget_Switch(privacyConfiguration.Value, value =>
            {
                privacyConfiguration.Value = value;
                SetContent();
            }));

            lines.Skip();

            // From here on out, the method assumes PUN (Photon Unity Networking) is being used
            // Honestly, I doubt a transition will be made any time soon with the volume of recent code that doesn't acknowledge Fusion
            lines.Append("Privacy: ").AppendLine(PhotonNetwork.CurrentRoom.IsOpen ? privacyString : "Closed");

            int playerCount = NetworkSystem.Instance.RoomPlayerCount;
            int maxPlayers = RoomSystem.UseRoomSizeOverride ? RoomSystem.GetCurrentRoomExpectedSize() : PhotonNetwork.CurrentRoom.MaxPlayers;
            lines.Append("Capacity: ").BeginColour(playerCount == maxPlayers ? ColourPalette.Green.GetInitialColour() : Color.white).Append(playerCount).Append(" out of ").Append(maxPlayers).EndColour().AppendLine();

            lines.Skip();

            if (NetworkSystem.Instance.MasterClient is NetPlayer host)
            {
                lines.Append("Host: ").Append(host.GetName().EnforcePlayerNameLength()).Add(new Widget_PushButton(() =>
                {
                    PlayerInspectorScreen.UserId = host.UserId;
                    LoadScreen<PlayerInspectorScreen>();
                })
                {
                    Colour = ColourPalette.Blue,
                    Symbol = Symbol.GetSharedSymbol(Symbols.Info)
                });
            }

            int ping = PhotonNetwork.NetworkingClient?.LoadBalancingPeer?.RoundTripTime ?? -1;
            lines.Append("Ping: ").Append(ping != -1 ? ping : "N/A").AppendLine(" ms");

            // https://doc.photonengine.com/pun/current/connection-and-authentication/regions
            string region = NetworkSystem.Instance.CurrentRegion.Replace("/*", "").ToLower();
            lines.Append("Region: ").AppendLine(region switch
            {
                "us" => "United States (East)", // Washington DC
                "usw" => "United States (West)", // San Jose
                "eu" => "Europe", // Amsterdam

                // Region codes below aren't used by Gorilla Tag as of commenting this
                // Server locations were only ever expanded in May 2021 (including US West and Europe) and never again
                "asia" => "Asia", // Singapore
                "au" => "Australia", // Sydney
                "cae" => "Canada", // Montreal
                "hk" => "Hong Kong", // Hong Kong
                "in" => "India", // Chennai
                "jp" => "Japan", // Tokyo
                "za" => "South Africa", // Johannesburg
                "sa" => "South America", // Sao Paulo
                "kr" => "South Korea", // Seoul
                "tr" => "Turkey", // Istanbul
                "uae" => "United Arab Emirates", // Dubai
                "ussc" => "United States (South Central)", // Dallas
                _ => region
            });

            lines.Skip();

            lines.Append("Game Mode: ").AppendLine(GameModeUtils.CurrentGamemode is Gamemode gamemode ? gamemode.DisplayName : GorillaScoreBoard.error.ToTitleCase());

            // "queueName" custom property is not set for ranked matches
            // Perhaps properties could be implemented here when they're set, they include "mmrTier" (Low/Medium/High) and "platform" (Quest/PC)
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("queueName", out object queueObject) && queueObject is string queueName)
            {
                bool isNativeQueue = ComputerQueuePatch.baseGameQueueNames.Contains(queueName);
                string queueTitle = isNativeQueue ? "Queue" : "Troop";
                lines.Append(queueTitle).Append(": ").AppendLine(isNativeQueue ? queueName.ToTitleCase() : queueName.ToUpper());
            }

            int participantCount = NetworkSystem.Instance.AllNetPlayers.Where(player => player != null && !player.IsNull).Count(GameMode.CanParticipate);
            lines.Append("Participation: ").BeginColour(participantCount == playerCount ? ColourPalette.Green.GetInitialColour() : ColourPalette.Red.GetInitialColour()).Append(participantCount).Append(" out of ").Append(playerCount).EndColour().AppendLine();

            return lines;
        }

        private void OnRoomJoined() => SetContent();

        private void OnRoomLeft() => LoadScreen<ScoreboardScreen>();

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
