using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using GorillaTagScripts.ModIO;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen]
    public class FriendScreen : InfoWatchScreen
    {
        public override string Title => "Friends";

        public List<FriendBackendController.Friend> FriendsList;

        public override void OnShow()
        {
            FriendSystem.Instance.OnFriendListRefresh += OnGetFriendsReceived;
            RequestFriendsList();
        }

        public override void OnClose()
        {
            FriendSystem.Instance.OnFriendListRefresh -= OnGetFriendsReceived;
        }

        public override void OnRefresh()
        {
            RequestFriendsList();
        }

        public void RequestFriendsList()
        {
            FriendsList = null;
            FriendSystem.Instance.RefreshFriendsList();
        }

        public async void OnGetFriendsReceived(List<FriendBackendController.Friend> friendsList)
        {
            foreach (var friend in friendsList)
            {
                if (friend is null || friend.Presence is not FriendBackendController.FriendPresence presence) continue;

                string userId = presence.FriendLinkId;

                TaskCompletionSource<GetAccountInfoResult> completionSource = new();
                GetAccountInfoResult currentAccountInfo = PlayerEx.GetAccountInfo(userId, completionSource.SetResult);
                currentAccountInfo ??= await completionSource.Task;
            }

            FriendsList = friendsList;

            SetContent();
        }

        public override ScreenContent GetContent()
        {
            LineBuilder lines = new();

            // FriendSystem.PlayerPrivacy is more readable to users
            FriendSystem.PlayerPrivacy localPlayerPrivacy = (FriendSystem.PlayerPrivacy)(int)FriendBackendController.Instance.MyPrivacyState;

            lines.Add($"Privacy: {localPlayerPrivacy}", new Widget_SnapSlider((int)localPlayerPrivacy, 0, 2, selection =>
            {
                FriendBackendController.Instance.lastPrivacyState = (FriendBackendController.PrivacyState)selection;
                FriendBackendController.Instance.SetPrivacyState((FriendBackendController.PrivacyState)selection);
                SetContent();
            })
            {
                Colour = GradientUtils.FromColour(Gradients.Green.Evaluate(0), Gradients.Red.Evaluate(0))
            });
            lines.Skip();

            if (FriendsList == null)
            {
                lines.Add("<align=\"center\">Refreshing friends list</align>");
                return lines;
            }

            var list = FriendsList.Where(friend => friend is not null).Select(friend => friend.Presence).ToArray();

            if (list.Length == 0)
            {
                lines.Add("<align=\"center\">No friends were found</align>");
                return lines;
            }

            lines.Add($"<align=\"center\">Showing {list.Length} friends:</align>");

            foreach (FriendBackendController.FriendPresence presence in list)
            {
                GetAccountInfoResult accountInfo = PlayerEx.GetAccountInfo(presence.FriendLinkId, null);

                if (accountInfo is null)
                    continue;

                string userName = (string.IsNullOrEmpty(presence.UserName) || presence.UserName.Length == 0) ? accountInfo.AccountInfo.TitleInfo.DisplayName[0..^4].SanitizeName() : presence.UserName;

                string roomName = presence.RoomId;
                bool isRoomPublic = presence.IsPublic.GetValueOrDefault(false);
                bool isOffline = string.IsNullOrEmpty(roomName) || roomName.Length == 0;
                bool inVirtualStump = !isOffline && roomName.StartsWith(GorillaComputer.instance.VStumpRoomPrepend);
                bool inZone = presence.Zone != null && ZoneManagement.instance.activeZones.Select(zone => zone.GetName().ToLower()).Any(zone => zone == presence.Zone);

                string roomText = isOffline ? "Offline" : (inVirtualStump ? $"{roomName} in Virtual Stump" : ((isRoomPublic && presence.Zone != null) ? $"{roomName} in {presence.Zone.ToUpper()}" : roomName));
                string text = string.Format("{0}: {1}", userName, roomText);

                if (isOffline)
                {
                    lines.Add(text);
                    continue;
                }

                if (roomName.Equals(NetworkSystem.Instance.RoomName))
                {
                    lines.Add(text, new Widget_PushButton()
                    {
                        Colour = Gradients.Yellow,
                        Symbol = InfoWatchSymbol.LightBulb
                    });
                    continue;
                }

                if ((!isRoomPublic || inZone) && !inVirtualStump)
                {
                    lines.Add(text, new Widget_PushButton(JoinFriend, presence, inVirtualStump)
                    {
                        Colour = Gradients.Green,
                        Symbol = InfoWatchSymbol.GreenFlag
                    });
                    continue;
                }

                lines.Add(text, new Widget_PushButton()
                {
                    Colour = Gradients.Red,
                    Symbol = InfoWatchSymbol.RedFlag
                });
            }

            return lines;
        }

        public void JoinFriend(object[] args)
        {
            if (args.ElementAtOrDefault(0) is FriendBackendController.FriendPresence presence && args.ElementAtOrDefault(1) is bool isVirtualStump)
            {
                if (isVirtualStump && !GorillaComputer.instance.IsPlayerInVirtualStump())
                {
                    try
                    {
                        GameObject treeRoom = ZoneManagement.instance.allObjects.First(gameObject => gameObject.name == "TreeRoom");
                        VirtualStumpTeleporter teleporter = treeRoom.GetComponentInChildren<VirtualStumpTeleporter>(true);
                        StartCoroutine(CustomMapManager.TeleportToVirtualStump(teleporter.teleporterIndex, success =>
                        {
                            if (success) JoinFriend(args);
                        }, teleporter.entrancePoint, teleporter.mySerializer));
                    }
                    catch(Exception ex)
                    {
                        Logging.Fatal("Teleporting player to virtual stump");
                        Logging.Error(ex);
                    }

                    return;
                }

                if (isVirtualStump && GorillaComputer.instance.IsPlayerInVirtualStump() && NetworkSystem.Instance.InRoom)
                {
                    CustomMapManager.ReturnToVirtualStump();
                }

                GorillaComputer.instance.roomToJoin = presence.RoomId;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(presence.RoomId, presence.IsPublic.GetValueOrDefault(false) ? JoinType.FriendStationPublic : JoinType.FriendStationPrivate);
            }
        }
    }
}