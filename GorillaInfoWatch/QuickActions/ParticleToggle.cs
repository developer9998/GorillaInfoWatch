using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using UnityEngine;
using HarmonyLib;
using System;

namespace GorillaInfoWatch.QuickActions
{
    public class ParticleToggle : IQuickAction
    {
        public string Name => "Toggle Particles";
        public ActionType Type => ActionType.Toggle;

        public bool InitialState => PlayerPrefs.GetString("disableParticles", "FALSE") != "FALSE";

        public Action<bool> OnActivate => (bool active) =>
        {
            GorillaComputer.instance.disableParticles = active;
            PlayerPrefs.SetString("disableParticles", GorillaComputer.instance.disableParticles.ToString().ToUpper());
            GorillaTagger.Instance.ShowCosmeticParticles(!GorillaComputer.instance.disableParticles);
        };
    }
}