using System;
using System.Collections.Generic;
using GorillaInfoWatch.Tools;
using PlayFab;
using PlayFab.ClientModels;

namespace GorillaInfoWatch.Extensions;

public static class PlayerExtensions
{
    private static readonly Dictionary<string, (GetAccountInfoResult accountInfo, DateTime cacheTime)>
            accountInfoCache = [];

    public static GetAccountInfoResult GetAccountInfo(this NetPlayer               netPlayer,
                                                      Action<GetAccountInfoResult> onAccountInfoRecieved)
        => GetAccountInfo(netPlayer.UserId, onAccountInfoRecieved);

    public static GetAccountInfoResult GetAccountInfo(string userId, Action<GetAccountInfoResult> onAccountInfoRecieved)
    {
        if (accountInfoCache.ContainsKey(userId) &&
            (DateTime.Now - accountInfoCache[userId].cacheTime).TotalMinutes < 6)
            return accountInfoCache[userId].accountInfo;

        if (!PlayFabClientAPI.IsClientLoggedIn())
            throw new InvalidOperationException("PlayFab Client must be logged in to post the account info request");

        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
        {
                PlayFabId = userId,
        }, accountInfo =>
           {
               if (accountInfoCache.ContainsKey(userId)) accountInfoCache[userId] = (accountInfo, DateTime.Now);
               else accountInfoCache.Add(userId, (accountInfo, DateTime.Now));

               onAccountInfoRecieved?.Invoke(accountInfo);
           }, error =>
              {
                  Logging.Fatal($"PlayFabClientAPI.GetAccountInfo for {userId}");
                  Logging.Error(error.GenerateErrorReport());
              });

        return null;
    }

    public static string GetName(this NetPlayer player, bool filterEmptyNames = true)
    {
        bool   isNamePermissionEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
        string nickName                = player.NickName;
        string defaultName             = player.DefaultName;

        return isNamePermissionEnabled
                       ? filterEmptyNames && (string.IsNullOrEmpty(nickName) || string.IsNullOrWhiteSpace(nickName))
                                 ? defaultName
                                 : nickName
                       : defaultName;
    }
}