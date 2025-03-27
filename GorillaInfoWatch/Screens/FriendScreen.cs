using GorillaInfoWatch.Models;
using GorillaInfoWatch.Attributes;
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;

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

        public void RequestFriendsList()
        {
            FriendsList = null;
            FriendSystem.Instance.RefreshFriendsList();
            Draw();
            SetLines();
        }

        public void OnGetFriendsReceived(List<FriendBackendController.Friend> friendsList)
        {
            FriendsList = friendsList;
            Draw();
            SetLines();
        }

        public void Draw()
        {
            LineBuilder = new();

            if (FriendsList == null)
            {
                LineBuilder.AddLine("<align=\"center\">Retrieving friends list</align>");
                return;
            }

            List<FriendBackendController.FriendPresence> list = [];
            foreach(var friend in FriendsList)
            {
                if (friend == null) continue;
                var presence = friend.Presence;
                if (presence == null) continue;
                string friend_name = presence.UserName;
                string room_id = presence.RoomId;
                if ((string.IsNullOrEmpty(friend_name) || string.IsNullOrWhiteSpace(friend_name)) && (string.IsNullOrEmpty(room_id) || string.IsNullOrWhiteSpace(room_id))) continue;
                list.Add(presence);
            }

            if (list.Count == 0)
            {
                LineBuilder.AddLine("<align=\"center\">No friends are currently active</align>");
                return;
            }

            LineBuilder.AddLine($"<align=\"center\">Showing {list.Count} friends:</align>");

            foreach(FriendBackendController.FriendPresence presence in list)
            {
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
                        LineBuilder.AddLine(line_content, new WidgetButton(FriendButtonClick, presence));
                    }
                    else
                    {
                        LineBuilder.AddLine(line_content);
                    }
                    continue;
                }

                LineBuilder.AddLine($"{presence.UserName}: OFFLINE");
            }
        }

        public void FriendButtonClick(bool value, object[] args)
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