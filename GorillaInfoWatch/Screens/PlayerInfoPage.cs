using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Screens
{
    public class PlayerInfoPage : Models.Screen
    {
        public override string Title => "Player Inspector";

        public static string RoomName;

        public static int ActorNumber;

        private bool IsValid => RoomName is not null && ActorNumber != -1 && NetworkSystem.Instance.InRoom && NetworkSystem.Instance.RoomName == RoomName && NetworkSystem.Instance.GetPlayer(ActorNumber) != null;

        private Gradient muteColour, friendColour;

        public override void OnShow()
        {
            base.OnShow();

            muteColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), Color.red);
            friendColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), GFriends.m_clrFriend);

            RoomSystem.LeftRoomEvent += OnRoomLeft;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
        }

        public override void OnClose()
        {
            base.OnClose();

            RoomName = null;
            ActorNumber = -1;

            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
        }

        public override ScreenLines GetContent()
        {
            if (!IsValid || !VRRigCache.Instance.TryGetVrrig(ActorNumber, out RigContainer rigContainer))
            {
                SetScreen<ScoreboardScreen>();
                return null;
            }

            // essential
            NetPlayer player = rigContainer.Creator;
            VRRig rig = rigContainer.Rig;
            GetAccountInfoResult accountInfo = player.GetAccountInfo(result => SetContent());

            // setup
            LineBuilder lines = new();

            // name
            string nameRef = player.GetName();
            string playerName = nameRef.EnforceLength(12);
            lines.AppendColour(playerName, rig.playerText1.color).Add(new Widget_PlayerSwatch(player, 520f, 90, 90), new Widget_PlayerSpeaker(player, 620f, 100, 100), new Widget_PlayerIcon(player, 520, new Vector2(70, 80)));
            string sanitizedName = nameRef.SanitizeName();
            if (sanitizedName != null && sanitizedName.Length > 0 && playerName != sanitizedName) lines.Append("Display Name: ").AppendLine(sanitizedName);

            // colour
            Color playerColour = rig.playerColor;
            float[] colourDigits = [playerColour.r, playerColour.g, playerColour.b];
            lines.Skip().Append("Colour: ").AppendColour(string.Join(' ', colourDigits.Select(digit => Mathf.Round(digit * 9f))), playerColour).AppendLine();

            // identity
            List<string> significance = [];

            if (GFriends.IsInFriendList(player.UserId)) significance.Add("Friend");
            if (PlayerHandler.Significance.TryGetValue(player, out PlayerSignificance plrSignificance)) significance.Add(plrSignificance.Symbol switch
            {
                Symbols.Dev => "dev9998",
                Symbols.Gizmo => "gizmogoat",
                Symbols.ModStick => "Moderator",
                Symbols.ForestGuideStick => "Forest Guide",
                Symbols.FingerPainter => "Finger Painter",
                Symbols.Illustrator => "Illustrator",
                _ => plrSignificance.Title
            });

            if (player.IsMasterClient) significance.Add("Master Client");
            if (!NetworkSystem.Instance.GetPlayerTutorialCompletion(ActorNumber)) significance.Add("Noob");

            lines.Skip().Append("Significance: ").AppendLine((significance is not null && significance.Count > 0) ? string.Join(", ", significance) : "None");
            lines.Append("Creation Date: ").AppendLine((accountInfo != null && accountInfo.AccountInfo?.TitleInfo?.Created is DateTime utcTime && utcTime.ToLocalTime() is DateTime localTime) ? $"{localTime.ToShortDateString()} at {localTime.ToShortTimeString()}" : "Loading..");

            if (player.IsLocal) return lines;

            lines.Skip();
            lines.Add(rigContainer.Muted ? "Unmute Player" : "Mute Player", new Widget_Switch(rigContainer.Muted, OnMuteButtonClick, player)
            {
                Colour = muteColour
            });

            bool isFriend = GFriends.IsFriend(player.UserId);
            lines.Add(GFriends.IsFriend(player.UserId) ? "Remove Friend" : "Add Friend", new Widget_Switch(isFriend, OnFriendButtonClick, player)
            {
                Colour = friendColour
            });

            return lines;
        }

        private void OnFriendButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer netPlayer)
            {
                if (GFriends.IsFriend(netPlayer.UserId)) GFriends.RemoveFriend(netPlayer.UserId);
                else GFriends.AddFriend(netPlayer.UserId);

                SetText();
            }
        }

        private void OnMuteButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player && VRRigCache.Instance.TryGetVrrig(player, out RigContainer container))
            {
                container.hasManualMute = true;
                container.Muted = value;

                GorillaScoreboardTotalUpdater.ReportMute(player, container.Muted ? 1 : 0);
                Logging.Info($"Reported mute for {player.NickName}: {container.Muted}");

                PlayerPrefs.SetInt(player.UserId, container.Muted ? 1 : 0);
                PlayerPrefs.Save();

                try
                {
                    GorillaScoreboardTotalUpdater.allScoreboards
                    .Select(scoreboard => scoreboard.lines)
                    .SelectMany(lines => lines)
                    .Where(line => line.linePlayer == player || line.rigContainer == container)
                    .ForEach(line => line.InitializeLine());
                }
                catch (Exception ex)
                {
                    Logging.Fatal($"Mute buttons could not be updated for {player.NickName}");
                    Logging.Error(ex);
                }

                SetText();
            }
        }

        private void OnRoomLeft()
        {
            ActorNumber = -1;
            RoomName = null;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (ActorNumber != -1 && player.ActorNumber == ActorNumber)
            {
                ActorNumber = -1;
                RoomName = null;
                SetContent();
            }
        }
    }
}
