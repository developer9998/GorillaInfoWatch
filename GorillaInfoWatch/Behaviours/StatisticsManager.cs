using GorillaGameModes;
using GorillaInfoWatch.Models.Interfaces;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utilla.Utils;

namespace GorillaInfoWatch.Behaviours;

public class StatisticsManager : MonoBehaviour, IInitializable
{
    public static ReadOnlyCollection<GameModeRecord> GameModeRecords;

    private Dictionary<GorillaGameManager, GameModeRecord> _gameModeRecords = [];

    public void Awake()
    {
        Events.OnPlayerTagged += OnPlayerTagged;
        Events.OnRoundComplete += OnRoundCompleted;
    }

    public void Initialize()
    {
        GameMode.GameModeZoneMapping.init();

        IEnumerable<GorillaGameManager> activeGameManagers = GameMode.GameModeZoneMapping.zoneGameModes
            .Where(element => !element.zone.Contains(GTZone.customMaps)).SelectMany(element => element.modes)
            .Distinct().Select(GameMode.GetGameModeInstance);

        activeGameManagers.ForEach(gameManager => GetRecord(gameManager));

        var standardList = GameMode.GameModeZoneMapping.GetModesForZone(GTZone.forest, false).Select(GameMode.GetGameModeInstance)
            .Select(gameManager => _gameModeRecords.SingleOrDefault(pair => pair.Key == gameManager))
            .Where(pair => pair.Value.IsTagBasedMode).ToList();

        var sortedList = _gameModeRecords.ToList();
        sortedList.Sort((pair1, pair2) => pair1.Value.DisplayName.CompareTo(pair2.Value.DisplayName));

        _gameModeRecords = standardList.Concat(sortedList.Where(pair => pair.Value.IsTagBasedMode).Except(standardList)).ToDictionary(pair => pair.Key, pair => pair.Value);
        GameModeRecords = _gameModeRecords.Values.ToList().AsReadOnly();
    }

    public GameModeRecord GetRecord(GorillaGameManager gameManager)
    {
        if (_gameModeRecords.TryGetValue(gameManager, out GameModeRecord record))
            return record;

        record = new(gameManager);
        _gameModeRecords.Add(gameManager, record);

        return record;
    }

    private void OnPlayerTagged(GorillaGameManager gameManager, NetPlayer taggedPlayer, NetPlayer taggingPlayer)
    {
        int tagIncrement = Convert.ToInt32(taggingPlayer.IsLocal);
        int taggedIncrement = Convert.ToInt32(taggedPlayer.IsLocal);
        GetRecord(gameManager).Increment(tagIncrement, taggedIncrement, 0);
    }

    // "I think I'm f-finished"
    // "Are you like, completed?"

    private void OnRoundCompleted(GorillaGameManager gameManager)
    {
        GetRecord(gameManager).Increment(0, 0, 1);
    }

    // "Everything is un-"
    // "Everything is unfin-"
    // "Everything is unfinished"

    // Only people named Dane and go by Dev online understand what is being referenced. Prove me wrong.

    public class GameModeRecord
    {
        public GorillaGameManager GameManager;

        public string DisplayName;

        public bool IsTagBasedMode;

        public int TagCount;

        public int TaggedCount;

        public int RoundCount;

        private readonly int _gameModeIndex;

        internal GameModeRecord(GorillaGameManager gameManager)
        {
            GameManager = gameManager;

            GameModeType gameModeType = gameManager.GameType();
            DisplayName = GameModeUtils.GetGameModeName(gameModeType);
            _gameModeIndex = (int)gameModeType;

            MethodInfo methodInfo = AccessTools.Method(gameManager.GetType(), nameof(GorillaGameManager.LocalCanTag));
            IsTagBasedMode = methodInfo != null && (methodInfo.IsDeclaredMember() || methodInfo.DeclaringType == typeof(GorillaTagManager));

            Load();
        }

        internal void Increment(int tags, int tagged, int plays)
        {
            if (tags == 0 && tagged == 0 && plays == 0) return;

            TagCount += tags;
            TaggedCount += tagged;
            RoundCount += plays;

            Save();
        }

        private void Load()
        {
            TagCount = DataManager.Instance.GetData($"GameMode{_gameModeIndex} Tags", defaultValue: 0, setDefaultValue: false);
            TaggedCount = DataManager.Instance.GetData($"GameMode{_gameModeIndex} Tagged", defaultValue: 0, setDefaultValue: false);
            RoundCount = DataManager.Instance.GetData($"GameMode{_gameModeIndex} Rounds", defaultValue: 0, setDefaultValue: false);
        }

        private void Save()
        {
            DataManager.Instance.SetData($"GameMode{_gameModeIndex} Tags", TagCount);
            DataManager.Instance.SetData($"GameMode{_gameModeIndex} Tagged", TaggedCount);
            DataManager.Instance.SetData($"GameMode{_gameModeIndex} Rounds", RoundCount);
        }
    }
}
