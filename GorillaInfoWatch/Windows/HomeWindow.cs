using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaInfoWatch.Windows
{
    public class HomeWindow : Window
    {
        private PageHandler<IEntry> PageHandler = new();

        public void SetEntries(List<IEntry> entries)
        {
            entries = entries.Where(entry => entry.GetType().Assembly == typeof(Plugin).Assembly).ToList();
            entries.AddRange(entries.Where(entry => entry.GetType().Assembly != typeof(Plugin).Assembly));

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

            string header = string.Concat("- ", Constants.Name, " | ", Constants.Version, " -");
            str.AppendLine(header.AlignCenter(Constants.Width)).AppendLine("A mod by Dev and Luna".AlignCenter(Constants.Width)).AppendLine();

            if (PageHandler.Items.Count > 0)
            {
                List<IEntry> EntryCollection = PageHandler.GetItemsAtEntry();
                for (int i = 0; i < EntryCollection.Count; i++)
                {
                    int index = i + PageHandler.PageNumber() * PageHandler.EntriesPerPage;
                    str.AppendItem(EntryCollection[i].Name, index, PageHandler);
                }

                str.AppendFooter(string.Concat(" Page ", PageHandler.PageNumber() + 1, "/", PageHandler.PageCount()), EntryCollection.Count, PageHandler.EntriesPerPage);
            }

            SetText(str.ToString());
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
                    DisplayWindow(PageHandler.Items[PageHandler.CurrentEntry].Window);
                    return;
            }
        }
    }
}
