using GorillaInfoWatch.Models.Enumerations;

namespace GorillaInfoWatch.Models.Significance
{
    public class PlayerSignificance
    {
        public string Title { get; }
        public Symbols Symbol { get; }

        internal PlayerSignificance(string title, Symbols symbol)
        {
            Title = title;
            Symbol = symbol;
        }

        public virtual bool IsValid(NetPlayer player) => false;
    }
}
