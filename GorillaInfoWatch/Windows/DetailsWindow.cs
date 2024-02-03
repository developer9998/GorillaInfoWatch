using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Windows
{
    public class DetailsEntry : IEntry
    {
        public string Name => "Details";
        public Type Window => typeof(DetailsWindow);
    }

    public class DetailsWindow : Window
    {
        private readonly ItemHandler ItemHandler;
        private readonly TextInfo TextInfo;

        public DetailsWindow()
        {
            ItemHandler = new ItemHandler(5);
            TextInfo = new CultureInfo("en-US", false).TextInfo;
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine("- Details -".AlignCenter(Constants.Width)).AppendLine();

            try
            {
                switch (ItemHandler.CurrentEntry)
                {
                    case 0:
                        str.AppendLine("== General ==".AlignCenter(Constants.Width)).AppendLine();

                        str.Append("Version: ").AppendLine(GorillaComputer.instance.version);
                        str.Append("Player Count: ").AppendLine(PhotonNetwork.CountOfPlayers.ToString()).AppendLine();

                        str.Append("Region: ").Append(PhotonNetwork.CloudRegion.Replace("/*", "").ToUpper() switch
                        {
                            "US" => "United States (East)",
                            "USW" => "United States (West)",
                            "EU" => "Europe",
                            _ => "Unknown"
                        }).AppendLine();
                        str.Append("Ping: ").Append(PhotonNetwork.GetPing().ToString());
                        break;
                    case 1:
                        str.AppendLine("== Preferences ==".AlignCenter(Constants.Width)).AppendLine();

                        str.Append("Name: ").AppendLine(GorillaComputer.instance.savedName);
                        str.Append("Colour: [")
                            .Append(Mathf.FloorToInt(GorillaTagger.Instance.offlineVRRig.playerColor.r * 9f)).Append(", ")
                            .Append(Mathf.FloorToInt(GorillaTagger.Instance.offlineVRRig.playerColor.g * 9f)).Append(", ")
                            .Append(Mathf.FloorToInt(GorillaTagger.Instance.offlineVRRig.playerColor.b * 9f)).Append("]")
                            .AppendLine();
                        str.Append("Turning: ")
                            .Append(TextInfo.ToTitleCase(GorillaComputer.instance.GetField<string>("turnType").ToLower()))
                            .Append(" (").Append(GorillaComputer.instance.GetField<int>("turnValue")).Append(")").AppendLine();
                        str.Append("Microphone: ").AppendLine(TextInfo.ToTitleCase(GorillaComputer.instance.pttType.ToLower()));
                        str.Append("Queue: ").AppendLine(TextInfo.ToTitleCase(GorillaComputer.instance.currentQueue.ToLower()));
                        str.Append("Gamemode: ").AppendLine(TextInfo.ToTitleCase(GorillaComputer.instance.currentGameMode.ToLower()));

                        break;
                    case 2:
                        str.AppendLine("== Session ==".AlignCenter(Constants.Width)).AppendLine();

                        str.Append("Playtime: ")
                            .AppendLine(TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(@"h\:mm\:ss"))
                            .AppendLine();

                        str.Append("Tags: ").AppendLine(DataManager.GetItem("Tags", 0).ToString());
                        break;
                    case 3:
                        str.AppendLine("== Room ==".AlignCenter(Constants.Width)).AppendLine();
                        if (!PhotonNetwork.InRoom)
                        {
                            str.AppendLine("<color=red>It is required to be in a room to use the Room section.</color>");
                        }
                        else
                        {
                            str.Append("Room ID: ").AppendLine(PhotonNetwork.CurrentRoom.Name);
                            str.Append("Room Count: ").Append(PhotonNetwork.CurrentRoom.PlayerCount).Append("/").Append(PhotonNetwork.CurrentRoom.MaxPlayers).AppendLine().AppendLine();

                            str.Append("Master: ").AppendLine(PhotonNetwork.MasterClient.NickName);
                            str.Append("Visibility: ").AppendLine(PhotonNetwork.CurrentRoom.IsVisible ? "Public" : "Private");

                        }
                        break;
                    case 4:
                        str.AppendLine("== Gamemode ==".AlignCenter(Constants.Width)).AppendLine();
                        if (!PhotonNetwork.InRoom)
                        {
                            str.AppendLine("<color=red>It is required to be in a room to use the Gamemode section.</color>");
                        }
                        else
                        {
                            str.Append("Gamemode: ")
                                .AppendLine(GorillaGameManager.instance.GameType().ToString())
                                .AppendLine();

                            if (GameMode.ActiveGameMode == null) break;

                            if (GameMode.ActiveGameMode.GetType() == typeof(GorillaTagManager))
                            {
                                GorillaTagManager tagManager = GameMode.ActiveGameMode.GetComponent<GorillaTagManager>();
                                if (tagManager.isCurrentlyTag)
                                {
                                    str.AppendLine("State: Tag");
                                    str.Append("Tagger: ").AppendLine(tagManager.currentIt.NickName);
                                }
                                else
                                {
                                    str.AppendLine("State: Infection");
                                    str.Append("Infected: ").Append(tagManager.currentInfected.Count).Append("/").AppendLine(PhotonNetwork.CurrentRoom.PlayerCount.ToString());
                                }
                            }
                            else if (GameMode.ActiveGameMode.GetType() == typeof(GorillaHuntManager))
                            {
                                GorillaHuntManager huntManager = GameMode.ActiveGameMode.GetComponent<GorillaHuntManager>();
                                if (!huntManager.huntStarted && huntManager.waitingToStartNextHuntGame && huntManager.currentTarget.Contains(PhotonNetwork.LocalPlayer) && !huntManager.currentHunted.Contains(PhotonNetwork.LocalPlayer) && huntManager.countDownTime == 0)
                                {
                                    str.AppendLine("State: Victory").AppendLine();
                                    str.AppendLine("You won! Congrats, hunter!");
                                    break;
                                }
                                if (!huntManager.huntStarted && huntManager.countDownTime != 0)
                                {
                                    str.AppendLine("State: Starting").AppendLine();
                                    str.AppendLine("Game starting in:").Append(huntManager.countDownTime).Append("...");
                                    break;
                                }
                                if (!huntManager.huntStarted)
                                {
                                    str.AppendLine("State: Idle").AppendLine();
                                    str.AppendLine("Waiting to start");
                                    break;
                                }

                                Player Target = GorillaGameManager.instance.GetComponent<GorillaHuntManager>().GetTargetOf(PhotonNetwork.LocalPlayer);
                                if (huntManager.huntStarted && Target == null)
                                {
                                    str.AppendLine("State: Playing").AppendLine();
                                    str.AppendLine("You are dead! Tag others to slow them.").AppendLine();

                                    int hunterCount = PhotonNetwork.PlayerList.Where(player => huntManager.currentTarget.Contains(player) && !huntManager.currentHunted.Contains(player)).Count();
                                    str.Append("Hunters:").Append(hunterCount).Append("/").AppendLine(PhotonNetwork.CurrentRoom.PlayerCount.ToString());
                                    break;
                                }

                                PhotonView TargetRig = GorillaGameManager.instance.FindVRRigForPlayer(Target);
                                if (huntManager.huntStarted && TargetRig)
                                {
                                    str.AppendLine("State: Playing").AppendLine();
                                    str.Append("Target: ").AppendLine(Target.NickName);
                                    str.Append("Distance: ").AppendLine(Mathf.CeilToInt((GorillaLocomotion.Player.Instance.headCollider.transform.position - TargetRig.transform.position).magnitude).ToString()).AppendLine();

                                    int hunterCount = PhotonNetwork.PlayerList.Where(player => huntManager.currentTarget.Contains(player) && !huntManager.currentHunted.Contains(player)).Count();
                                    str.Append("Hunters:").Append(hunterCount).Append("/").AppendLine(PhotonNetwork.CurrentRoom.PlayerCount.ToString());
                                    break;
                                }
                            }
                        }
                        break;
                }
            }
            catch
            {

            }

            SetText(str);
        }

        public override void OnButtonPress(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Left:
                    ItemHandler.Change(-1);
                    break;
                case ButtonType.Right:
                    ItemHandler.Change(1);
                    break;
                case ButtonType.Back:
                    DisplayWindow<HomeWindow>();
                    return;
                default:
                    return;
            }

            OnScreenRefresh();
        }
    }
}
