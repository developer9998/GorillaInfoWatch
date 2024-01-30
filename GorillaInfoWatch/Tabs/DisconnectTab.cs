using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System;

namespace GorillaInfoWatch.Tabs
{
    public class DisconnectEntry : IEntry
    {
        public string Name => "Disconnect";
        public Type EntryType => typeof(DisconnectTab);
    }

    public class DisconnectTab : Tab
    {
        public override ExecutionType ExecutionType => ExecutionType.Callable;

        public override void OnTabDisplayed(object[] Parameters)
        {
            PhotonNetworkController.Instance.AttemptDisconnect();
        }
    }
}
