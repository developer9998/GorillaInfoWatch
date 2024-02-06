using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Windows.Scoreboard
{
    public class PlayerWindow : Window
    {
        private readonly ItemHandler ItemHandler;
        private readonly Configuration Config;

        private Player Player;

        private VRRig Rig;
        private AudioSource Speaker;
        private GorillaPlayerScoreboardLine Line;

        public PlayerWindow(Configuration configuration)
        {
            Config = configuration;

            ItemHandler = new ItemHandler(4);
        }

        public override void OnWindowDisplayed(object[] Parameters)
        {
            if (Parameters != null)
            {
                Player = (Player)Parameters[0];

                Rig = (VRRig)Parameters[1];
                Line = ScoreboardUtils.FindLine(Player);
                Speaker = Rig.GetField<AudioSource>("voiceAudio");

                ItemHandler.CurrentEntry = 0;
            }
        }

        public override void OnScreenRefresh()
        {
            if (!PhotonNetwork.InRoom || !Utils.PlayerInRoom(Player.ActorNumber) || !Line)
            {
                DisplayWindow<ScoreboardWindow>();
                return;
            }

            StringBuilder str = new();
            str.AppendLine($"- Player -".AlignCenter(Constants.Width)).AppendLine();

            str.Append("Name: ")
                .Append(Player.NickName).AppendLine();
            str.Append("Colour: [<color=#")
                .Append(ColorUtility.ToHtmlStringRGB(Rig.playerColor)).Append(">")
                .Append(Mathf.RoundToInt(Rig.playerColor.r * 9f)).Append(", ")
                .Append(Mathf.RoundToInt(Rig.playerColor.g * 9f)).Append(", ")
                .Append(Mathf.RoundToInt(Rig.playerColor.b * 9f)).Append("</color>]")
                .AppendLine().AppendLine();

            str.AppendItem(Line.muteButton.isOn ? "Unmute" : "Mute", 0, ItemHandler);
            str.AppendItem(Line.reportButton.isOn ? "Reported" : "Report", 1, ItemHandler);
            str.AppendItem(DataManager.GetItem(string.Concat(Player.UserId, "fav"), false, DataType.Stored) ? "Unfavourite" : "Favourite", 2, ItemHandler);
            str.AppendItem(string.Concat("Volume: [", AsciiUtils.Bar(10, Mathf.RoundToInt(Speaker.volume * 10)), "]"), 3, ItemHandler);

            if (Line.reportButton.isOn)
            {
                str.AppendLine().Append("<color=red>You have reported this player!</color>");
            }

            SetText(str.ToString());
        }

        public override void OnButtonPress(InputType type)
        {
            if (ItemHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            switch (type)
            {
                case InputType.Left:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 3:
                            Speaker.volume = Mathf.Clamp(Speaker.volume - 0.2f, 0f, 1f);
                            DataManager.AddItem(string.Concat(Player.UserId, "_volume"), Speaker.volume, DataType.Stored);
                            break;
                    }
                    break;
                case InputType.Right:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 3:
                            Speaker.volume = Mathf.Clamp(Speaker.volume + 0.2f, 0f, 1f);
                            DataManager.AddItem(string.Concat(Player.UserId, "_volume"), Speaker.volume, DataType.Stored);
                            break;
                    }
                    break;
                case InputType.Enter:
                    switch (ItemHandler.CurrentEntry)
                    {
                        case 0:
                            Line.muteButton.isOn ^= true;
                            Line.PressButton(Line.muteButton.isOn, GorillaPlayerLineButton.ButtonType.Mute);
                            break;
                        case 1:
                            if (!Line.reportButton.isOn) DisplayWindow(typeof(ReportWindow), new object[] { Player, Line });
                            return;
                        case 2:
                            DataManager.AddItem(string.Concat(Player.UserId, "fav"), !DataManager.GetItem(string.Concat(Player.UserId, "fav"), false, DataType.Stored), DataType.Stored);

                            Rig.playerText.color = DataManager.GetItem(string.Concat(Player.UserId, "fav"), false, DataType.Stored) ? PresetUtils.Parse(Config.FavouriteColour.Value) : Color.white;
                            ScoreboardUtils.RedrawLines();
                            break;
                    }
                    break;
                case InputType.Back:
                    Player = null;
                    Rig = null;
                    DisplayWindow<ScoreboardWindow>();
                    return;
                default:
                    return;
            }

            OnScreenRefresh();
        }
    }
}
