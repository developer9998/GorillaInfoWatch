using GorillaInfoWatch.Interfaces;
using System;

namespace GorillaInfoWatch.QuickActions
{
    public class Disconnect : IQuickAction
    {
        public string Name => "Disconnect";

        public Action Function => () => NetworkSystem.Instance.ReturnToSinglePlayer();
    }
}