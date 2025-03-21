using System.Collections.Generic;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Scoreboard")]
    public class ScoreboardScreen : WatchScreen
    {
        public override string Title => "Scoreboard";

        public GorillaScoreBoard last_board;

        public override void OnScreenOpen()
        {
            Build();
        }
        
        public void Build()
        {
            LineBuilder = new();

            if (!NetworkSystem.Instance.InRoom)
            {
                Description = "";
                LineBuilder.AddLine("<align=\"center\">Not in Room</align>");
                return;
            }

            Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";

            var players_in_room = new List<NetPlayer>(NetworkSystem.Instance.AllNetPlayers);
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull) continue;

                string player_name;
                if (!GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && RigUtils.TryGetVRRig(player, out RigContainer container) && container.Rig)
                {
                    player_name = container.Rig.playerNameVisible;
                }
                else
                {
                    player_name = player.NickName.NormalizeName();
                }
                
                LineBuilder.AddLine($"<line-indent=3em>{player_name}", new WidgetPlayerSwatch(player), new WidgetPlayerSpeaker(player));
            }
        }

        /*
        private Gorillanalytics _analytics;

        public PlayerListPage()
        {
            _analytics = UnityEngine.Object.FindObjectOfType<Gorillanalytics>();
        }

        public override void OnDisplay()
        {
            base.OnDisplay();

            if (NetworkSystem.Instance.InRoom)
            {
                TextInfo _textInfo = new CultureInfo("en-US", false).TextInfo;

                PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out object obj);
                GetModeInfo(obj.ToString(), out string serverMode, out bool serverModded);

                // TODO: clean this up, probably make it it's own sort of internal-lib i can use
                SetHeader("Scoreboard", string.Format("Room ID: {0} | Gamemode: {1} | {2}/{3}", NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName, serverMode == "unknown" ? GorillaGameManager.instance != null ? GorillaGameManager.instance.GameModeName() : "Unknown" : (serverModded ? string.Concat("Modded ", _textInfo.ToTitleCase(serverMode.ToLower())) : _textInfo.ToTitleCase(serverMode.ToLower())), NetworkSystem.Instance.RoomPlayerCount, 10));
                NetPlayer[] netPlayers = [.. PhotonNetwork.PlayerList.Select(player => NetworkSystem.Instance.GetPlayer(player.ActorNumber))];
                for (int i = 0; i < netPlayers.Length; i++)
                {
                    AddPlayer(netPlayers[i], button: new LineButton(OnPlayerSelected, i, true, false));
                }
            }
            else
            {
                SetHeader("Scoreboard", "You are not connected to a room");
            }

            SetLines();
        }

        public void OnPlayerSelected(object sender, ButtonArgs args)
        {
            NetPlayer player = ((NetPlayer[])[.. PhotonNetwork.PlayerList.Select(player => NetworkSystem.Instance.GetPlayer(player.ActorNumber))])[args.returnIndex];
            ShowPage(typeof(PlayerInfoPage), [player]);
        }

        private void GetModeInfo(string reference, out string mode, out bool modded)
        {
            List<string> modes = _analytics.modes;
            mode = modes.Find(reference.Contains) ?? "unknown";
            modded = reference.Contains("_MODDED");
        }
        */
    }
}
