using GorillaInfoWatch.Attributes;
using GorillaInfoWatch.Models;
using Photon.Pun;
using System.Linq;

namespace GorillaInfoWatch.Pages
{
    [DisplayInHomePage("Scoreboard")]
    public class PlayerListPage : Page
    {
        public override void OnDisplay()
        {
            base.OnDisplay();

            if (NetworkSystem.Instance.InRoom)
            {
                SetHeader("Scoreboard", string.Format("Room ID: {0} Gamemode: {1} {2}/{3}", (NetworkSystem.Instance.SessionIsPrivate ? "-Private-" : NetworkSystem.Instance.RoomName), GorillaGameManager.instance.GameModeName(), NetworkSystem.Instance.RoomPlayerCount, 10));
                NetPlayer[] netPlayers = [.. PhotonNetwork.PlayerList.Select(player => NetworkSystem.Instance.GetPlayer(player.ActorNumber))];
                for (int i = 0; i < netPlayers.Length; i++)
                {
                    AddPlayer(netPlayers[i], button: new LineButton(OnPlayerSelected, i, true, false));
                }
            }
            else
            {
                SetHeader("Scoreboard", "-Not in Room-");
            }

            SetLines();
        }

        public void OnPlayerSelected(object sender, ButtonArgs args)
        {
            NetPlayer player = ((NetPlayer[])[.. PhotonNetwork.PlayerList.Select(player => NetworkSystem.Instance.GetPlayer(player.ActorNumber))])[args.returnIndex];
            ShowPage(typeof(PlayerInfoPage), [player]);
        }
    }
}
