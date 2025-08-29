using GorillaInfoWatch.Models.Enumerations;

namespace GorillaInfoWatch.Models.Significance
{
    public class PlayerSignificance
    {
        public string Title { get; }
        public Symbols Symbol { get; }

        public virtual string Description
        {
            get => string.IsNullOrEmpty(internalDescription) ? string.Empty : internalDescription;
        }

        private readonly string internalDescription;

        internal PlayerSignificance(string title, Symbols symbol, string description = null)
        {
            Title = title;
            Symbol = symbol;
            internalDescription = description;
        }

        public virtual bool IsValid(NetPlayer player) => false;
    }
}
