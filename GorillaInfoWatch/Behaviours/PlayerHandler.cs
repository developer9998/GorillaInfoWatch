using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Enumerations;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Behaviours
{
    public class PlayerHandler : MonoBehaviour
    {
        public static event Action<NetPlayer, PlayerSignificance> OnSignificanceChanged;

        public static PlayerHandler Instance;

        public static Dictionary<NetPlayer, PlayerSignificance> Significance = [];

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            RoomSystem.JoinedRoomEvent += OnJoinedRoom;
            NetworkSystem.Instance.OnPlayerJoined += OnPlayerJoined;
            NetworkSystem.Instance.OnPlayerLeft += OnPlayerLeft;

            Events.OnQuestCompleted += OnQuestCompleted;
            Events.OnRigRecievedCosmetics += OnGetUserCosmetics;
        }

        public IEnumerator Start()
        {
            yield return new WaitUntil(() =>
            {
                string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
                return playerId != null && playerId.Length > 0;
            });

            if (NetworkSystem.Instance is NetworkSystem netSys)
            {
                netSys.UpdatePlayers();
                EvaluatePlayer(netSys.LocalPlayer);
            }

            yield break;
        }

        public void OnJoinedRoom()
        {
            NetworkSystem.Instance.PlayerListOthers.ForEach(player => EvaluatePlayer(player));
        }

        public void OnPlayerJoined(NetPlayer player)
        {
            EvaluatePlayer(player);

            if (player.IsLocal) // called for the local player when marked "InGame" / connected to a room
                return;

            string userId = player.UserId;

            if (GFriends.IsFriend(userId))
            {
                Notifications.SendNotification(new("Your friend has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetName().EnforceLength(12)), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.GetName().EnforceLength(12)}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
                return;
            }

            if (GFriends.IsVerified(userId))
            {
                Notifications.SendNotification(new("A verified user has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetName().EnforceLength(12)), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.GetName().EnforceLength(12)}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
                return;
            }

            if (Significance.TryGetValue(player, out PlayerSignificance significance) && significance is FigureSignificance)
            {
                Notifications.SendNotification(new("A notable user has joined", player.GetName().EnforceLength(12), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.NickName.SanitizeName()}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
            }
        }

        public void OnPlayerLeft(NetPlayer player)
        {
            string userId = player.UserId;

            if (GFriends.IsFriend(userId))
            {
                Notifications.SendNotification(new("Your friend has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetName().EnforceLength(12)), 5, Sounds.notificationNegative));
            }
            else if (GFriends.IsVerified(userId))
            {
                Notifications.SendNotification(new("A verified user has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetName().EnforceLength(12)), 5, Sounds.notificationNegative));
            }
            else if (Significance.TryGetValue(player, out PlayerSignificance significance) && significance is FigureSignificance)
            {
                Notifications.SendNotification(new("A notable user has left", player.GetName().EnforceLength(12), 5, Sounds.notificationNegative));
            }

            EvaluatePlayer(player);
        }

        public void OnQuestCompleted(RotatingQuestsManager.RotatingQuest quest)
        {
            Logging.Info($"Quest completed: {quest.GetTextDescription()}");
            Notifications.SendNotification(new("You completed a quest", quest.questName, 5, Sounds.notificationNeutral));
        }

        public void OnGetUserCosmetics(VRRig rig)
        {
            if (rig.Creator is not NetPlayer player || player.IsNull || player.IsLocal)
                return;

            if (EvaluatePlayer(player) && Significance.TryGetValue(player, out var significance) && significance is ItemSignificance item)
            {
                string userId = player.UserId;
                string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
                Notifications.SendNotification(new($"A notable cosmetic was detected", displayName, 5, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInfoPage), $"Inspect {player.NickName.SanitizeName()}", delegate ()
                {
                    player = Array.Find(NetworkSystem.Instance.PlayerListOthers, player => player.UserId == userId);
                    if (player != null && !player.IsNull)
                    {
                        PlayerInfoPage.RoomName = NetworkSystem.Instance.RoomName;
                        PlayerInfoPage.ActorNumber = player.ActorNumber;
                    }
                })));
            }
        }

        public bool EvaluatePlayer(NetPlayer player)
        {
            if (player is null || player.IsNull || (!player.IsLocal && !player.InRoom))
                return false;

            PlayerSignificance predicate = null;

            if (Main.Instance is not null && Array.Find(Main.Significance_Figures.ToArray(), figure => figure.IsValid(player)) is FigureSignificance figure)
                predicate = figure;
            else if (player.IsLocal || VRRigCache.rigsInUse.TryGetValue(player, out RigContainer playerRig) && playerRig.TryGetComponent(out NetworkedPlayer component) && component.HasInfoWatch)
                predicate = Main.Significance_Watch;
            else if (Main.Instance is not null && Array.Find(Main.Significance_Cosmetics.ToArray(), cosmetic => cosmetic.IsValid(player)) is ItemSignificance cosmetic)
                predicate = cosmetic;
            else if (GFriends.IsVerified(player.UserId))
                predicate = Main.Significance_Verified;

            if (predicate is not null)
            {
                if (!Significance.ContainsKey(player))
                {
                    Logging.Info($"Added significant player {player.NickName}: {predicate.Symbol}");
                    Significance.Add(player, predicate);
                    OnSignificanceChanged?.SafeInvoke(player, predicate);
                    return true;
                }

                if (Significance[player] != predicate)
                {
                    Logging.Info($"Changed significant player {player.NickName}: from {Significance[player].Symbol} to {predicate.Symbol}");
                    Significance[player] = predicate;
                    OnSignificanceChanged?.SafeInvoke(player, predicate);
                    return true;
                }
            }
            else if (Significance.ContainsKey(player))
            {
                Logging.Info($"Removed significant player {player.NickName}");
                Significance.Remove(player);
                OnSignificanceChanged?.SafeInvoke(player, null);
            }

            return false;
        }
    }
}
