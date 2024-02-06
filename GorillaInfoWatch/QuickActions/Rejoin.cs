using GorillaInfoWatch.Interfaces;
using GorillaNetworking;
using Photon.Pun;
using System;
using System.Threading.Tasks;

namespace GorillaInfoWatch.QuickActions
{
    public class Rejoin : IQuickAction
    {
        private bool isRejoining;

        public string Name => "Rejoin";

        public Action Function => async () =>
        {
            if (PhotonNetwork.InRoom && !isRejoining)
            {
                string roomID = PhotonNetwork.CurrentRoom.Name;

                isRejoining = true;
                PhotonNetworkController.Instance.AttemptDisconnect();

                while (!PhotonNetwork.IsConnectedAndReady)
                {
                    await Task.Yield();
                }

                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomID);
                isRejoining = false;
            }
        };
    }
}
