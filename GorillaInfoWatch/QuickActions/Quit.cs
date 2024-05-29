using GorillaInfoWatch.Interfaces;
using Photon.Pun;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class Quit : IQuickAction
    {
        public bool? Active => null;
        public string Name => "Quit Game";
        public Action Function => () =>
        {
            PhotonNetwork.Disconnect();
            Application.Quit();
        };
    }
}