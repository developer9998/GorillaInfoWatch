using System;
using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{

    public class PlayerInfoPage : WatchScreen
    {
        public override string Title => "Player Inspector";

        public static RigContainer Container;

        private bool IsValid => Container != null && Container.Creator is NetPlayer creator && creator.InRoom() && !creator.IsNull;

        private readonly Dictionary<string, GetAccountInfoResult> accountInfoCache = [];

        public override ScreenContent GetContent()
        {
            if (!IsValid)
            {
                SetScreen<ScoreboardScreen>();
                return null;
            }

            NetPlayer player = Container.Creator;
            VRRig rig = Container.Rig;

            bool hasAccountInfo = accountInfoCache.TryGetValue(player.UserId, out GetAccountInfoResult accountInfo);

            if (!hasAccountInfo)
            {
                PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
                {
                    PlayFabId = player.UserId
                }, result =>
                {
                    if (accountInfoCache.TryAdd(player.UserId, result))
                        SetContent();
                },
                error =>
                {
                    Logging.Fatal($"PlayFabClientAPI.GetAccountInfo ({player.UserId})");
                    Logging.Error(error.GenerateErrorReport());

                    Main.Instance.PlayErrorSound();
                });
            }

            PageBuilder pages = new();

            LineBuilder basicInfoLines = new();

            string playerName = (KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags) ? player.NickName : player.DefaultName).LimitLength(12);
            string playerNameSanitized = playerName.SanitizeName();

            basicInfoLines.AddLine($"{playerNameSanitized}{(playerNameSanitized != playerName ? $" ({playerName})" : "")}", new WidgetPlayerSwatch(player, 520f, 90, 90), new WidgetPlayerSpeaker(player, 620f, 100, 100), new WidgetSpecialPlayerSwatch(player, 520f, 80, 70));

            basicInfoLines.AddLine($"Creation Date: {(hasAccountInfo ? $"{accountInfo.AccountInfo.TitleInfo.Created.ToLongDateString()} at {accountInfo.AccountInfo.TitleInfo.Created.ToShortTimeString()}" : "Retrieving..")}");

            basicInfoLines.AddLines(1);
            basicInfoLines.AddLine(string.Format("Colour: [{0}, {1}, {2} | {3}, {4}, {5}]",
                Mathf.RoundToInt(rig.playerColor.r * 9f),
                Mathf.RoundToInt(rig.playerColor.g * 9f),
                Mathf.RoundToInt(rig.playerColor.b * 9f),
                Mathf.RoundToInt(rig.playerColor.r * 255f),
                Mathf.RoundToInt(rig.playerColor.r * 255f),
                Mathf.RoundToInt(rig.playerColor.r * 255f)));
            basicInfoLines.AddLine($"Points: {rig.currentQuestScore}");
            basicInfoLines.AddLine($"Voice Type: {(rig.localUseReplacementVoice || rig.remoteUseReplacementVoice ? "MONKE" : "HUMAN")}");
            basicInfoLines.AddLine($"Is Master Client: {(player.IsMasterClient ? "Yes" : "No")}");

            if (!player.IsLocal)
            {
                basicInfoLines.AddLines(1);
                basicInfoLines.AddLine(Container.Muted ? "Unmute" : "Mute", new Switch(OnMuteButtonClick, player)
                {
                    Value = Container.Muted
                });

                if (FriendLib.FriendCompatible)
                {
                    bool isFriend = FriendLib.IsFriend(player.UserId);
                    basicInfoLines.AddLine(FriendLib.IsFriend(player.UserId) ? "Remove Friend" : "Add Friend", new Switch(OnFriendButtonClick, player)
                    {
                        Value = isFriend
                    });
                }
            }

            pages.AddPage("General", basicInfoLines);

            if (!player.IsLocal)
            {
                LineBuilder compDataLines = new();

                compDataLines.AddLine($"FPS: {rig.fps}");
                compDataLines.AddLines(1);
                compDataLines.AddLine($"Turn Type: {rig.turnType}");
                compDataLines.AddLine($"Turn Factor: {rig.turnFactor}");

                pages.AddPage("Competitive", compDataLines);

                LineBuilder infoWatchLines = new();

                bool hasInfoWatch = rig.TryGetComponent(out NetworkedPlayer component) && component.HasWatch;
                infoWatchLines.AddLine($"Has GorillaInfoWatch: {(hasInfoWatch ? "Yes" : "No")}");

                if (hasInfoWatch)
                {
                    infoWatchLines.AddLines(1);
                    if (player.GetPlayerRef().CustomProperties.TryGetValue(Constants.NetworkVersionKey, out object versionObj) && versionObj is string version)
                        infoWatchLines.AddLine($"Version: {version}");
                    if (!string.IsNullOrEmpty(component.Watch.TimeZone))
                        infoWatchLines.AddLine($"Time Zone: {component.Watch.TimeZone}");
                }

                pages.AddPage("GorillaInfoWatch", infoWatchLines);
            }

            return pages;
        }

        private void OnFriendButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer netPlayer)
            {
                if (FriendLib.IsFriend(netPlayer.UserId))
                    FriendLib.RemoveFriend(netPlayer);
                else
                    FriendLib.AddFriend(netPlayer);

                SetText();
            }
        }

        private void OnMuteButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player && RigUtils.TryGetVRRig(player, out RigContainer container))
            {
                container.hasManualMute = true;
                container.Muted ^= true;

                try
                {
                    GorillaScoreboardTotalUpdater.allScoreboards.Select(scoreboard => scoreboard.lines)
                        .SelectMany(line => line)
                        .Where(line => line.linePlayer == player || line.rigContainer == container)
                        .ForEach(line =>
                        {
                            line.muteButton.isAutoOn = false;
                            line.muteButton.isOn = container.Muted;
                            line.muteButton.UpdateColor();
                        }
                    );
                }
                catch (Exception ex)
                {
                    Logging.Fatal($"Unable to update mute colour of lines for {player.NickName}");
                    Logging.Error(ex);
                }

                GorillaScoreboardTotalUpdater.ReportMute(player, container.Muted ? 1 : 0);
                Logging.Info($"Reported mute for {player.NickName}: {container.Muted}");

                PlayerPrefs.SetInt(player.UserId, container.Muted ? 1 : 0);
                PlayerPrefs.Save();

                SetText();
            }
        }
    }
}
