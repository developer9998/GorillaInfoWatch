using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Utilities;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Screens
{
    internal class PlayerInspectorScreen : InfoScreen
    {
        public override string Title => "Player Inspector";

        public static string UserId;

        private bool initialized;

        private Gradient muteColour, friendColour;

        private readonly string describedFormat = "<line-height=45%>{0}<br><size=60%>{1}";

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

            if (!initialized)
            {
                initialized = true;

                muteColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), Color.red);
                friendColour = ColourPalette.CreatePalette(ColourPalette.Button.Evaluate(0), GFriends.m_clrFriend);
            }

            Events.OnRigNameUpdate += OnRigNameUpdate;
            RoomSystem.PlayerLeftEvent += OnPlayerLeft;
            RoomSystem.LeftRoomEvent += OnRoomLeft;
        }

        public override void OnScreenUnload()
        {
            base.OnScreenUnload();

            UserId = null;

            Events.OnRigNameUpdate -= OnRigNameUpdate;
            RoomSystem.PlayerLeftEvent -= OnPlayerLeft;
            RoomSystem.LeftRoomEvent -= OnRoomLeft;
        }

        public override InfoContent GetContent()
        {
            if (!NetworkSystem.Instance.InRoom || PlayerUtility.GetPlayer(UserId) is not NetPlayer player || !GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig))
            {
                ReturnScreen();
                return null;
            }

            RigContainer rigContainer = rig.rigContainer ?? rig.GetComponent<RigContainer>();
            GetAccountInfoResult accountInfo = player.GetAccountInfo(result => SetContent());

            PageBuilder inspectorPage = new();

            LineBuilder lines = new();

            string playerName = player.GetName();
            string normalizedName = playerName.EnforcePlayerNameLength();
            lines.AppendColour(normalizedName, rig.playerText1.color).Add(new Widget_Symbol()
            {
                Alignment = new(47.5f),
                ControllerType = typeof(WidgetController_PlayerSwatch),
                ControllerParameters = [player]
            }, new Widget_Symbol()
            {
                Alignment = new(47.5f),
                ControllerType = typeof(WidgetController_PlayerIcon),
                ControllerParameters = [player]
            }, new Widget_Symbol()
            {
                Alignment = new(47.5f)
                {
                    HorizontalOffset = 100
                },
                ControllerType = typeof(WidgetController_PlayerSpeaker),
                ControllerParameters = [player]
            });

            string displayName = playerName.SanitizeName();
            if (displayName != null && displayName.Length > 0 && normalizedName != displayName) lines.Append("Display Name: ").AppendLine(displayName);

            Color playerColour = rig.playerColor;
            float[] colourDigits = [playerColour.r, playerColour.g, playerColour.b];
            lines.AppendLine().Append("Colour: ").AppendColour(string.Join(' ', colourDigits.Select(digit => Mathf.Round(digit * 9f))), playerColour).AppendLine();

            lines.Append("Creation Date: ").AppendLine((accountInfo != null && accountInfo.AccountInfo?.TitleInfo?.Created is DateTime utcTime && utcTime.ToLocalTime() is DateTime localTime) ? $"{localTime.ToShortDateString()} at {localTime.ToShortTimeString()}" : ". . .");

            bool isLocal = player.IsLocal;

            bool isFriend = false;

            if (!isLocal)
            {
                lines.Skip();

                lines.Add(rigContainer.Muted ? "Unmute Player" : "Mute Player", new Widget_Switch(rigContainer.Muted, OnMuteButtonClick, player)
                {
                    Colour = muteColour
                });

                isFriend = GFriends.IsFriend(UserId);
                lines.Add(isFriend ? "Remove Friend" : "Add Friend", new Widget_Switch(isFriend, OnFriendButtonClick, player)
                {
                    Colour = friendColour
                });
            }

            #region Significance

            LineBuilder significanceLines = new();

            List<PlayerSignificance> list = [];

            bool isInFriendList = !isLocal && GFriends.IsInFriendList(player.UserId);

            if (SignificanceManager.Instance.Significance.TryGetValue(player, out PlayerSignificance[] plrSignificance))
            {
                plrSignificance = [.. plrSignificance];
                if (isInFriendList) plrSignificance[1] = Main.Significance_Friend;
                Array.ForEach(Array.FindAll(plrSignificance, item => item != null), list.Add);
            }

            if (!isLocal && !isInFriendList && !GFriends.NeedToCheckRecently(UserId) && GFriends.HasPlayedWithUsRecently(UserId) == GFriends.eRecentlyPlayed.Before)
                list.Add(Main.Significance_RecentlyPlayed);

            if (player.IsMasterClient)
                list.Add(Main.Significance_Master);

            if (list.Count > 0)
            {
                StringBuilder str = new();

                for (int i = 0; i < list.Count; i++)
                {
                    PlayerSignificance significance = list[i];

                    str.Append(i + 1).Append(". ");

                    if (string.IsNullOrEmpty(significance.Description)) str.Append(significance.Title);
                    else str.Append(string.Format(describedFormat, significance.Title, significance.Description.Replace(Constants.SignificancePlayerNameTag, normalizedName)));

                    significanceLines.Add(str.ToString(), widgets: significance.Symbol != Symbols.None ? [new Widget_Symbol(Symbol.GetSharedSymbol(significance.Symbol))
                    {
                        Alignment = WidgetAlignment.Left
                    }] : []);
                    str.Clear();

                    significanceLines.Skip();
                }
            }

            #endregion

            inspectorPage.Add(lines);
            if (significanceLines.Lines.Count > 0) inspectorPage.Add("Significance", significanceLines);

            return inspectorPage;
        }

        private void OnMuteButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player)
            {
                PlayerUtility.MutePlayer(player, value);
                SetText();
            }
        }

        private void OnFriendButtonClick(bool value, object[] args)
        {
            if (args.ElementAtOrDefault(0) is NetPlayer player)
            {
                PlayerUtility.FriendPlayer(player, value);
                SetContent();
            }
        }

        private void OnRigNameUpdate(VRRig targetRig)
        {
            NetPlayer player = targetRig.Creator ?? targetRig.OwningNetPlayer;
            if (player == null || player.IsNull || player.UserId != UserId) return;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (player.IsNull || player.UserId != UserId) return;
            OnRoomLeft();
        }

        private void OnRoomLeft()
        {
            UserId = null;
            SetContent();
        }
    }
}
