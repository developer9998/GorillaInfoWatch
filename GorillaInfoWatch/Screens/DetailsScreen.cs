using GameObjectScheduling;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using GorillaNetworking;
using KID.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class DetailsScreen : Models.InfoScreen
    {
        public override string Title => "Details";

        public override InfoContent GetContent()
        {
            LineBuilder profileLines = new();

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            profileLines.Add($"Name: {localRig.playerNameVisible}");

            Color playerColour = localRig.playerColor;
            Color32 playerColour32 = playerColour;

            profileLines.Add(string.Format("Colour: [{0}, {1}, {2} / {3}, {4}, {5}]",
                Mathf.RoundToInt(playerColour.r * 9f),
                Mathf.RoundToInt(playerColour.g * 9f),
                Mathf.RoundToInt(playerColour.b * 9f),
                Mathf.RoundToInt(playerColour32.r),
                Mathf.RoundToInt(playerColour32.g),
                Mathf.RoundToInt(playerColour32.b)));

            var accountInfo = NetworkSystem.Instance.GetLocalPlayer().GetAccountInfo(result => SetContent());
            profileLines.Add($"Creation Date: {(accountInfo is null || accountInfo.AccountInfo?.TitleInfo?.Created is not DateTime created ? ". . ." : $"{created.ToShortDateString()} at {created.ToShortTimeString()}")}");
            profileLines.Skip();

            if (KIDManager.HasPermissionToUseFeature(EKIDFeatures.Voice_Chat))
            {
                profileLines.Add($"Voice: {GorillaComputer.instance.voiceChatOn switch
                {
                    "TRUE" => "Human",
                    "FALSE" => "Monke",
                    "OFF" => "Off",
                    _ => "N/A"
                }}", new Widget_Switch(GorillaComputer.instance.voiceChatOn == "TRUE", value =>
                {
                    GorillaComputer.instance.ProcessVoiceState(value ? GorillaKeyboardBindings.option1 : GorillaKeyboardBindings.option2);
                    GorillaComputer.instance.UpdateScreen();
                    SetContent();
                }));

                profileLines.Add($"Transmission: {GorillaComputer.instance.pttType.ToTitleCase()}", new Widget_SnapSlider(GorillaComputer.instance.pttType switch
                {
                    "PUSH TO TALK" => 1,
                    "PUSH TO MUTE" => 2,
                    _ => 0
                }, 0, 2, value =>
                {
                    if (Enum.TryParse(string.Concat("option", value + 1), out GorillaKeyboardBindings binding))
                    {
                        GorillaComputer.instance.ProcessMicState(binding);
                        GorillaComputer.instance.UpdateScreen();
                        SetContent();
                    }
                }));

                profileLines.Add($"Auto Mute: {GorillaComputer.instance.autoMuteType.ToTitleCase()}", new Widget_SnapSlider(GorillaComputer.instance.autoMuteType switch
                {
                    "AGGRESSIVE" => 0,
                    "MODERATE" => 1,
                    _ => 2
                }, 0, 2, value =>
                {
                    if (Enum.TryParse(string.Concat("option", value + 1), out GorillaKeyboardBindings binding))
                    {
                        GorillaComputer.instance.ProcessAutoMuteState(binding);
                        GorillaComputer.instance.UpdateScreen();
                        SetContent();
                    }
                }));
            }

            LineBuilder economyLines = new();

            economyLines.Add($"Shiny Rocks: {CosmeticsController.instance.CurrencyBalance}");

            TimeSpan whenShinyRocks = TimeSpan.FromSeconds(CosmeticsController.instance.secondsUntilTomorrow);
            economyLines.Add($"+ 100 Shiny Rocks in: {CountdownText.GetTimeDisplay(whenShinyRocks, "{0} {1}").ToLower()} ({(DateTime.Now + whenShinyRocks).ToShortTimeString()})");

            economyLines.Skip();

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
                economyLines.Add($"{displaySlot}: {displayName}");
            }

            LineBuilder safetyLines = new();

            safetyLines.Add($"K-ID Enabled: {KIDManager.KidEnabled}");

            AgeStatusType accountStatus = KIDManager.GetActiveAccountStatus();
            string serializedObject = JsonConvert.SerializeObject(accountStatus);
            string accountStatusName = string.IsNullOrEmpty(serializedObject) ? accountStatus.ToString().ToTitleCase() : serializedObject.ToTitleCase().Replace('-', ' ').Replace('_', ' ').TrimStart('"').TrimEnd('"');

            safetyLines.Add($"Account Status: {(KIDManager.InitialisationSuccessful ? accountStatusName : "Not Ready")}");
            safetyLines.Skip();

            foreach (EKIDFeatures feature in Enum.GetValues(typeof(EKIDFeatures)).Cast<EKIDFeatures>())
            {
                if (KIDManager.GetPermissionDataByFeature(feature) is Permission permission)
                {
                    string permissionName = feature switch
                    {
                        EKIDFeatures.Mods => "Virtual Stump",
                        EKIDFeatures.Groups => "Groups",
                        _ => permission.Name.ToTitleCase().Replace('-', ' ').Replace('_', ' ')
                    };
                    string management = permission.ManagedBy == Permission.ManagedByEnum.PROHIBITED ? "<color=red>Prohibited</color>" : string.Format("Managed by {0}", permission.ManagedBy.ToString().ToTitleCase());

                    safetyLines.Add($"{permissionName}: {management}", new Widget_Switch(permission.Enabled));
                }
            }

            LineBuilder progressionLines = new();

            progressionLines.Add($"Tutorial Completion: {(NetworkSystem.Instance.GetMyTutorialCompletion() ? "Complete" : "Incomplete")}");
            progressionLines.Add($"Total Points: {ProgressionController.TotalPoints}");
            progressionLines.Add($"Unclaimed Points: {ProgressionController._gInstance.unclaimedPoints}");

            if (localRig.TryGetComponent(out GRPlayer grPlayer))
            {
                progressionLines.Skip();

                GRPlayer.ProgressionData grProgression = grPlayer.CurrentProgression;
                int nextTier = GhostReactorProgression.TotalPointsForNextGrade(grProgression.redeemedPoints);
                progressionLines.Add($"Employment: {GhostReactorProgression.GetTitleNameAndGrade(grProgression.redeemedPoints)}");
                progressionLines.Add($"Earned: {grProgression.points} out of {nextTier} / {Mathf.FloorToInt((float)grProgression.points / nextTier * 100f)}%");
                progressionLines.Add($"Promotion: {(grProgression.points - nextTier) >= 0}");
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

            return new PageBuilder(("Profile", profileLines), ("Economy", economyLines), ("Safety", safetyLines), ("Progression", progressionLines));
        }
    }
}
