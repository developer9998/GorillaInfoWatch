using System;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class DetailsScreen : WatchScreen
    {
        public override string Title => "Details";

        private bool show_sensitive_data = false;

        public override ScreenContent GetContent()
        {
            LineBuilder profileLines = new();

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            profileLines.AddLine($"Name: {localRig.playerNameVisible}");

            Color playerColour = localRig.playerColor;
            profileLines.AddLine(string.Format("Colour: [{0}, {1}, {2} / {3}, {4}, {5}]",
                Mathf.RoundToInt(playerColour.r * 9f),
                Mathf.RoundToInt(playerColour.g * 9f),
                Mathf.RoundToInt(playerColour.b * 9f),
                Mathf.RoundToInt(playerColour.r * 255f),
                Mathf.RoundToInt(playerColour.r * 255f),
                Mathf.RoundToInt(playerColour.r * 255f)));

            var accountInfo = NetworkSystem.Instance.GetLocalPlayer().GetAccountInfo(result => SetContent());
            profileLines.AddLine($"Creation Date: {(accountInfo is null || accountInfo.AccountInfo?.TitleInfo?.Created is not DateTime created ? "Loading.." : $"{created.ToShortDateString()} at {created.ToShortTimeString()}")}");

            LineBuilder economyLines = new();

            economyLines.AddLine($"Shiny Rocks: {CosmeticsController.instance.CurrencyBalance}");
            economyLines.AddLine($"+ 100 Shiny Rocks in: {TimeSpan.FromSeconds(CosmeticsController.instance.secondsUntilTomorrow):h\\:mm\\:ss}");
            economyLines.AddLine();

            var currentWornSet = CosmeticsController.instance.currentWornSet;
            for (int i = 0; i < currentWornSet.items.Length; i++)
            {
                var item = currentWornSet.items[i];
                if (item.isNullItem)
                    continue;
                string displayName = CosmeticsController.instance.GetItemDisplayName(item);
                CosmeticsController.CosmeticSlots slot = (CosmeticsController.CosmeticSlots)i;
                string displaySlot = slot switch
                {
                    CosmeticsController.CosmeticSlots.ArmLeft => "Left Arm",
                    CosmeticsController.CosmeticSlots.ArmRight => "Right Arm",
                    CosmeticsController.CosmeticSlots.HandLeft => "Left Paw",
                    CosmeticsController.CosmeticSlots.HandRight => "Right Paw",
                    CosmeticsController.CosmeticSlots.BackLeft => "Back Left",
                    CosmeticsController.CosmeticSlots.BackRight => "Back Right",
                    CosmeticsController.CosmeticSlots.TagEffect => "Tag Effect",
                    _ => slot.ToString()
                };
                economyLines.AddLine($"{displaySlot}: {displayName}");
            }

            LineBuilder progressionLines = new();

            progressionLines.AddLine($"Tutorial Completion: {NetworkSystem.Instance.GetMyTutorialCompletion()}");
            progressionLines.AddLine($"Total Points: {ProgressionController._gInstance.totalPointsRaw}");
            progressionLines.AddLine($"Unclaimed Points: {ProgressionController._gInstance.unclaimedPoints}");

            if (localRig.TryGetComponent(out GRPlayer grPlayer))
            {
                progressionLines.AddLine();
                var grProgression = grPlayer.CurrentProgression;
                int nextTier = GhostReactorProgression.TotalPointsForNextGrade(grProgression.redeemedPoints);
                progressionLines.AddLine($"Employment: {GhostReactorProgression.GetTitleNameAndGrade(grProgression.redeemedPoints)}");
                progressionLines.AddLine($"Earned: {grProgression.points} out of {nextTier} / {Mathf.FloorToInt((float)grProgression.points / nextTier * 100f)}%");
                progressionLines.AddLine($"Promotion: {(grProgression.points - nextTier) >= 0}");
            }

            /*
            StringBuilder str = new();
            str.Append("Name: ").AppendLine(GorillaComputer.instance.savedName ?? PlayerPrefs.GetString("playerName", "gorilla"));

            Color playerColor = GorillaTagger.Instance.offlineVRRig.playerColor;
            str.AppendLine(string.Format("Colour: [{0}, {1}, {2} | {3}, {4}, {5}]",
                Mathf.RoundToInt(playerColor.r * 9f),
                Mathf.RoundToInt(playerColor.g * 9f),
                Mathf.RoundToInt(playerColor.b * 9f),
                Mathf.RoundToInt(playerColor.r * 255f),
                Mathf.RoundToInt(playerColor.r * 255f),
                Mathf.RoundToInt(playerColor.r * 255f)));

            str.Append("Shiny Rocks: ").AppendLine(CosmeticsController.instance.CurrencyBalance.ToString());
            string creator_code = (string)AccessTools.Field(typeof(ATM_Manager), "currentCreatorCode").GetValue(ATM_Manager.instance);
            str.Append("Creator Code: ").AppendLine(creator_code == "" ? "None" : creator_code);
            str.Append("Points: ").AppendLine(ProgressionController.TotalPoints.ToString());

            bool troop_active = GorillaComputer.instance.troopQueueActive || PlayerPrefs.GetInt("troopQueueActive", 0) == 1;
            str.Append(troop_active ? "Troop: " : "Queue: ").AppendLine(troop_active ? (GorillaComputer.instance.troopName ?? PlayerPrefs.GetString("troopName", "")) : (GorillaComputer.instance.currentQueue ?? PlayerPrefs.GetString("currentQueue", "DEFAULT")));
            str.Append("Game Mode: ").Append(GorillaComputer.instance.currentGameMode.Value.Replace("_", " ").ToUpper());

            LineBuilder profile = str;

            profile.AddLine($"Show Sensitive Data: {(show_sensitive_data ? "Yes" : "No")}", new Switch(ProcessSensitiveData)
            {
                Value = show_sensitive_data
            });
            if (show_sensitive_data)
            {
                profile.AddLine($"User ID: {PlayFabAuthenticator.instance.GetPlayFabPlayerId()}");
            }

            str = new();
            str.Append("Playtime: ").AppendLine(TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(@"h\:mm\:ss"));
            str.Append("Tags: ").AppendLine(Singleton<DataHandler>.Instance.GetItem("Tags", 0).ToString());
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
            */

            return new PageBuilder(("Profile", profileLines), ("Economy", economyLines), ("Progression", progressionLines));
        }

        public void ProcessSensitiveData(bool value)
        {
            show_sensitive_data = value;
            SetText();
        }
    }
}
