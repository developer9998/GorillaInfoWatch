using GorillaInfoWatch.Interfaces;
using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class SetParticles : IQuickAction
    {
        public string Name => "Set Particles";

        public Action<bool> Function => (bool active) =>
        {
            GorillaComputer.instance.disableParticles = active;
            PlayerPrefs.SetString("disableParticles", GorillaComputer.instance.disableParticles.ToString().ToUpper());
            GorillaTagger.Instance.ShowCosmeticParticles(!GorillaComputer.instance.disableParticles);
        };
    }
}