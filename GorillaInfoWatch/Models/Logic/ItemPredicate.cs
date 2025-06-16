namespace GorillaInfoWatch.Models.Logic
{
    public class ItemPredicate(InfoWatchSymbol symbol, string itemId) : PlayerPredicate(symbol)
    {
        public string ItemId { get; } = itemId;

        public override bool IsValid(NetPlayer player)
        {
            if (player is not null && !player.IsNull && VRRigCache.Instance.TryGetVrrig(player, out RigContainer container) && container.Rig is VRRig rig)
            {
                return rig.concatStringOfCosmeticsAllowed.Contains(ItemId);
            }

            return false;
        }
    }
}
