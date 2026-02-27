using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Utilities;
using PlayFab.ClientModels;
using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    internal class PlayerInspectorScreen : InfoScreen
    {
        public override string Title => "Player Inspector";

        public static string UserId;

        private readonly Gradient _muteColour, _friendColour;

        private readonly string _descriptionFormat = "<line-height=45%>{0}<br><size=60%>{1}";

        public PlayerInspectorScreen()
        {
            _muteColour = ColourPalette.CreatePalette(ColourPalette.Button.GetInitialColour(), Color.red);
            _friendColour = ColourPalette.CreatePalette(ColourPalette.Button.GetInitialColour(), FriendUtility.FriendColour);
        }

        public override void OnScreenLoad()
        {
            base.OnScreenLoad();

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

            string playerName = player.GetPlayerName(false);
            string playerNameLimited = player.GetPlayerName(true);

            lines.AppendColour(playerNameLimited, rig.playerText1.color).Add(new Widget_Symbol()
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
            if (displayName != null && displayName.Length > 0 && playerNameLimited != displayName) lines.Append("Display Name: ").AppendLine(displayName);

            Color playerColour = rig.playerColor;
            float[] colourDigits = [playerColour.r, playerColour.g, playerColour.b];
            lines.AppendLine().Append("Colour: ").AppendColour(string.Join(' ', colourDigits.Select(digit => Mathf.Round(digit * 9f))), playerColour).AppendLine();

            lines.Append("Creation Date: ").AppendLine((accountInfo != null && accountInfo.AccountInfo?.TitleInfo?.Created is DateTime utcTime && utcTime.ToLocalTime() is DateTime localTime) ? $"{localTime:d} at {localTime:t}" : ". . .");

            bool isLocal = player.IsLocal;

            bool isFriend = false;

            if (isLocal) goto Significance;

            lines.Skip();

            lines.Add(rigContainer.Muted ? "Unmute Player" : "Mute Player", new Widget_Switch(rigContainer.Muted, OnMuteButtonClick, player)
            {
                Colour = _muteColour
            });

            if (FriendUtility.HasFriendSupport)
            {
                isFriend = FriendUtility.IsFriend(UserId);
                lines.Add(isFriend ? "Remove Friend" : "Add Friend", new Widget_Switch(isFriend, OnFriendButtonClick, player)
                {
                    Colour = _friendColour
                });
            }

            #region Significance

        Significance:

            LineBuilder significanceLines = new();

            var array = Enumerable.Repeat<PlayerSignificance>(null, Enum.GetValues(typeof(SignificanceLayer)).Length).ToArray();

            if (SignificanceManager.Instance.GetSignificance(player, out PlayerSignificance[] playerSignificance))
            {
                for (int i = 0; i < playerSignificance.Length; i++)
                {
                    array[i] = playerSignificance[i];
                }
            }

            bool isInFriendList = !isLocal && FriendUtility.IsInFriendList(player.UserId);
            if (isInFriendList) array[(int)SignificanceLayer.Friend] = SignificanceManager.Significance_Friend;

            if (player.IsMasterClient) array[(int)SignificanceLayer.Master] = SignificanceManager.Significance_Master;

            if (!isLocal && !FriendUtility.NeedToCheckRecently(UserId) && FriendUtility.HasPlayedWithUsRecently(UserId) is var hasRecentlyPlayed && hasRecentlyPlayed.recentlyPlayed == FriendUtility.RecentlyPlayed.Before)
                array[(int)SignificanceLayer.RecentlyPlayed] = SignificanceManager.Significance_RecentlyPlayed;

            if (array.Length > 0)
            {
                int number = 1;

                StringBuilder str = new();

                for (int i = 0; i < array.Length; i++)
                {
                    PlayerSignificance significance = array[i];

                    if (significance == null) continue;

                    str.Append(number).Append(". ");
                    number++;

                    if (string.IsNullOrEmpty(significance.Description)) str.Append(significance.Title);
                    else str.Append(string.Format(_descriptionFormat, significance.Title, string.Format(significance.Description, playerNameLimited)));

                    significanceLines.Add(str.ToString(), widgets: significance.Symbol != null ? [new Widget_Symbol(significance.Symbol)
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
            NetPlayer player = targetRig.Creator;
            if (player == null || player.IsNull || player.UserId != UserId) return;
            SetContent();
        }

        private void OnPlayerLeft(NetPlayer player)
        {
            if (player == null || player.IsNull || player.UserId != UserId) return;
            OnRoomLeft();
        }

        private void OnRoomLeft()
        {
            UserId = null;
            SetContent();
        }
    }
}
