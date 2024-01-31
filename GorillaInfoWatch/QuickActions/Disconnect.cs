using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System;

namespace GorillaInfoWatch.QuickActions
{
    public class Disconnect : IQuickAction
    {
        public string Name => "Disconnect";
        public ActionType Type => ActionType.Static;

        public bool InitialState => true;

        public Action<bool> OnActivate => (bool active) => PhotonNetworkController.Instance.AttemptDisconnect();
    }
}