using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class Quit : IQuickAction
    {
        public string Name => "Quit Game";
        public ActionType Type => ActionType.Static;

        public bool InitialState => true;

        public Action<bool> OnActivate => (bool active) => Application.Quit();
    }
}