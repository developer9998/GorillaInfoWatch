using GorillaInfoWatch.Tools;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

namespace GorillaInfoWatch.Extensions
{
    public static class PlayerEx
    {
        private static readonly Dictionary<string, (GetAccountInfoResult accountInfo, DateTime cacheTime)> accountInfoCache = [];

        public static GetAccountInfoResult GetAccountInfo(this NetPlayer netPlayer, Action<GetAccountInfoResult> onAccountInfoRecieved)
            => GetAccountInfo(netPlayer.UserId, onAccountInfoRecieved);

        public static GetAccountInfoResult GetAccountInfo(string userId, Action<GetAccountInfoResult> onAccountInfoRecieved)
        {
            if (accountInfoCache.ContainsKey(userId) && (DateTime.Now - accountInfoCache[userId].cacheTime).TotalMinutes < 5)
                return accountInfoCache[userId].accountInfo;

            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
            {
                PlayFabId = userId
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

        public static string GetNameRef(this NetPlayer player)
        {
            bool isNamePermissionEnabled = KIDManager.CheckFeatureSettingEnabled(EKIDFeatures.Custom_Nametags);
            string playerName = player.NickName;
            string defaultName = player.DefaultName;
            return isNamePermissionEnabled ? ((string.IsNullOrEmpty(playerName) || string.IsNullOrWhiteSpace(playerName)) ? defaultName : playerName) : defaultName;
        }
    }
}
