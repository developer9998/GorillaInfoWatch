using GorillaInfoWatch.Tools;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;

namespace GorillaInfoWatch.Extensions
{
    public static class PlayerEx
    {
        private static readonly Dictionary<string, GetAccountInfoResult> accountInfoCache = [];

        public static GetAccountInfoResult GetAccountInfo(this NetPlayer player, Action<GetAccountInfoResult> onAccountInfoRecieved)
        {
            if (player is null || player.IsNull || !player.IsValid)
                return null;

            return GetAccountInfo(player.UserId, onAccountInfoRecieved);
        }

        public static GetAccountInfoResult GetAccountInfo(string userId, Action<GetAccountInfoResult> onAccountInfoRecieved)
        {
            if (accountInfoCache.TryGetValue(userId, out GetAccountInfoResult result))
                return result;

            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
            {
                PlayFabId = userId
            },
            result =>
            {
                if (!accountInfoCache.ContainsKey(userId))
                {
                    accountInfoCache.Add(userId, result);
                    onAccountInfoRecieved?.Invoke(result);
                }
            },
            error =>
            {
                Logging.Fatal($"PlayFabClientAPI.GetAccountInfo ({userId})");
                Logging.Error(error.GenerateErrorReport());
            });

            return null;
        }
    }
}
