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
        private readonly Configuration Config;

        public Favourite(Configuration configuration)
        {
            Config = configuration;
        }

        public Action<PlayerInfo> OnPlayerJoin => (PlayerInfo Arguments) =>
        {
            bool isRecognised = DataManager.GetItem(string.Concat(Arguments.Player.UserId, "rec"), false);
            bool isFriend = DataManager.GetItem(string.Concat(Arguments.Player.UserId, "fav"), false, DataType.Stored);
            Arguments.Rig.playerText.color = isFriend ? PresetUtils.Parse(Config.FavouriteColour.Value) : (isRecognised ? PresetUtils.Parse(Config.VerifiedColour.Value) : Color.white);
        };

        public Action<PlayerInfo> OnPlayerLeave => (PlayerInfo Arguments) =>
        {
            Arguments.Rig.playerText.color = Color.white;
        };
    }
}
