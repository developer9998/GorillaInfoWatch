using System.Linq;

namespace GorillaInfoWatch.Models.Significance
{
    public class ItemSignificance(InfoWatchSymbol symbol, string itemId) : PlayerSignificance(symbol)
    {
        public string ItemId { get; } = itemId;

        public override bool IsValid(NetPlayer player)
        {
            if (player is not null && !player.IsNull && VRRigCache.Instance.TryGetVrrig(player, out RigContainer container) && container.Rig is VRRig rig)
            {
                return rig.IsItemAllowed(ItemId) || (rig.cosmeticSet is var set && set.items.Any(item => item.itemName == ItemId));
            }

            return false;
        }
    }
}
