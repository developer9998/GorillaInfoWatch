using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using HarmonyLib;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class SetVoice : IQuickAction
    {
        public string Name => "Set Voice Chat";
        public ActionType Type => ActionType.Toggle;

        public bool InitialState => PlayerPrefs.GetString("voiceChatOn", "TRUE") == "TRUE";

        public Action<bool> OnActivate => (bool active) =>
        {
            GorillaComputer.instance.voiceChatOn = active ? "TRUE" : "FALSE";
            PlayerPrefs.SetString("voiceChatOn", GorillaComputer.instance.voiceChatOn);

            AccessTools.Method(typeof(GorillaTagger).Assembly.GetType("RigContainer"), "RefreshAllRigVoices").Invoke(null, null);
        };
    }
}