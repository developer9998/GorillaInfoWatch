using GorillaNetworking;

namespace GorillaInfoWatch.Models.Significance
{
    public class ItemSignificance(InfoWatchSymbol symbol, string itemId) : PlayerSignificance(symbol)
    {
        public string ItemId { get; } = itemId;

        public override bool IsValid(NetPlayer player)
        {
            if (player is null || player.IsNull)
                return false;

            if (player.IsLocal)
                return CosmeticsController.instance.concatStringCosmeticsAllowed is string allowedCosmetics1 && allowedCosmetics1.Contains(ItemId);

            if (VRRigCache.Instance.TryGetVrrig(player, out RigContainer container) && container.Rig is VRRig rig)
                return rig.concatStringOfCosmeticsAllowed is string allowedCosmetics2 && allowedCosmetics2.Contains(ItemId);

            return false;
        }
    }
}
