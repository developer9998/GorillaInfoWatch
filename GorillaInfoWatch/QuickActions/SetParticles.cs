using GorillaInfoWatch.Interfaces;
using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class SetParticles : IQuickAction
    {
        public bool? Active => !GorillaComputer.instance.disableParticles;

        public string Name => "Toggle Particles";

        public Action Function => () =>
        {
            GorillaComputer.instance.disableParticles ^= true;
            PlayerPrefs.SetString("disableParticles", GorillaComputer.instance.disableParticles.ToString().ToUpper());
            GorillaTagger.Instance.ShowCosmeticParticles(!GorillaComputer.instance.disableParticles);
        };
    }
}