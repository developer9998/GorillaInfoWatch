namespace GorillaInfoWatch.Models.Logic
{
    public class PlayerPredicate(InfoWatchSymbol symbol)
    {
        public InfoWatchSymbol Symbol { get; } = symbol;

        public virtual bool IsValid(NetPlayer player)
        {
            return false;
        }

        public override string ToString() => Symbol.ToString();
    }
}
