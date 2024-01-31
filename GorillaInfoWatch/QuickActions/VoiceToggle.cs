using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using UnityEngine;
using HarmonyLib;
using System;

namespace GorillaInfoWatch.QuickActions
{
    public class VoiceToggle : IQuickAction
    {
        public string Name => "Toggle Voice Chat";
        public ActionType Type => ActionType.Toggle;

        public bool InitialState => PlayerPrefs.GetString("voiceChatOn", "TRUE") != "TRUE";

        public Action<bool> OnActivate => (bool active) =>
        {
            GorillaComputer.instance.voiceChatOn = active ? "FALSE" : "TRUE";
            PlayerPrefs.SetString("voiceChatOn", GorillaComputer.instance.voiceChatOn);

            AccessTools.Method(typeof(GorillaTagger).Assembly.GetType("RigContainer"), "RefreshAllRigVoices").Invoke(null, null);
        };
    }
}