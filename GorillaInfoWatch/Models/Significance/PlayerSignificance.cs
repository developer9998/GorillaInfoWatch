namespace GorillaInfoWatch.Models.Significance;

public class PlayerSignificance
{
    public string Title { get; }
    public Symbols Symbol { get; }

    public virtual string Description => string.IsNullOrEmpty(_description) ? string.Empty : _description;
    public virtual SignificanceVisibility Visibility => SignificanceVisibility.None;

    private readonly string _description;

    internal PlayerSignificance(string title, Symbols symbol, string description = null)
    {
        Title = title;
        Symbol = symbol;
        _description = description;
    }

    public virtual bool IsValid(NetPlayer player) => false;
}
