using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Utilities;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Windows.Scoreboard
{
    public class ScoreboardEntry : IEntry
    {
        public string Name => "Scoreboard";
        public Type Window => typeof(ScoreboardWindow);
    }

    public class ScoreboardWindow : Window
    {
        private readonly PageHandler<Player> PageHandler = new();

        public ScoreboardWindow()
        {
            PageHandler = new()
            {
                EntriesPerPage = 10
            };
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();
            str.AppendLine("- Scoreboard -".AlignCenter(Constants.Width)).AppendLine();

            if (!PhotonNetwork.InRoom)
            {
                str.AppendLine("<color=red>It is required to be in a room to use the Scoreboard tab.</color>");
            }
            else if (GorillaGameManager.instance)
            {
                PageHandler.Items = GorillaGameManager.instance.currentPlayerArray.ToList();

                List<Player> EntryCollection = PageHandler.GetItemsAtEntry();
                for (int i = 0; i < EntryCollection.Count; i++)
                {
                    int index = i + PageHandler.PageNumber() * PageHandler.EntriesPerPage;
                    VRRig rig = GorillaGameManager.StaticFindRigForPlayer(EntryCollection[i]);
                    if (rig)
                    {
                        str.AppendItem(string.Concat("<color=#", ColorUtility.ToHtmlStringRGB(rig.playerColor), ">██</color> ", EntryCollection[i].NickName.GetFilteredName(), " ", rig.GetComponent<GorillaSpeakerLoudness>().IsSpeaking ? "◀⚟" : (RigCacheUtils.GetField<bool>(EntryCollection[i]) ? "◀×" : "  ")), index, PageHandler);
                    }
                }
            }

            SetText(str);
        }

        public override void OnButtonPress(InputType type)
        {
            if (PageHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            switch (type)
            {
                case InputType.Enter:
                    Player player = PageHandler.Items[PageHandler.CurrentEntry];
                    if (!player.IsLocal)
                    {
                        VRRig rig = GorillaGameManager.StaticFindRigForPlayer(player);
                        DisplayWindow(typeof(PlayerWindow), new object[] { player, rig });
                    }
                    OnScreenRefresh();
                    break;
                case InputType.Back:
                    DisplayWindow<HomeWindow>();
                    break;
            }
        }
    }
}
