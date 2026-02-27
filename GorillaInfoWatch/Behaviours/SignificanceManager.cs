using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Interfaces;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours;

// This code also handles notifications surrounding players (for friends and verified/notable users joining/leaving/etc)
public class SignificanceManager : MonoBehaviour, IInitializeCallback
{
    public static SignificanceManager Instance { get; private set; }
    public SignificanceVisibility Visibility { get; private set; }

    public static ReadOnlyCollection<FigureSignificance> Significance_Figures { get; private set; }
    public static ReadOnlyCollection<ItemSignificance> Significance_Cosmetics { get; private set; }
    public static PlayerSignificance Significance_Watch { get; private set; }
    public static PlayerSignificance Significance_Verified { get; private set; }
    public static PlayerSignificance Significance_Master { get; private set; }
    public static PlayerSignificance Significance_Friend { get; private set; }
    public static PlayerSignificance Significance_RecentlyPlayed { get; private set; }

    public event Action<NetPlayer, PlayerSignificance[]> OnSignificanceChanged;

    private readonly Dictionary<NetPlayer, PlayerSignificance[]> _significance = [];

    private readonly int _layerCount = Enum.GetValues(typeof(SignificanceLayer)).Length;

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
        RoomSystem.LeftRoomEvent += OnLeftRoom;

        Events.OnRigRecievedCosmetics += OnPlayerCosmeticsRecieved;
        Events.OnRigUpdatedCosmetics += OnPlayerCosmeticsUpdated;
        CosmeticsV2Spawner_Dirty.OnPostInstantiateAllPrefabs2 += () => OnPlayerCosmeticsUpdated(GorillaTagger.Instance.offlineVRRig);
    }

    public async void Initialize()
    {
        Significance_Figures = Content.Shared.Figures;
        Significance_Cosmetics = Content.Shared.Cosmetics;
        Significance_Watch = new("GorillaInfoWatch User", Content.Shared.Symbols["Info Watch"], "{0} is a user of GorillaInfoWatch");
        Significance_Verified = new("Verified", Content.Shared.Symbols["Verified"], "{0} is marked as verified by the GorillaFriends mod");
        Significance_Master = new("Master Client", null, "{0} is the host (specifically the master client) of the room");
        Significance_Friend = new("Friend", null, "You have {0} added as a friend (with GorillaFriends)");
        Significance_RecentlyPlayed = new("Recently Played", null, "You have played with {0} recently (via. GorillaFriends)");

        if (DataManager.Instance.HasData(Constants.DataEntry_Consent))
            SetVisibility((SignificanceVisibility)DataManager.Instance.GetData<int>(Constants.DataEntry_Consent), false);
        else
            SetVisibility(SignificanceVisibility.None);

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

    public bool CheckPlayer(NetPlayer player, SignificanceCheckScope checkScope)
    {
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (player.IsNull) throw new ArgumentException("NetPlayer is classified as null (NetPlayer.IsNull)", nameof(player));

        PlayerSignificance[] array = _significance.ContainsKey(player) ? [.. _significance[player]] : [.. Enumerable.Repeat<PlayerSignificance>(null, _layerCount)];

        if (checkScope.HasFlag(SignificanceCheckScope.RemovalCandidate))
        {
            if (_significance.ContainsKey(player))
            {
                Logging.Info($"Removed significant player {player.GetPlayerName()}");
                _significance.Remove(player);
                OnSignificanceChanged?.SafeInvoke(player, null);
            }

            return false;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Figure))
        {
            array[(int)SignificanceLayer.Figure] = UseVisibleSignificance(player, (Array.Find(Significance_Figures.ToArray(), figure => figure.IsValid(player)) is FigureSignificance figure) ? figure : null);
        }

        // Position 1 (array[1]) in array is used in PlayerInspectorScreen which may occupy the friend significance object in that particular circumstance
        // The array in that code however is duplicated from the array we work on in this code

        if (checkScope.HasFlag(SignificanceCheckScope.Item))
        {
            var tuple = Significance_Cosmetics.Select(cosmetic => Tuple.Create(cosmetic, cosmetic.GetState(player)));
            var element = tuple.FirstOrDefault(element => element.Item2 > ItemSignificance.ItemState.None);
            array[(int)SignificanceLayer.Item] = element != null ? ((element?.Item2 == ItemSignificance.ItemState.Allowed) ? UseVisibleSignificance(player, element.Item1) : element?.Item1) : null;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.InfoWatch))
        {
            array[(int)SignificanceLayer.InfoWatch] = PlayerUtility.HasInfoWatch(player) ? Significance_Watch : null;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Verified))
        {
            array[(int)SignificanceLayer.Verified] = FriendUtility.IsVerified(player.UserId) ? Significance_Verified : null;
        }

        // Add record if player doesn't have any
        if (!_significance.ContainsKey(player))
        {
            Logging.Message($"Added significant player {player.GetPlayerName()}");
            array.Select(element => element?.Title ?? "None").Select((element, index) => new { element, index }).ForEach(a => Logging.Info($"{(SignificanceLayer)a.index}: {a.element}"));
            _significance.Add(player, array);
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        // Update record if existing sequence isn't equal to proposed sequence
        if (!_significance[player].SequenceEqual(array))
        {
            Logging.Message($"Changed significant player {player.GetPlayerName()}");
            array.Select(element => element?.Title ?? "None").Select((element, index) => new { element, index }).ForEach(a => Logging.Info($"{(SignificanceLayer)a.index}: {a.element}"));
            _significance[player] = array;
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        return false;
    }

    #region Utilities (significance/consent)

    public bool GetSignificance(NetPlayer player, out PlayerSignificance[] significance) => _significance.TryGetValue(player, out significance);

    public void SetVisibility(SignificanceVisibility visibility, bool saveData = true)
    {
        Visibility = visibility;
        CheckPlayer(NetworkSystem.Instance.GetLocalPlayer(), SignificanceCheckScope.LocalPlayer);

        NetworkManager.Instance.SetProperty("Consent", (int)visibility);
        if (saveData) DataManager.Instance.SetData(Constants.DataEntry_Consent, (int)visibility);
    }

    public PlayerSignificance UseVisibleSignificance(NetPlayer player, PlayerSignificance significance)
    {
        if (significance == null) return significance;
        SignificanceVisibility significanceVisibility = significance.Visibility;
        if (significanceVisibility == SignificanceVisibility.None) return significance;
        SignificanceVisibility playerVisibility = PlayerUtility.GetSignificanceVisibility(player);
        return (playerVisibility & significanceVisibility) == significanceVisibility ? significance : null;
    }

    #endregion

    #region Events

    public void OnPlayerCosmeticsRecieved(VRRig rig)
    {
        NetPlayer player = rig.Creator;
        if (player == null || player.IsNull || player.IsLocal) return;

        Logging.Message($"{player.GetPlayerName()} Cosmetics: {rig.rawCosmeticString}");

        if (CheckPlayer(player, SignificanceCheckScope.Item) && GetSignificance(player, out PlayerSignificance[] significance) && Array.Find(significance, item => item is ItemSignificance) is ItemSignificance item)
        {
            Logging.Info($"Special Cosmetic (OnPlayerCosmeticsRecieved): {item.Title}");

            string userId = player.UserId;
            string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
            Notifications.SendNotification(new($"A special cosmetic has been identified", displayName, 5, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.NickName.SanitizeName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
        }
    }

    public void OnPlayerCosmeticsUpdated(VRRig rig) => OnPlayerCosmeticsUpdated(rig, false);

    public void OnPlayerCosmeticsUpdated(VRRig rig, bool showNotification)
    {
        NetPlayer player = rig.isOfflineVRRig ? NetworkSystem.Instance.GetLocalPlayer() : rig.Creator;
        if (player == null || player.IsNull) return;

        bool result = CheckPlayer(player, SignificanceCheckScope.Item);
        if (showNotification && result && !player.IsLocal && GetSignificance(player, out PlayerSignificance[] significance) && Array.Find(significance, item => item is ItemSignificance) is ItemSignificance item)
        {
            Logging.Info($"Special Cosmetic (OnPlayerCosmeticsUpdated): {item.Title}");

            string userId = player.UserId;
            string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
            Notifications.SendNotification(new($"A special cosmetic has been identified", displayName, 5, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.NickName.SanitizeName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
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

        if (FriendUtility.IsFriend(userId) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Friend))
        {
            Notifications.SendNotification(new("Your friend has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(FriendUtility.FriendColour), player.GetPlayerName()), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.GetPlayerName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
            return;
        }

        if (FriendUtility.IsVerified(userId) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Verified))
        {
            Notifications.SendNotification(new("A verified user has joined", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(FriendUtility.VerifiedColour), player.GetPlayerName()), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.GetPlayerName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
            return;
        }

        if (GetSignificance(player, out PlayerSignificance[] significance) && significance.Any(item => item is FigureSignificance) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.ModSignificant))
        {
            Notifications.SendNotification(new("A notable user has joined", player.GetPlayerName(), 3f, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.GetPlayerName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
        }
    }

    public void OnPlayerLeft(NetPlayer player)
    {
        string userId = player.UserId;

        if (FriendUtility.IsFriend(userId) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Friend))
        {
            Notifications.SendNotification(new("Your friend has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(FriendUtility.FriendColour), player.GetPlayerName()), 5, Sounds.notificationNegative));
            goto CheckPlayer;
        }

        if (FriendUtility.IsVerified(userId) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.Verified))
        {
            Notifications.SendNotification(new("A verified user has left", string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(FriendUtility.VerifiedColour), player.GetPlayerName()), 5, Sounds.notificationNegative));
            goto CheckPlayer;
        }

        if (GetSignificance(player, out PlayerSignificance[] significance) && significance.Any(item => item is FigureSignificance) && Configuration.AllowedNotifcationSources.Value.HasFlag(NotificationSource.ModSignificant))
        {
            Notifications.SendNotification(new("A notable user has left", player.GetPlayerName(), 5, Sounds.notificationNegative));
            goto CheckPlayer;
        }

    CheckPlayer:
        CheckPlayer(player, SignificanceCheckScope.PlayerLeft);
    }

    public void OnLeftRoom()
    {
        NetPlayer[] significantPlayers = [.. _significance.Keys.Where(player => player == null || player.IsNull || !player.IsLocal)];

        for (int i = 0; i < significantPlayers.Length; i++)
        {
            NetPlayer player = significantPlayers[i];
            CheckPlayer(player, SignificanceCheckScope.PlayerLeft);
        }
    }


    #endregion
}