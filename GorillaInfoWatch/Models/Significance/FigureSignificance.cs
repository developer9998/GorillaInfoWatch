using System;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    public class FigureSignificance(Sprite sprite, string[] userIds) : PlayerSignificance(sprite)
    {
        public string[] UserIDs { get; } = Array.ConvertAll(userIds, userId => userId.Contains(" ") ? userId.Split(" ").First() : userId);

        public override bool IsValid(NetPlayer player)
        {
            return player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
        }
    }
}
