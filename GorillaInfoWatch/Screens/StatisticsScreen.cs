using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Widgets;
using GorillaNetworking;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Screens
{
    [ShowOnHomeScreen, PreserveScreenSection(ClearContent = true)]
    internal class StatisticsScreen : InfoScreen
    {
        public override string Title => "Statistics";

        private int _gameModeRecordIndex;

        public override InfoContent GetContent()
        {
            LineBuilder lines = new();

            VRRig localRig = GorillaTagger.Instance.offlineVRRig;

            lines.Add($"Name: {localRig.playerNameVisible}");

            Color playerColour = localRig.playerColor;
            Color32 playerColour32 = playerColour;

            lines.Add(string.Format("Colour: [{0}, {1}, {2} / {3}, {4}, {5}]",
                Mathf.RoundToInt(playerColour.r * 9f),
                Mathf.RoundToInt(playerColour.g * 9f),
                Mathf.RoundToInt(playerColour.b * 9f),
                Mathf.RoundToInt(playerColour32.r),
                Mathf.RoundToInt(playerColour32.g),
                Mathf.RoundToInt(playerColour32.b)));

            var accountInfo = NetworkSystem.Instance.GetLocalPlayer().GetAccountInfo(result => SetContent());
            lines.Add($"Creation Date: {(accountInfo is null || accountInfo.AccountInfo?.TitleInfo?.Created is not DateTime created ? ". . ." : $"{created:d} at {created:t}")}");

            lines.Skip().Add($"Cosmetics: {CosmeticsController.instance.unlockedCosmetics.Count}");
            lines.Add($"Shiny Rocks: {CosmeticsController.instance.CurrencyBalance}");

            var gameModes = StatisticsManager.GameModeRecords;
            _gameModeRecordIndex = _gameModeRecordIndex.Wrap(0, gameModes.Count);

            lines.Skip().Add($"Selected Game Mode: {gameModes[_gameModeRecordIndex].DisplayName}", new Widget_PushButton(IncrementGameMode, -1)
            {
                Colour = ColourPalette.Red,
                Symbol = new Symbol(Symbols.Minus)
                {
                    Colour = Color.black
                }
            }, new Widget_PushButton(IncrementGameMode, 1)
            {
                Colour = ColourPalette.Green,
                Symbol = new Symbol(Symbols.Plus)
                {
                    Colour = Color.black
                }
            });

            var gameMode = gameModes[_gameModeRecordIndex];
            lines.Add($"Player Tags: {gameMode.TagCount}");
            lines.Add($"Tagged: {gameMode.TaggedCount}");
            lines.Add($"Rounds Played: {gameMode.RoundCount}");
            if (gameMode.GameManager.GetType() != typeof(GorillaTagManager)) lines.Skip().BeginCentre().AppendColour("Game Mode data may be inaccurate outside of Infection", Color.red).EndAlign().AppendLine();

            return lines;
        }

        private void IncrementGameMode(object[] parameters)
        {
            _gameModeRecordIndex += (int)parameters[0];
            SetContent();
        }
    }
}
