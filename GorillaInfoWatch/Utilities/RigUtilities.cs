namespace GorillaInfoWatch.Utilities
{
    public static class RigUtilities
    {
        public static bool HasItem(RigContainer playerRig, string item_name, bool wearing_item = true, bool item_allowed = true)
        {
            if (playerRig == null) return false;

            if (wearing_item && playerRig.Rig.cosmeticSet != null)
            {
                return playerRig.Rig.cosmeticSet.HasItem(item_name) && (!item_allowed || playerRig.Rig.IsItemAllowed(item_name));
            }

            return item_allowed && playerRig.Rig.IsItemAllowed(item_name);
        }
    }
}