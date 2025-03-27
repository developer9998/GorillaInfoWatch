using System.Collections;
using System.Collections.Generic;
using GorillaGameModes;
using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [DisplayAtHomeScreen]
    public class ScoreboardScreen : WatchScreen
    {
        public override string Title => "Scoreboard";

        private IEnumerator routine = null;
        private int player_count = -1;

        public override void OnScreenOpen()
        {
            Draw();
        }

        public override void OnScreenClose()
        {
            StopRefresh();
        }

        public void Draw()
        {
            LineBuilder = new();

            if (!NetworkSystem.Instance.InRoom)
            {
                Description = "";
                LineBuilder.AddLine("<align=\"center\">Not in Room</align>");
                StopRefresh();
                player_count = -1;
                return;
            }

            Description = $"{NetworkSystem.Instance.RoomPlayerCount}/{PhotonNetworkController.Instance.GetRoomSize(NetworkSystem.Instance.GameModeString)} Room ID: {(NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName)} Game Mode: {((GameMode.ActiveGameMode != null) ? GameMode.ActiveGameMode.GameModeName() : "ERROR")}";
            
            var players_in_room = new List<NetPlayer>(NetworkSystem.Instance.AllNetPlayers);
            players_in_room.Sort((x, y) => x.ActorNumber.CompareTo(y.ActorNumber));

            foreach (NetPlayer player in players_in_room)
            {
                if (player == null || player.IsNull) continue;
                LineBuilder.AddLine((GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) || !RigUtils.TryGetVRRig(player, out RigContainer container)) ? player.NickName.NormalizeName() : container.Rig.playerNameVisible, new WidgetPlayerSwatch(player), new WidgetPlayerSpeaker(player), new WidgetSpecialPlayerSwatch(player), new WidgetButton(TryInspectPlayer, player));
            }

            StartRefresh();
            player_count = NetworkSystem.Instance.RoomPlayerCount;
        }

        public void StartRefresh()
        {
            if (routine == null)
            {
                routine = Refresh(1f / 3f);
                StartCoroutine(routine);
            }
        }

        public void StopRefresh()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
        }

        public IEnumerator Refresh(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                int player_count = this.player_count;
                Draw();
                if (player_count != this.player_count) SetLines();
                else UpdateLines();
            }
        }

        public void TryInspectPlayer(bool value, object[] args)
        {
            if (args[0] is NetPlayer player && RigUtils.TryGetVRRig(player, out RigContainer playerRig))
            {
                PlayerInfoPage.Container = playerRig;
                ShowScreen(typeof(PlayerInfoPage));
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
