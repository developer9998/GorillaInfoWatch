using GorillaInfoWatch.Models.Enumerations;
using System;
using System.Linq;

namespace GorillaInfoWatch.Models.Significance
{
    public class FigureSignificance : PlayerSignificance
    {
        public string[] UserIDs { get; }

        internal FigureSignificance(string title, Symbols symbol, string[] userIds): base(title, symbol)
        {
            UserIDs = Array.ConvertAll(userIds, userId => userId.Contains(" ") ? userId.Split(" ").First() : userId);
        }

        public override bool IsValid(NetPlayer player) => player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
    }
}
