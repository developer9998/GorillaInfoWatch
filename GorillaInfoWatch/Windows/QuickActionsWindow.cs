using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorillaInfoWatch.Windows
{
    public class QuickActionsEntry : IEntry
    {
        public string Name => "Quick Actions";
        public Type Window => typeof(QuickActionsWindow);
    }

    public class QuickActionsWindow : Window
    {
        private readonly PageHandler<IQuickAction> PageHandler = new();

        public QuickActionsWindow(List<IQuickAction> quickActions)
        {
            PageHandler = new()
            {
                EntriesPerPage = 10,
                Items = quickActions
            }; // love you dane!! youre so cute :3 (lunakitty)
        }

        public override void OnScreenRefresh()
        {
            StringBuilder str = new();

            str.AppendLine("- Quick Actions -".AlignCenter(Constants.Width)).AppendLine();

            if (PageHandler.Items.Count > 0)
            {
                List<IQuickAction> EntryCollection = PageHandler.GetItemsAtEntry();
                for (int i = 0; i < EntryCollection.Count; i++)
                {
                    int index = i + PageHandler.PageNumber() * PageHandler.EntriesPerPage;
                    str.AppendItem(EntryCollection[i].Name, index, PageHandler);
                }

                str.AppendFooter(string.Concat(" Page ", PageHandler.PageNumber() + 1, "/", PageHandler.PageCount()), EntryCollection.Count, PageHandler.EntriesPerPage);
            }

            SetText(str.ToString());
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
                    IQuickAction QuickAction = PageHandler.Items[PageHandler.CurrentEntry];
                    QuickAction.Function?.Invoke();
                    OnScreenRefresh();
                    break;
                case ButtonType.Back:
                    DisplayWindow<HomeWindow>();
                    break;
            }
        }
    }
}
