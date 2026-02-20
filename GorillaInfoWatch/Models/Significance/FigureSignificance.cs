using System;
using System.Linq;

namespace GorillaInfoWatch.Models.Significance;

public class FigureSignificance : PlayerSignificance
{
    public string[] UserIDs { get; }

    public override SignificanceVisibility Visibility => SignificanceVisibility.Figure;
    public override string Description => _figureDescription;

    private readonly string _figureDescription;

    internal FigureSignificance(string title, Symbols symbol, string description, string[] userIds) : base(title, symbol)
    {
        UserIDs = Array.ConvertAll(userIds, userId => userId.Contains(" ") ? userId.Split(" ").First() : userId);
        _figureDescription = description;
    }

    public override bool IsValid(NetPlayer player) => player is not null && !player.IsNull && UserIDs.Contains(player.UserId);
}
