using GorillaInfoWatch.Utilities;
using PlayFab.ClientModels;
using System;

namespace GorillaInfoWatch.Extensions
{
    public static class PlayerExtensions
    {
        public static GetAccountInfoResult GetAccountInfo(this NetPlayer netPlayer, Action<GetAccountInfoResult> onAccountInfoRecieved) => PlayerUtility.GetAccountInfo(netPlayer.UserId, onAccountInfoRecieved);

        public static string GetName(this NetPlayer player, bool filterEmptyNames = true)
        {
            bool isNamePermissionEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
            string nickName = player.NickName;
            string defaultName = player.DefaultName;
            return isNamePermissionEnabled ? ((filterEmptyNames && (string.IsNullOrEmpty(nickName) || string.IsNullOrWhiteSpace(nickName))) ? defaultName : nickName) : defaultName;
        }
    }
}
