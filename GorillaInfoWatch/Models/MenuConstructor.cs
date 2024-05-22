using GorillaInfoWatch.Behaviours;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = GorillaInfoWatch.Behaviours.Button;
using Slider = GorillaInfoWatch.Behaviours.Slider;

namespace GorillaInfoWatch.Models
{
    public class MenuConstructor
    {
        public Relations Relations;

        public Image Background;
        public TextMeshProUGUI Header;
        public Transform LineGrid;

        public readonly List<PhysicalLine> Lines = [];

        public void InitializeLines()
        {
            foreach (Transform line in LineGrid)
            {
                PhysicalLine physicalLine = line.AddComponent<PhysicalLine>();

                physicalLine.Text = physicalLine.GetComponent<TextMeshProUGUI>();
                physicalLine.Button = physicalLine.transform.Find("Grid/Button").AddComponent<Button>();
                physicalLine.Button.Relations = Relations;
                physicalLine.Slider = physicalLine.transform.Find("Grid/Slider").AddComponent<Slider>();
                physicalLine.Slider.Relations = Relations;
                physicalLine.SpeakIcon = physicalLine.transform.Find("Grid/OpenSpeaker").gameObject;
                physicalLine.MuteIcon = physicalLine.transform.Find("Grid/MutedSpeaker").gameObject;

                Lines.Add(physicalLine);

                physicalLine.gameObject.SetActive(false);
            }
        }

        public void ClearLines() => Lines.Do(line =>
        {
            line.gameObject.SetActive(false);

            line.Button.OnPressed = null;
            line.Player = null;
        });

        public void AddLine(object line)
        {
            if (line is not GenericLine genLine) return;

            PhysicalLine currentLine = Lines.FirstOrDefault(line => !line.gameObject.activeSelf);

            if (currentLine)
            {
                currentLine.gameObject.SetActive(true);
                if (line is PlayerLine playerLine) // only applies to player-lines
                {
                    currentLine.Player = playerLine.Player;
                    currentLine.Text.text = currentLine.PlayerText;
                }
                else // only applies to generic-lines
                {
                    currentLine.Text.text = genLine.Text;
                    currentLine.SpeakIcon.SetActive(genLine.Symbols.HasFlag(LineSymbol.Talk));
                    currentLine.MuteIcon.SetActive(genLine.Symbols.HasFlag(LineSymbol.Mute));
                }
                // applies to any line
                currentLine.Button.ApplyButton(genLine.Button);
                currentLine.Slider.ApplySlider(genLine.Slider);
            }
        }
    }
}
