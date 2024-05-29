using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using Photon.Pun;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Scoreboard")]
    public class PlayerListPage : Page
    {
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
            modded = reference.Contains(Utilla.Models.Gamemode.GamemodePrefix);
        }
    }
}
