using System;
using System.Linq;

namespace GorillaInfoWatch.Models.Significance
{
    public class FigureSignificance(InfoWatchSymbol symbol, string[] userIds) : PlayerSignificance(symbol)
    {
        public string[] UserIDs { get; } = Array.ConvertAll(userIds, userId => userId.Contains(" ") ? userId.Split(" ").First() : userId);

        public override bool IsValid(NetPlayer player)
        {
            return player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
        }
    }
}
