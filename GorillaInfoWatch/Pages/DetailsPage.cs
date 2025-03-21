using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Details")]
    public class DetailsScreen : WatchScreen
    {
        public override string Title => "Details";

        private bool show_sensitive_data = false;

        public override void OnScreenOpen()
        {
            SetContent();
        }

        public void SetContent()
        {
            StringBuilder str = new();
            str.Append("Name: ").AppendLine(GorillaComputer.instance.savedName ?? PlayerPrefs.GetString("playerName", "gorilla"));

            Color colour = GorillaTagger.Instance.offlineVRRig.playerColor;
            str.Append("Colour (0-9): ").AppendLine($"{Mathf.RoundToInt(colour.r * 9)}, {Mathf.RoundToInt(colour.g * 9)}, {Mathf.RoundToInt(colour.b * 9)}");
            str.Append("Colour (0-255): ").AppendLine($"{Mathf.RoundToInt(colour.r * 255)}, {Mathf.RoundToInt(colour.g * 255)}, {Mathf.RoundToInt(colour.b * 255)}");

            str.Append("Shiny Rocks: ").AppendLine(CosmeticsController.instance.CurrencyBalance.ToString());
            string creator_code = (string)AccessTools.Field(typeof(ATM_Manager), "currentCreatorCode").GetValue(ATM_Manager.instance);
            str.Append("Creator Code: ").AppendLine(creator_code == "" ? "None" : creator_code);
            str.Append("Points: ").AppendLine(ProgressionController.TotalPoints.ToString());

            bool troop_active = GorillaComputer.instance.troopQueueActive || PlayerPrefs.GetInt("troopQueueActive", 0) == 1;
            str.Append(troop_active ? "Troop: " : "Queue: ").AppendLine(troop_active ? (GorillaComputer.instance.troopName ?? PlayerPrefs.GetString("troopName", "")) : (GorillaComputer.instance.currentQueue ?? PlayerPrefs.GetString("currentQueue", "DEFAULT")));
            str.Append("Game Mode: ").Append(GorillaComputer.instance.currentGameMode.Value.Replace("_", " ").ToUpper());

            LineBuilder profile = str;

            profile.AddLine($"Show Sensitive Data: {(show_sensitive_data ? "Yes" : "No")}", new WidgetButton(WidgetButton.EButtonType.Switch, show_sensitive_data, ProcessSensitiveData));
            if (show_sensitive_data)
            {
                profile.AddLine($"User ID: {PlayFabAuthenticator.instance.GetPlayFabPlayerId()}");
            }

            str = new();
            str.Append("Playtime: ").AppendLine(TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(@"h\:mm\:ss"));
            str.Append("Tags: ").AppendLine(Metadata.GetItem("Tags", 0).ToString());
            LineBuilder session = str;

            str = new();
            str.Append("Players Online: ").AppendLine(NetworkSystem.Instance.GlobalPlayerCount().ToString());
            if (NetworkSystem.Instance.CurrentPhotonBackend == "PUN" || NetworkSystem.Instance is NetworkSystemPUN)
            {
                // START OF PUN CODE
                bool photon_server_connection = PhotonNetwork.NetworkingClient != null && PhotonNetwork.IsConnected && PhotonNetwork.Server != ServerConnection.NameServer;
                if (photon_server_connection && NetworkSystem.Instance.netState > NetSystemState.PingRecon && NetworkSystem.Instance.netState < NetSystemState.Disconnecting)
                {
                    // referenced variables/properties in "photon_server_connection" make up the CloudRegion property and PhotonNetwork.GetPing method result
                    string cloud_region = PhotonNetwork.CloudRegion.Replace("/*", "").ToLower();
                    str.Append("Region: ").AppendLine(cloud_region switch
                    {
                        "us" => "United States (East)",
                        "usw" => "United States (West)",
                        "eu" => "Europe",
                        _ => cloud_region
                    });
                    str.Append("Ping: ").AppendLine(PhotonNetwork.GetPing().ToString());
                }
                // END OF PUN CODE
            }
            str.Append("In Room: ").AppendLine(NetworkSystem.Instance.InRoom.ToString());

            if (NetworkSystem.Instance.InRoom)
            {
                str.AppendLine().Append("Room Name: ").AppendLine(NetworkSystem.Instance.RoomName);
                str.Append("Players In Room: ").Append(NetworkSystem.Instance.RoomPlayerCount).Append("/").AppendLine(PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString).ToString());
                str.Append("Game Mode: ").AppendLine((bool)GameMode.ActiveGameMode ? GameMode.ActiveGameMode.GameModeName() : "Null");
                VRRig master_rig = GorillaGameManager.StaticFindRigForPlayer(NetworkSystem.Instance.MasterClient);
                str.Append("Master: ").AppendLine(master_rig == null ? NetworkSystem.Instance.MasterClient.NickName : master_rig.playerNameVisible);
            }
            LineBuilder connection = str;

            str = new();
            str.Append("Platform: ").AppendLine(PlayFabAuthenticator.instance.platform.ToString());
            str.Append("Version: ").AppendLine(GorillaComputer.instance.version);
            str.Append("Build Date: ").AppendLine(GorillaComputer.instance.buildDate);
            LineBuilder platform = str;

            PageBuilder = new(("Profile", profile), ("Session", session), ("Connection", connection), ("Platform", platform));
        }

        public void ProcessSensitiveData(bool value, object[] args)
        {
            show_sensitive_data = !show_sensitive_data;
            SetContent();
            UpdateLines();
        }
    }
}
