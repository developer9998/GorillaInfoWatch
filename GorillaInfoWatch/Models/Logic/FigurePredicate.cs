using System;
using System.Linq;

namespace GorillaInfoWatch.Models.Logic
{
    public class FigurePredicate(InfoWatchSymbol symbol, string[] userIds) : PlayerPredicate(symbol)
    {
        public string[] UserIDs { get; } = userIds;

        public override bool IsValid(NetPlayer player)
        {
            return player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
        }
    }
}
