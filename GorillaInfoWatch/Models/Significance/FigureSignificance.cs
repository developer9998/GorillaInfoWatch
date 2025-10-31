using System;
using System.Linq;
using GorillaInfoWatch.Models.Enumerations;

namespace GorillaInfoWatch.Models.Significance;

public class FigureSignificance : PlayerSignificance
{
    private readonly string figureDescription;

    internal FigureSignificance(string title, Symbols symbol, string description, string[] userIds) : base(title,
            symbol)
    {
        UserIDs = Array.ConvertAll(userIds, userId => userId.Contains(" ") ? userId.Split(" ").First() : userId);
        figureDescription = description;
    }

    public string[] UserIDs { get; }

    public override string Description => figureDescription;

    public override bool IsValid(NetPlayer player) =>
            player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
}