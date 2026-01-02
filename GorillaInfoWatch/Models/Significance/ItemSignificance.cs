using GorillaNetworking;
using System.Linq;

namespace GorillaInfoWatch.Models.Significance;

public class ItemSignificance : PlayerSignificance
{
    public string ItemId { get; }

    public override SignificanceVisibility Visibility => SignificanceVisibility.Item;
    public override string Description
    {
        get
        {
            if (string.IsNullOrEmpty(_itemDisplayName)) _itemDisplayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(ItemId));
            return string.Concat("{0} owns the \"", _itemDisplayName, "\" item");
        }
    }

    private string _itemDisplayName = null;

    internal ItemSignificance(string title, Symbols symbol, string itemId) : base(title, symbol) => ItemId = itemId;

    public override bool IsValid(NetPlayer player) => GetState(player) > 0;

    internal ItemState GetState(NetPlayer player)
    {
        if (player == null || player.IsNull) return ItemState.None;

        string cosmeticAllowedString;
        CosmeticsController.CosmeticSet cosmeticSet;

        if (player.IsLocal)
        {
            cosmeticAllowedString = CosmeticsController.instance?.concatStringCosmeticsAllowed ?? string.Empty;
            cosmeticSet = CosmeticsController.instance?.currentWornSet ?? CosmeticsController.CosmeticSet.EmptySet;
        }
        else if (VRRigCache.Instance.TryGetVrrig(player, out RigContainer rigContainer) && rigContainer.Rig.InitializedCosmetics)
        {
            cosmeticAllowedString = rigContainer.Rig.concatStringOfCosmeticsAllowed ?? string.Empty;
            cosmeticSet = rigContainer.Rig.cosmeticSet ?? CosmeticsController.CosmeticSet.EmptySet;
        }
        else return ItemState.None;

        return cosmeticSet.items.Where(item => !item.isNullItem).Any(item => item.itemName == ItemId) ? ItemState.Equipped : (cosmeticAllowedString.Contains(ItemId) ? ItemState.Allowed : ItemState.None);
    }

    internal enum ItemState
    {
        None,
        Allowed,
        Equipped
    }
}
