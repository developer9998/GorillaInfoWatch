using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaInfoWatch.Tabs
{
    public class ScoreboardEntry : IEntry
    {
        public string Name => "Scoreboard";
        public Type EntryType => typeof(ScoreboardTab);
    }

    public class ScoreboardTab : Tab
    {
        private readonly PageHandler<Player> PageHandler = new();

        public ScoreboardTab()
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
                    int index = i + (PageHandler.PageNumber() * PageHandler.EntriesPerPage);
                    VRRig rig = GorillaGameManager.StaticFindRigForPlayer(EntryCollection[i]);
                    if (rig)
                    {
                        str.AppendItem(string.Concat(EntryCollection[i].NickName, " [<color=#", ColorUtility.ToHtmlStringRGB(rig.playerColor), ">██</color>]"), index, PageHandler);
                    }
                }
            }

            SetText(str);
        }

        public override void OnButtonPress(ButtonType type)
        {
            if (PageHandler.HandleButton(type))
            {
                OnScreenRefresh();
                return;
            }

            switch (type)
            {
                case ButtonType.Enter:
                    Player player = PageHandler.Items[PageHandler.CurrentEntry];
                    if (!player.IsLocal)
                    {
                        VRRig rig = GorillaGameManager.StaticFindRigForPlayer(player);
                        DisplayTab(typeof(PlayerTab), new object[] { player, rig });
                    }
                    OnScreenRefresh();
                    break;
                case ButtonType.Back:
                    DisplayTab<MainTab>();
                    break;
            }
        }
    }
}
