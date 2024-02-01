using GorillaInfoWatch.Interfaces;
using GorillaNetworking;
using HarmonyLib;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class SetVoice : IQuickAction
    {
        public string Name => "Set Voice Chat";

        public Action Function => () =>
        {
            GorillaComputer.instance.voiceChatOn = GorillaComputer.instance.voiceChatOn == "TRUE" ? "FALSE" : "TRUE";
            PlayerPrefs.SetString("voiceChatOn", GorillaComputer.instance.voiceChatOn);

            AccessTools.Method(typeof(GorillaTagger).Assembly.GetType("RigContainer"), "RefreshAllRigVoices").Invoke(null, null);
        };
    }
}