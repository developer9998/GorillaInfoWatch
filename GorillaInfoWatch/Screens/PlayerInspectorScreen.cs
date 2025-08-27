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
    public class PlayerInspectorScreen : Models.InfoScreen
    {
        public override string Title => "Player Inspector";

        public static string RoomName, UserId;

        private bool initialized;

        private Gradient muteColour, friendColour;

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

            if (!initialized)
            {
                initialized = true;
                muteColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), Color.red);
                friendColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), GFriends.m_clrFriend);
            }

            RoomSystem.LeftRoomEvent += OnRoomLeft;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
        }

        public override void OnScreenUnload()
        {
            base.OnScreenUnload();

            RoomName = null;
            UserId = null;

            RoomSystem.LeftRoomEvent -= OnRoomLeft;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
        }

        public override InfoContent GetContent()
        {
            if (!NetworkSystem.Instance.InRoom || NetworkSystem.Instance.RoomName != RoomName || !GetPlayer(UserId, out NetPlayer player) || !GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig))
            {
                LoadScreen<ScoreboardScreen>();
                return null;
            }

            // essential
            RigContainer rigContainer = rig.rigContainer ?? rig.GetComponent<RigContainer>();
            GetAccountInfoResult accountInfo = player.GetAccountInfo(result => SetContent());

            // setup
            LineBuilder lines = new();

            // name
            string nameRef = player.GetName();
            string playerName = nameRef.EnforcePlayerNameLength();
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
                Symbols.ModStick => "Moderator",
                Symbols.ForestGuideStick => "Forest Guide",
                Symbols.FingerPainter => "Finger Painter",
                _ => plrSignificance.Title
            });

            if (player.IsMasterClient) significance.Add("Host");
            if (!NetworkSystem.Instance.GetPlayerTutorialCompletion(player.ActorNumber)) significance.Add("Noob");

            lines.Skip().Append("Significance: ").AppendLine((significance is not null && significance.Count > 0) ? string.Join(", ", significance) : "None");
            lines.Append("Creation Date: ").AppendLine((accountInfo != null && accountInfo.AccountInfo?.TitleInfo?.Created is DateTime utcTime && utcTime.ToLocalTime() is DateTime localTime) ? $"{localTime.ToShortDateString()} at {localTime.ToShortTimeString()}" : ". . .");

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
                GorillaPlayerScoreboardLine[] lines = [.. GorillaScoreboardTotalUpdater.allScoreboards
                    .Select(scoreboard => scoreboard.lines)
                    .SelectMany(lines => lines)
                    .Where(line => line.linePlayer == player || line.rigContainer == container)];

                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; i++)
                    {
                        GorillaPlayerScoreboardLine line = lines[i];

                        if (i == 0)
                        {
                            line.muteButton.isOn = value;
                            line.PressButton(value, GorillaPlayerLineButton.ButtonType.Mute);
                            continue;
                        }

                        line.InitializeLine();
                    }
                }
                else Logging.Warning("No scoreboard lines detected");

                SetText();
            }
        }

        private void OnRoomLeft()
        {
            RoomName = null;
            UserId = null;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (!player.IsNull || player.UserId == UserId) OnRoomLeft();
        }

        public bool GetPlayer(string userId, out NetPlayer foundPlayer)
        {
            if (!string.IsNullOrEmpty(userId) && NetworkSystem.Instance != null && NetworkSystem.Instance.InRoom)
            {
                for (int i = 0; i < 2; i++)
                {
                    NetPlayer[] array = NetworkSystem.Instance.AllNetPlayers;

                    foreach (NetPlayer player in array)
                    {
                        if (!player.IsNull && player.UserId == userId)
                        {
                            foundPlayer = player;
                            return true;
                        }
                    }

                    if (i == 0) NetworkSystem.Instance.UpdatePlayers();
                }
            }

            foundPlayer = null;
            return false;
        }
    }
}
