using GorillaInfoWatch.Models.Enumerations;

namespace GorillaInfoWatch.Models.Significance;

public class PlayerSignificance
{
    private readonly string internalDescription;

    internal PlayerSignificance(string title, Symbols symbol, string description = null)
    {
        Title               = title;
        Symbol              = symbol;
        internalDescription = description;
    }

    public string  Title  { get; }
    public Symbols Symbol { get; }

    public virtual string Description => string.IsNullOrEmpty(internalDescription) ? string.Empty : internalDescription;

    public virtual bool IsValid(NetPlayer player) => false;
}