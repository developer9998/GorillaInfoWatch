using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using System;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{

    public class PlayerInfoPage : WatchScreen
    {
        public override string Title => "Player Inspector";

        public static RigContainer Container;

        private bool IsValid => Container is not null && Container.Creator is NetPlayer creator && !creator.IsNull && (creator.IsLocal || (NetworkSystem.Instance.InRoom && NetworkSystem.Instance.GetPlayer(creator.ActorNumber) is not null));

        private int playerActorNum;

        private readonly Gradient muteColour, friendColour;

        public PlayerInfoPage()
        {
            muteColour = new Gradient();
            muteColour.SetKeys
            (
                [new GradientColorKey(new Color32(191, 188, 170, 255), 0), new GradientColorKey(new Color32(255, 0, 0, 255), 1)],
                [new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)]
            );

            friendColour = new Gradient();
            friendColour.SetKeys
            (
                [new GradientColorKey(new Color32(191, 188, 170, 255), 0), new GradientColorKey(FriendLib.FriendColour, 1)],
                [new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)]
            );
        }

        public override void OnScreenOpen()
        {
            base.OnScreenOpen();

            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
        }

        public override void OnScreenClose()
        {
            base.OnScreenClose();

            playerActorNum = -1;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
        }

        public override ScreenContent GetContent()
        {
            if (!IsValid)
            {
                SetScreen<ScoreboardScreen>();
                return null;
            }

            NetPlayer player = Container.Creator;

            if (playerActorNum == -1)
                playerActorNum = player.ActorNumber;

            VRRig rig = Container.Rig;

            var accountInfo = player.GetAccountInfo(result => SetContent());

            PageBuilder pages = new();

            LineBuilder basicInfoLines = new();

            bool hasPermission = GorillaComputer.instance.NametagsEnabled && KIDManager.HasPermissionToUseFeature(EKIDFeatures.Custom_Nametags);
            string playerName = hasPermission ? player.NickName : player.DefaultName;
            string playerNameSanitized = playerName.SanitizeName().LimitLength(12);

            basicInfoLines.AddLine($"{playerNameSanitized}{(playerNameSanitized != playerName ? $" ({playerName})" : "")}", new WidgetPlayerSwatch(player, 520f, 90, 90), new WidgetPlayerSpeaker(player, 620f, 100, 100), new WidgetSpecialPlayerSwatch(player, 520f, 80, 70));

            basicInfoLines.AddLine($"Creation Date: {(accountInfo is null || accountInfo.AccountInfo?.TitleInfo?.Created is not DateTime created ? "Loading.." : $"{created.ToShortDateString()} at {created.ToShortTimeString()}")}");

            basicInfoLines.AddLines(1);
            basicInfoLines.AddLine(string.Format("Colour: [{0}, {1}, {2} | {3}, {4}, {5}]",
                Mathf.RoundToInt(rig.playerColor.r * 9f),
                Mathf.RoundToInt(rig.playerColor.g * 9f),
                Mathf.RoundToInt(rig.playerColor.b * 9f),
                Mathf.RoundToInt(rig.playerColor.r * 255f),
                Mathf.RoundToInt(rig.playerColor.g * 255f),
                Mathf.RoundToInt(rig.playerColor.b * 255f)));
            basicInfoLines.AddLine($"Points: {rig.currentQuestScore}");
            basicInfoLines.AddLine($"Voice Type: {(rig.localUseReplacementVoice || rig.remoteUseReplacementVoice ? "MONKE" : "HUMAN")}");
            basicInfoLines.AddLine($"Is Master Client: {(player.IsMasterClient ? "Yes" : "No")}");

            if (!player.IsLocal)
            {
                basicInfoLines.AddLines(1);
                basicInfoLines.AddLine(Container.Muted ? "Unmute" : "Mute", new Switch(OnMuteButtonClick, player)
                {
                    Value = Container.Muted,
                    Colour = muteColour
                });

                if (FriendLib.FriendCompatible)
                {
                    bool isFriend = FriendLib.IsFriend(player.UserId);
                    basicInfoLines.AddLine(FriendLib.IsFriend(player.UserId) ? "Remove Friend" : "Add Friend", new Switch(OnFriendButtonClick, player)
                    {
                        Value = isFriend,
                        Colour = friendColour
                    });
                }
            }

            pages.AddPage("General", basicInfoLines);

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
                    GorillaScoreboardTotalUpdater.allScoreboards
                        .Select(scoreboard => scoreboard.lines)
                        .SelectMany(line => line)
                        .Where(line => line.linePlayer == player || line.rigContainer == container)
                        .ForEach(line =>
                        {
                            line.muteButton.isAutoOn = false;
                            line.muteButton.isOn = container.Muted;
                            line.muteButton.UpdateColor();
                        });
                }
                catch (Exception ex)
                {
                    Logging.Fatal($"Mute buttons could not be updated for {player.NickName}");
                    Logging.Error(ex);
                }

                GorillaScoreboardTotalUpdater.ReportMute(player, container.Muted ? 1 : 0);
                Logging.Info($"Reported mute for {player.NickName}: {container.Muted}");

                PlayerPrefs.SetInt(player.UserId, container.Muted ? 1 : 0);
                PlayerPrefs.Save();

                SetText();
            }
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (playerActorNum != -1 && player.ActorNumber == playerActorNum)
            {
                Container = null;
                SetContent();
            }
        }
    }
}
