using GorillaInfoWatch.Utilities;
using PlayFab.ClientModels;
using System;

namespace GorillaInfoWatch.Extensions;

public static class PlayerExtensions
{
    public static GetAccountInfoResult GetAccountInfo(this NetPlayer netPlayer, Action<GetAccountInfoResult> onAccountInfoRecieved) => PlayerUtility.GetAccountInfo(netPlayer.UserId, onAccountInfoRecieved);

    public static string GetPlayerName(this NetPlayer player, bool limitLength = true)
    {
        bool isNamePermissionEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
        string nickName = player.NickName;
        string defaultName = player.DefaultName;

        string playerName = isNamePermissionEnabled ? ((string.IsNullOrEmpty(nickName) || string.IsNullOrWhiteSpace(nickName)) ? defaultName : nickName) : defaultName;
        return limitLength ? playerName.LimitLength(12) : playerName;
    }
}