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
        private PageHandler<IQuickAction> PageHandler = new();
        private Dictionary<IQuickAction, bool> QuickActionActivity;

        public QuickActionsWindow(List<IQuickAction> quickActions)
        {
            PageHandler = new()
            {
                EntriesPerPage = 10,
                Items = quickActions
            }; // love you dane!! youre so cute :3

            QuickActionActivity = quickActions.ToDictionary(key => key, value => value.InitialState);   
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
                    str.AppendItem(EntryCollection[i].Type == ActionType.Static ? EntryCollection[i].Name : string.Concat(EntryCollection[i].Name, " [", QuickActionActivity[EntryCollection[i]]  ? "<color=lime>E</color>" : "<color=red>D</color>", "]"), index, PageHandler);
                }

                str.Append(string.Concat(Enumerable.Repeat("\n", PageHandler.EntriesPerPage - EntryCollection.Count))).AppendLine();
                str.Append(string.Concat(" Page ", PageHandler.PageNumber() + 1, "/", PageHandler.PageCount()));
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
                    QuickActionActivity[QuickAction] = QuickAction.Type == ActionType.Static ? true : !QuickActionActivity[QuickAction];
                    QuickAction?.OnActivate?.Invoke(QuickActionActivity[QuickAction]);

                    OnScreenRefresh();
                    break;
            }
        }
    }
}
