using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaInfoWatch.Tabs
{
    public class MainTab : Tab
    {
        private PageHandler<IEntry> PageHandler = new();

        public void SetEntries(List<IEntry> entries)
        {
            PageHandler = new PageHandler<IEntry>()
            {
                Items = entries,
                EntriesPerPage = 8
            };

            OnScreenRefresh();
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();

            string header = string.Concat("- ", Constants.Name, " | ", DateTime.Now.ToString("h:mm tt"), " -");
            str.AppendLine(header.AlignCenter(Constants.Width)).AppendLine("A mod by Dev and Luna".AlignCenter(34)).AppendLine();

            if (PageHandler.Items.Count > 0)
            {
                List<IEntry> EntryCollection = PageHandler.GetItemsAtEntry();
                for (int i = 0; i < EntryCollection.Count; i++)
                {
                    int index = i + (PageHandler.PageNumber() * PageHandler.EntriesPerPage);
                    str.AppendItem(EntryCollection[i].Name, index, PageHandler);
                }

                str.Append(string.Concat(Enumerable.Repeat("\n", PageHandler.EntriesPerPage - EntryCollection.Count))).AppendLine();
                str.Append(string.Concat(" Page ", PageHandler.PageNumber() + 1, "/", PageHandler.PageCount()));
            }

            SetText(str.ToString());
        }

        public override void OnButtonPress(ButtonType type)
        {
            switch (type)
            {
                case ButtonType.Down:
                    PageHandler.Change(1);
                    break;
                case ButtonType.Up:
                    PageHandler.Change(-1);
                    break;
                case ButtonType.Enter:
                    DisplayTab(PageHandler.Items[PageHandler.CurrentEntry].EntryType);
                    return;
                default:
                    return;
            }

            OnScreenRefresh();
        }
    }
}
