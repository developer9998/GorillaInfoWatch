using GorillaInfoWatch.Interfaces;
using System;
using UnityEngine;

namespace GorillaInfoWatch.QuickActions
{
    public class Quit : IQuickAction
    {
        public string Name => "Quit Game";

        public Action<bool> Function => (bool active) => Application.Quit();
    }
}