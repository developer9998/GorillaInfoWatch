using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Behaviours;

// This code also handles notifications surrounding players (for friends and verified/notable users joining/leaving/etc)
public class SignificanceManager : MonoBehaviour
{
    public static SignificanceManager Instance { get; private set; }

    public event Action<NetPlayer, PlayerSignificance[]> OnSignificanceChanged;

    public readonly Dictionary<NetPlayer, PlayerSignificance[]> Significance = [];

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
        Events.OnRigRecievedCosmetics += OnGetUserCosmetics;
        CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 += () => CheckPlayer(NetworkSystem.Instance.GetLocalPlayer(), SignificanceCheckScope.Item);
    }

    public async void Start()
    {
        if (NetworkSystem.Instance is not NetworkSystem netSys || PlayFabAuthenticator.instance is not PlayFabAuthenticator authenticator) return;

        await new WaitWhile(() =>
        {
            string playerId = authenticator.GetPlayFabPlayerId();
            return string.IsNullOrEmpty(playerId) && !netSys.WrongVersion;
        }).AsAwaitable();

        if (!netSys.WrongVersion)
        {
            Logging.Message("Local Player authenticated");

            netSys.UpdatePlayers();
            CheckPlayer(netSys.GetLocalPlayer(), SignificanceCheckScope.LocalPlayer);
        }
    }

    public void OnJoinedRoom()
    {
        NetPlayer[] playersInRoom = NetworkSystem.Instance.PlayerListOthers;
        playersInRoom.ForEach(player => CheckPlayer(player, SignificanceCheckScope.PlayerJoined));
    }

    public void OnPlayerJoined(NetPlayer player)
    {
        CheckPlayer(player, SignificanceCheckScope.PlayerJoined);

        if (player.IsLocal) // called for the local player when marked "InGame" / connected to a room
            return;

        string userId = player.UserId;

        if (GFriends.IsFriend(userId))
        {
            Notifications.SendNotification(new("Your friend has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetName().EnforcePlayerNameLength()), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.GetName().EnforcePlayerNameLength()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
            return;
        }

        if (GFriends.IsVerified(userId))
        {
            Notifications.SendNotification(new("A verified user has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetName().EnforcePlayerNameLength()), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.GetName().EnforcePlayerNameLength()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
            return;
        }

        if (Significance.TryGetValue(player, out PlayerSignificance[] significance) && significance.Any(item => item is FigureSignificance))
        {
            Notifications.SendNotification(new("A notable user has joined", player.GetName().EnforcePlayerNameLength(), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.NickName.SanitizeName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
        }
    }

    public void OnPlayerLeft(NetPlayer player)
    {
        string userId = player.UserId;

        if (GFriends.IsFriend(userId))
        {
            Notifications.SendNotification(new("Your friend has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrFriend), player.GetName().EnforcePlayerNameLength()), 5, Sounds.notificationNegative));
        }
        else if (GFriends.IsVerified(userId))
        {
            Notifications.SendNotification(new("A verified user has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(GFriends.m_clrVerified), player.GetName().EnforcePlayerNameLength()), 5, Sounds.notificationNegative));
        }
        else if (Significance.TryGetValue(player, out PlayerSignificance[] significance) && significance.Any(item => item is FigureSignificance))
        {
            Notifications.SendNotification(new("A notable user has left", player.GetName().EnforcePlayerNameLength(), 5, Sounds.notificationNegative));
        }

        CheckPlayer(player, SignificanceCheckScope.PlayerLeft);
    }

    public void OnGetUserCosmetics(VRRig rig)
    {
        NetPlayer player = rig.Creator ?? rig.OwningNetPlayer;
        if (player == null || player.IsNull || player.IsLocal || rig.isOfflineVRRig || rig.isLocal) return;

        CheckPlayer(player, SignificanceCheckScope.Item);

        // TODO: implement a kind of "consent" check that requires permission from player before sending notification

        /*
        if (CheckPlayer(player, SignificanceCheckScope.Item) && Significance.TryGetValue(player, out PlayerSignificance[] significance) && Array.Find(significance, item => item is ItemSignificance) is ItemSignificance item)
        {
            string userId = player.UserId;
            string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
            Notifications.SendNotification(new($"A notable cosmetic was detected", displayName, 5, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.NickName.SanitizeName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
        }
        */
    }

    public bool CheckPlayer(NetPlayer player, SignificanceCheckScope checkScope)
    {
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (player.IsNull) throw new ArgumentException("NetPlayer is classified as null (NetPlayer.IsNull)", nameof(player));

        PlayerSignificance[] array = Significance.ContainsKey(player) ? [.. Significance[player]] : [.. Enumerable.Repeat<PlayerSignificance>(null, 5)];

        if (checkScope.HasFlag(SignificanceCheckScope.RemovalCandidate) && Significance.ContainsKey(player) && !player.IsLocal && (!NetworkSystem.Instance.InRoom || !player.InRoom))
        {
            Logging.Info($"Removed significant player {player.GetName()}");
            Significance.Remove(player);
            OnSignificanceChanged?.SafeInvoke(player, null);
            return false;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Figure))
        {
            array[0] = (Array.Find(Main.Significance_Figures.ToArray(), figure => figure.IsValid(player)) is FigureSignificance figure) ? figure : null;
        }

        // Position 1 (array[1]) in array is used in PlayerInspectorScreen which may occupy the friend significance object in that particular circumstance
        // The array in that code however is duplicated from the array we work on in this code

        if (checkScope.HasFlag(SignificanceCheckScope.Item))
        {
            array[2] = (Array.Find(Main.Significance_Cosmetics.ToArray(), cosmetic => cosmetic.IsValid(player)) is ItemSignificance cosmetic) ? cosmetic : null;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.InfoWatch) && !array.Contains(Main.Significance_Watch) && PlayerUtility.HasInfoWatch(player))
        {
            array[3] = Main.Significance_Watch;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Verified))
        {
            array[4] = GFriends.IsVerified(player.UserId) ? Main.Significance_Verified : null;
        }

        // Add record if player doesn't have any
        if (!Significance.ContainsKey(player))
        {
            Logging.Message($"Added significant player {player.GetName()}");
            Logging.Info(string.Join(", ", array.Where(item => item != null).Select(item => item.Title)));
            Significance.Add(player, array);
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        // Update record if existing sequence isn't equal to proposed sequence
        if (!Significance[player].SequenceEqual(array))
        {
            Logging.Message($"Changed significant player {player.GetName()}");
            Logging.Info(string.Join(", ", array.Where(item => item != null).Select(item => item.Title)));
            Significance[player] = array;
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        return false;
    }
}