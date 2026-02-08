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
using System.Linq;
using UnityEngine;
using GFriends = GorillaFriends.Main;

namespace GorillaInfoWatch.Behaviours;

// This code also handles notifications surrounding players (for friends and verified/notable users joining/leaving/etc)
public class SignificanceManager : MonoBehaviour, IInitialize
{
    public static SignificanceManager Instance { get; private set; }
    public PlayerConsent Consent { get; private set; }

    public event Action<NetPlayer, PlayerSignificance[]> OnSignificanceChanged;

    private readonly Dictionary<NetPlayer, PlayerSignificance[]> _significance = [];

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

    public void Initialize()
    {
        if (DataManager.Instance.HasData(Constants.DataEntry_Consent))
        {
            SetConsent((PlayerConsent)DataManager.Instance.GetData<int>(Constants.DataEntry_Consent), false);
            if (PlayFabAuthenticator.instance.GetPlayFabPlayerId() is string playerId && !string.IsNullOrEmpty(playerId)) CheckPlayer(NetworkSystem.Instance.GetLocalPlayer(), SignificanceCheckScope.LocalPlayer);
            return;
        }

        SetConsent(PlayerConsent.None);
    }

    public void OnJoinedRoom()
    {
        NetPlayer[] playersInRoom = NetworkSystem.Instance.PlayerListOthers;
        playersInRoom.ForEach(player => CheckPlayer(player, SignificanceCheckScope.PlayerJoined));
    }

    public void OnPlayerJoined(NetPlayer player)
    {
        CheckPlayer(player, SignificanceCheckScope.PlayerJoined);
    }

    public void OnPlayerLeft(NetPlayer player)
    {
        string userId = player.UserId;

        CheckPlayer(player, SignificanceCheckScope.PlayerLeft);
        NetworkedPlayer.RemoveTemporaryConsent(userId);
    }

    public void OnLeftRoom()
    {
        var playersInRoom = _significance.Keys.Where(player => player.IsNull || !player.IsLocal).ToArray();
        for (int i = 0; i < playersInRoom.Length; i++)
        {
            var player = playersInRoom[i];
            CheckPlayer(player, SignificanceCheckScope.PlayerLeft);
            NetworkedPlayer.RemoveTemporaryConsent(!player.IsNull ? player.UserId : string.Empty);
        }
    }

    public void OnGetUserCosmetics(VRRig rig)
    {
        NetPlayer player = rig.Creator ?? rig.OwningNetPlayer;
        if (player == null || player.IsNull || player.IsLocal || rig.isOfflineVRRig || rig.isLocal) return;

        if (CheckPlayer(player, SignificanceCheckScope.Item) && PlayerUtility.GetConsent(player).HasFlag(PlayerConsent.Item) && GetSignificance(player, out PlayerSignificance[] significance) && Array.Find(significance, item => item is ItemSignificance) is ItemSignificance item)
        {
            string userId = player.UserId;
            string displayName = CosmeticsController.instance.GetItemDisplayName(CosmeticsController.instance.GetItemFromDict(item.ItemId));
            Notifications.SendNotification(new($"A notable cosmetic was detected", displayName, 5, Sounds.notificationPositive, new Notification.ExternalScreen(typeof(PlayerInspectorScreen), $"Inspect {player.NickName.SanitizeName()}", delegate ()
            {
                player = PlayerUtility.GetPlayer(userId);
                if (player != null && !player.IsNull) PlayerInspectorScreen.UserId = player.UserId;
            })));
        }
    }

    public bool CheckPlayer(NetPlayer player, SignificanceCheckScope checkScope)
    {
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (player.IsNull) throw new ArgumentException("NetPlayer is classified as null (NetPlayer.IsNull)", nameof(player));

        PlayerSignificance[] array = _significance.ContainsKey(player) ? [.. _significance[player]] : [.. Enumerable.Repeat<PlayerSignificance>(null, 5)];

        if (checkScope.HasFlag(SignificanceCheckScope.RemovalCandidate) && _significance.ContainsKey(player) && !player.IsLocal)
        {
            Logging.Info($"Removed significant player {player.GetName()}");
            _significance.Remove(player);
            OnSignificanceChanged?.SafeInvoke(player, null);
            return false;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Figure))
        {
            array[0] = UseConsensualSignificance(player, (Array.Find(Main.Significance_Figures.ToArray(), figure => figure.IsValid(player)) is FigureSignificance figure) ? figure : null, PlayerConsent.Figure);
        }

        // Position 1 (array[1]) in array is used in PlayerInspectorScreen which may occupy the friend significance object in that particular circumstance
        // The array in that code however is duplicated from the array we work on in this code

        if (checkScope.HasFlag(SignificanceCheckScope.Item))
        {
            array[2] = UseConsensualSignificance(player, (Array.Find(Main.Significance_Cosmetics.ToArray(), cosmetic => cosmetic.IsValid(player)) is ItemSignificance cosmetic) ? cosmetic : null, PlayerConsent.Item);
        }

        if (checkScope.HasFlag(SignificanceCheckScope.InfoWatch))
        {
            array[3] = PlayerUtility.HasInfoWatch(player) ? Main.Significance_Watch : null;
        }

        if (checkScope.HasFlag(SignificanceCheckScope.Verified))
        {
            array[4] = GFriends.IsVerified(player.UserId) ? Main.Significance_Verified : null;
        }

        // Add record if player doesn't have any
        if (!_significance.ContainsKey(player))
        {
            Logging.Message($"Added significant player {player.GetName()}");
            Logging.Info(string.Join(", ", array.Where(item => item != null).Select(item => item.Title)));
            _significance.Add(player, array);
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        // Update record if existing sequence isn't equal to proposed sequence
        if (!_significance[player].SequenceEqual(array))
        {
            Logging.Message($"Changed significant player {player.GetName()}");
            Logging.Info(string.Join(", ", array.Where(item => item != null).Select(item => item.Title)));
            _significance[player] = array;
            OnSignificanceChanged?.SafeInvoke(player, array);
            return true;
        }

        return false;
    }

    #region Utilities (significance/consent)

    public bool GetSignificance(NetPlayer player, out PlayerSignificance[] significance) => _significance.TryGetValue(player, out significance);

    public void SetConsent(PlayerConsent consent, bool saveData = true)
    {
        Consent = consent;
        CheckPlayer(NetworkSystem.Instance.GetLocalPlayer(), SignificanceCheckScope.LocalPlayer);

        NetworkManager.Instance.SetProperty("Consent", (int)consent);
        if (saveData) DataManager.Instance.SetData(Constants.DataEntry_Consent, (int)consent);
    }

    public PlayerSignificance UseConsensualSignificance(NetPlayer player, PlayerSignificance value, PlayerConsent consentFlag)
    {
        PlayerConsent playerConsent = PlayerUtility.GetConsent(player);
        return (playerConsent & consentFlag) == consentFlag ? value : null;
    }

    #endregion
}