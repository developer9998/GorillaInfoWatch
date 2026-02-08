using GorillaNetworking;

namespace GorillaInfoWatch.Models.Significance;

public class ItemSignificance : PlayerSignificance
{
    public string ItemId { get; }

    public override string Description
    {
        get
        {
            if (string.IsNullOrEmpty(itemDisplayName)) itemDisplayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(ItemId));
            return string.Concat("{0} owns the \"", itemDisplayName, "\" item");
        }
    }

    private string itemDisplayName = null;

    internal ItemSignificance(string title, Symbols symbol, string itemId) : base(title, symbol) => ItemId = itemId;

    public override bool IsValid(NetPlayer player)
    {
        if (player is null || player.IsNull)
            return false;

        if (player.IsLocal)
            return CosmeticsController.instance?.concatStringCosmeticsAllowed is string localAllowedCosmetics && localAllowedCosmetics.Contains(ItemId);

        if (VRRigCache.Instance.TryGetVrrig(player, out RigContainer playerRig))
            return playerRig.Rig.rawCosmeticString is string allowedCosmetics && allowedCosmetics.Contains(ItemId);

        return false;
    }
}
