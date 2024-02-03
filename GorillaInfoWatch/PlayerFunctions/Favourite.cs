using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using System;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Favourite : IPlayerFunction
    {
        public Action<PlayerArgs> OnPlayerJoin => (PlayerArgs Arguments) =>
        {
            Arguments.Rig.playerText.color = DataManager.GetItem(string.Concat(Arguments.Player.UserId, "fav"), false, DataType.Stored) ? new Color32(110, 183, 183, 255) : Color.white;
        };

        public Action<PlayerArgs> OnPlayerLeave => (PlayerArgs Arguments) =>
        {
            Arguments.Rig.playerText.color = Color.white;
        };
    }
}
