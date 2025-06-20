using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models.Significance
{
    public class PlayerSignificance(Sprite sprite)
    {
        public Sprite Sprite { get; } = sprite;

        public PlayerSignificance(InfoWatchSymbol symbol): this(Main.Instance.Sprites.TryGetValue(symbol, out Sprite sprite) ? sprite : null)
        {

        }

        public virtual bool IsValid(NetPlayer player)
        {
            return false;
        }

        public override string ToString() => Sprite.name;
    }
}
