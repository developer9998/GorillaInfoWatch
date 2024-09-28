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

        public bool? Active => null;

        public string Name => "Rejoin";

        public Action Function => async () =>
        {
            if (PhotonNetwork.InRoom && !isRejoining)
            {
                string roomID = PhotonNetwork.CurrentRoom.Name;

                await NetworkSystem.Instance.ReturnToSinglePlayer();

                while (!PhotonNetwork.IsConnectedAndReady)
                {
                    await Task.Yield();
                }

                PhotonNetwork.RejoinRoom(roomID);
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomID, JoinType.Solo);
                isRejoining = false;
            }
        };
    }
}
