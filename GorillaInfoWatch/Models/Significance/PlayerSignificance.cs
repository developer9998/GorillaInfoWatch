namespace GorillaInfoWatch.Models.Significance
{
    public class PlayerSignificance(string title, InfoWatchSymbol symbol)
    {
        public string Title { get; } = title;
        public InfoWatchSymbol Symbol { get; } = symbol;

        public virtual bool IsValid(NetPlayer player) => false;
    }
}
