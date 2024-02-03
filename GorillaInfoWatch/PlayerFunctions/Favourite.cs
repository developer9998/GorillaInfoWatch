using GorillaInfoWatch.Interfaces;
using System;
using Photon.Realtime;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.PlayerFunctions
{
    public class Favourite : IPlayerFunction
    {
        public Action<Player, VRRig> OnPlayerJoin => (Player Player, VRRig Rig) =>
        {
            Rig.playerText.color = DataManager.GetItem(string.Concat(Player.UserId, "fav"), false, DataType.Stored) ? new Color32(110, 183, 183, 255) : Color.white;
        };

        public Action<Player, VRRig> OnPlayerLeave => (Player Player, VRRig Rig) =>
        {
            Rig.playerText.color = Color.white;
        };
    }
}
