using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System;

namespace GorillaInfoWatch.Tabs
{
    public class DisconnectEntry : IEntry
    {
        public string Name => "Disconnect";
        public Type Window => typeof(DisconnectWindow);
    }

    public class DisconnectWindow : Window
    {
        public override ExecutionType ExecutionType => ExecutionType.Callable;

        public override void OnWindowDisplayed(object[] Parameters)
        {
            PhotonNetworkController.Instance.AttemptDisconnect();
        }
    }
}
