using GorillaInfoWatch.Interfaces;
using GorillaNetworking;
using System;

namespace GorillaInfoWatch.QuickActions
{
    public class Disconnect : IQuickAction
    {
        public string Name => "Disconnect";

        public Action<bool> Function => (bool active) => PhotonNetworkController.Instance.AttemptDisconnect();
    }
}