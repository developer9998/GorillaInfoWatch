using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaNetworking;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class FriendScreen : WatchScreen
    {
        public override string Title => "Friends";

        public List<FriendBackendController.Friend> FriendsList;

        public override void OnScreenOpen()
        {
            FriendSystem.Instance.OnFriendListRefresh += OnGetFriendsReceived;
            RequestFriendsList();
        }

        public override void OnScreenClose()
        {
            FriendSystem.Instance.OnFriendListRefresh -= OnGetFriendsReceived;
        }

        public override void OnScreenRefresh()
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
            if (FriendsList == null)
                return new LineBuilder("<align=\"center\">Retrieving friends list</align>");

            var list = FriendsList.Where(friend => friend is not null).Select(friend => friend.Presence).ToArray();

            if (list.Length == 0)
                return new LineBuilder("<align=\"center\">No friends are currently active</align>");

            var lines = new LineBuilder($"<align=\"center\">Showing {list.Length} friends:</align>");

            foreach (FriendBackendController.FriendPresence presence in list)
            {
                GetAccountInfoResult accountInfo = PlayerEx.GetAccountInfo(presence.FriendLinkId, null);
                if (accountInfo is null)
                    continue;

                if (!string.IsNullOrEmpty(presence.RoomId) && presence.RoomId.Length > 0)
                {
                    bool has_room = !string.IsNullOrEmpty(presence.RoomId) && presence.RoomId.Length > 0;
                    bool? isPublic = presence.IsPublic;
                    bool is_public_room = (isPublic.GetValueOrDefault() == true) & (isPublic != null);
                    bool has_vstump_prepend = presence.RoomId[0] == '@';
                    string line_content = $"{presence.UserName}: {(has_room ? (has_vstump_prepend ? $"CUSTOM: {presence.RoomId}" : (!is_public_room ? $"PRIVATE: {presence.RoomId}" : $"{presence.Zone}: {presence.RoomId}")) : "Offline")}";

                    bool is_in_room = presence.RoomId.Equals(NetworkSystem.Instance.RoomName);
                    bool in_zone = false;
                    if (!is_in_room && is_public_room && !presence.Zone.IsNullOrEmpty())
                    {
                        string text = presence.Zone.ToLower();
                        in_zone = ZoneManagement.instance.activeZones.Any(zone => text.Contains(zone.GetName().ToLower()));
                    }

                    bool joinable = !has_vstump_prepend && !is_in_room && (!is_public_room || in_zone);
                    if (joinable)
                    {
                        lines.AddLine(line_content, new PushButton(FriendButtonClick, presence));
                    }
                    else
                    {
                        lines.AddLine(line_content);
                    }
                    continue;
                }

                lines.AddLine($"{(string.IsNullOrEmpty(presence.UserName) || string.IsNullOrWhiteSpace(presence.UserName) ? accountInfo.AccountInfo.TitleInfo.DisplayName[0..^4] : presence.UserName)}: OFFLINE");
            }

            return lines;
        }

        public void FriendButtonClick(object[] args)
        {
            if (args[0] is FriendBackendController.FriendPresence presence)
            {
                bool? isPublic = presence.IsPublic;
                JoinType joinType = ((isPublic.GetValueOrDefault() == true) & (isPublic != null)) ? JoinType.FriendStationPublic : JoinType.FriendStationPrivate;
                bool has_vstump_prepend = presence.RoomId[0] == '@';
                string friend_room = has_vstump_prepend ? presence.RoomId[1..].ToUpper() : presence.RoomId.ToUpper();
                GorillaComputer.instance.roomToJoin = friend_room;
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(friend_room, joinType);
            }
        }
    }
}