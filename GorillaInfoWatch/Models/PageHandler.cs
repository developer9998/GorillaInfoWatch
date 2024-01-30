using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class PageHandler<T> : ItemHandler
    {
        public List<T> Items = new();
        public int EntriesPerPage = 5;

        private List<T> CheckedItems = new();

        public List<T> GetItemsAtPage(int PageNumber)
        {
            EntryCount = Items.Count;

            if (CurrentEntry > Items.Count - 1) CurrentEntry = Items.Count - 1;

            if (CheckedItems.Count > Items.Count)
            {
                T item = CheckedItems[CurrentEntry];
                if (Items.Contains(item)) CurrentEntry = Items.IndexOf(item);
            }

            CheckedItems = Items;

            List<T> result = new();
            for (int i = PageNumber * EntriesPerPage; i < EntriesPerPage + PageNumber * EntriesPerPage; i++)
            {
                if (i > Items.Count - 1) break;
                result.Add(Items[i]);
            }

            return result;
        }

        public List<T> GetItemsAtEntry() => GetItemsAtEntry(CurrentEntry);
        public List<T> GetItemsAtEntry(int EntryNumber) => GetItemsAtPage(PageNumber(EntryNumber));

        public int PageNumber() => PageNumber(CurrentEntry);
        public int PageNumber(int EntryNumber) => Mathf.FloorToInt(EntryNumber / (float)EntriesPerPage);
        public int PageCount()
        {
            EntryCount = Items.Count;
            return Mathf.CeilToInt(Items.Count / (float)EntriesPerPage);
        }

        public override void Change(int Increment)
        {
            EntryCount = Items.Count;
            base.Change(Increment);
        }
    }
}
