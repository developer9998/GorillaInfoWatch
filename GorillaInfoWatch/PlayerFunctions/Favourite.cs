using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using System;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Favourite : IPlayerFunction
    {
        private Configuration Config;

        public Favourite(Configuration configuration)
        {
            Config = configuration;
        }

        public Action<PlayerInfo> OnPlayerJoin => (PlayerInfo Arguments) =>
        {
            Arguments.Rig.playerText.color = DataManager.GetItem(string.Concat(Arguments.Player.UserId, "fav"), false, DataType.Stored) ? PresetUtils.Parse(Config.FavouriteColour.Value) : Color.white;
        };

        public Action<PlayerInfo> OnPlayerLeave => (PlayerInfo Arguments) =>
        {
            Arguments.Rig.playerText.color = Color.white;
        };
    }
}
