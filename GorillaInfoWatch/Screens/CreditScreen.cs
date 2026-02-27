using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Attributes;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Models.Widgets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace GorillaInfoWatch.Screens;

[ShowOnHomeScreen, PreserveSection]
internal class CreditScreen : InfoScreen
{
    public override string Title => "Credits";

    private readonly string creditFormat = "<line-height=45%>{0}<br><size=60%>{1}: {2}";

    private PageBuilder pageBuilder;

    private Dictionary<string, Supporter[]> _supporters;

    private readonly Dictionary<string, Texture2D> _avatars = [];

    public async void Awake()
    {
        using UnityWebRequest request = UnityWebRequest.Get(Constants.URL_Supporters);
        UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
        await asyncOperation.AsAwaitable();

        _supporters = request.result == UnityWebRequest.Result.Success ? JsonConvert.DeserializeObject<Dictionary<string, Supporter[]>>(request.downloadHandler.text) : null;

        _supporters?.SelectMany(section => section.Value).ForEach(async supporter =>
        {
            string avatar = supporter.Avatar;
            using UnityWebRequest texRequest = UnityWebRequest.Get(avatar);
            UnityWebRequestAsyncOperation texAsyncOperation = texRequest.SendWebRequest();
            await texAsyncOperation;
            Texture2D tex = new(128, 128, TextureFormat.RGB24, false)
            {
                filterMode = FilterMode.Point
            };
            tex.LoadImage(texRequest.downloadHandler.data);
            _avatars[avatar] = tex;
        });
    }

    public override void OnScreenLoad()
    {
        if (pageBuilder == null)
        {
            pageBuilder = new();

            ReadOnlyCollection<FigureSignificance> figures = SignificanceManager.Significance_Figures;

            LineBuilder developerLines = new();
            bool hasReadTester = false;

            foreach (FigureSignificance figure in figures)
            {
                if (string.IsNullOrEmpty(figure.Description) || string.IsNullOrWhiteSpace(figure.Description)) continue;

                string[] split = figure.Description.Split(": ");
                if (!hasReadTester && split.Last().ToLower().Contains("tester"))
                {
                    hasReadTester = true;
                    developerLines.Skip();
                }

                developerLines.Add(string.Format(creditFormat, figure.Title, split.First(), split.Last()), new Widget_Symbol(figure.Symbol)
                {
                    Alignment = WidgetAlignment.Left
                });
            }

            pageBuilder.Add(new SectionDefinition("", "These are the lead developers, contributors, and testers of GorillaInfoWatch"), developerLines);

            if (_supporters != null)
            {
                Random random = new(DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Year);

                LineBuilder generalSupporterLines = new();

                generalSupporterLines.BeginCentre().Append("Basic Tier - $5 CAD").EndAlign().AppendLine();

                Supporter[] basic = _supporters["Basic"];
                for (int i = 0; i < 5; i++)
                {
                    Supporter supporter = basic[random.Next() % basic.Length];

                    string platform = supporter.Platform switch
                    {
                        "kofi" => "Ko-fi",
                        "patreon" => "Patreon",
                        _ => "Unknown"
                    };

                    generalSupporterLines.Add(string.Format(creditFormat, supporter.DisplayName, supporter.Username, $"{platform} Supporter"), new Widget_Symbol(_avatars[supporter.Avatar])
                    {
                        Alignment = WidgetAlignment.Left
                    }, new Widget_Symbol(Content.Shared.Symbols[platform])
                    {
                        Alignment = WidgetAlignment.Right
                    });
                }

                generalSupporterLines.Skip().BeginCentre().Append("Dweller Tier - $12.50 CAD").EndAlign().AppendLine();

                Supporter[] dweller = _supporters["Dweller"];
                for (int i = 0; i < 5; i++)
                {
                    Supporter supporter = dweller[random.Next() % dweller.Length];

                    string platform = supporter.Platform switch
                    {
                        "kofi" => "Ko-fi",
                        "patreon" => "Patreon",
                        _ => "Unknown"
                    };

                    generalSupporterLines.Add(string.Format(creditFormat, supporter.DisplayName, supporter.Username, $"{platform} Supporter"), new Widget_Symbol(_avatars[supporter.Avatar])
                    {
                        Alignment = WidgetAlignment.Left
                    }, new Widget_Symbol(Content.Shared.Symbols[platform])
                    {
                        Alignment = WidgetAlignment.Right
                    });
                }

                pageBuilder.Add(new SectionDefinition("", "These are five random users of both the Basic and Dweller tiers"), generalSupporterLines);

                LineBuilder topSupporterLines = new();
                topSupporterLines.BeginCentre().Append("Prestige Tier - $25 CAD").EndAlign().AppendLine();

                Supporter[] prestige = _supporters["Prestige"];
                foreach (Supporter supporter in prestige)
                {
                    string platform = supporter.Platform switch
                    {
                        "kofi" => "Ko-fi",
                        "patreon" => "Patreon",
                        _ => "Unknown"
                    };

                    topSupporterLines.Add(string.Format(creditFormat, supporter.DisplayName, supporter.Username, $"{platform} Supporter"), new Widget_Symbol(_avatars[supporter.Avatar])
                    {
                        Alignment = WidgetAlignment.Left
                    }, new Widget_Symbol(Content.Shared.Symbols[platform])
                    {
                        Alignment = WidgetAlignment.Right
                    });
                }

                pageBuilder.Add(new SectionDefinition("", "These are all supporters of the Prestige tier"), topSupporterLines);
            }
        }
    }

    public override InfoContent GetContent() => pageBuilder;

    [Serializable]
    private class Supporter
    {
        [JsonProperty("display")]
        public string DisplayName { get; set; }

        [JsonProperty("user")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("origin")]
        public string Platform { get; set; }
    }
}