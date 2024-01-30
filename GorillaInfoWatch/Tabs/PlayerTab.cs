using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Tabs
{
    public class PlayerTab : Tab
    {
        private ItemHandler ItemHandler;

        // Player
        private Player Player;

        // Behaviour
        private VRRig Rig;
        private AudioSource Speaker;
        private GorillaPlayerScoreboardLine Line;

        public override void OnTabDisplayed(object[] Parameters)
        {
            if (Parameters != null)
            {
                Player = (Player)Parameters[0];

                Rig = (VRRig)Parameters[1];
                Line = ScoreboardUtils.FindLine(Player);
                Speaker = Rig.GetField<AudioSource>("voiceAudio");

                ItemHandler = new ItemHandler(3);
            }
        }

        public override void OnScreenRefresh()
        {
            if (!PhotonNetwork.InRoom || !Utils.PlayerInRoom(Player.ActorNumber) || !Line)
            {
                DisplayTab<ScoreboardTab>();
                return;
            }

            StringBuilder str = new();
            str.AppendLine($"- Player -".AlignCenter(Constants.Width)).AppendLine();

            str.Append("Name: ")
                .Append(Player.NickName).AppendLine();
            str.Append("Colour: [<color=#")
                .Append(ColorUtility.ToHtmlStringRGB(Rig.playerColor)).Append(">")
                .Append(Mathf.FloorToInt(Rig.playerColor.r * 9f)).Append(", ")
                .Append(Mathf.FloorToInt(Rig.playerColor.g * 9f)).Append(", ")
                .Append(Mathf.FloorToInt(Rig.playerColor.b * 9f)).Append("</color>]")
                .AppendLine().AppendLine();

            str.AppendItem(Line.muteButton.isOn ? "Unmute" : "Mute", 0, ItemHandler);
            str.AppendItem(Line.reportButton.isOn ? "Reported" : "Report", 1, ItemHandler);
            str.AppendItem(string.Concat("Volume: [", AsciiUtils.Bar(10, Mathf.RoundToInt(Speaker.volume * 10)), "]"), 2, ItemHandler);

            if (Line.reportButton.isOn)
            {
                str.AppendLine().Append("<color=red>You have reported this player!</color>");
            }

            SetText(str.ToString());
        }

        public override void OnButtonPress(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Down:
                    ItemHandler.Change(1);
                    break;
                case ButtonType.Up:
                    ItemHandler.Change(-1);
                    break;
                case ButtonType.Left:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 2:
                            Speaker.volume = Mathf.Clamp(Speaker.volume - 0.2f, 0f, 1f);
                            break;
                    }
                    break;
                case ButtonType.Right:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 2:
                            Speaker.volume = Mathf.Clamp(Speaker.volume + 0.2f, 0f, 1f);
                            break;
                    }
                    break;
                case ButtonType.Enter:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 0:
                            Line.muteButton.isOn ^= true;
                            Line.PressButton(Line.muteButton.isOn, GorillaPlayerLineButton.ButtonType.Mute);
                            break;
                        case 1:
                            if (!Line.reportButton.isOn) DisplayTab(typeof(ReportTab), new object[] { Player, Line });
                            return;
                    }
                    break;
                case ButtonType.Back:
                    Player = null;
                    Rig = null;
                    DisplayTab<ScoreboardTab>();
                    return;
                default:
                    return;
            }

            OnScreenRefresh();
        }
    }
}
