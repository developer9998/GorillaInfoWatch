using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class ItemHandler
    {
        public int CurrentEntry = 0, EntryCount = 5;

        public ItemHandler() { }
        public ItemHandler(int entryCount) => EntryCount = entryCount;

        public bool HandleButton(ButtonType button)
        {
            switch (button)
            {
                case ButtonType.Up:
                    Change(-1);
                    return true;
                case ButtonType.Down:
                    Change(1);
                    return true;
                default:
                    return false;
            }
        }

        public virtual void Change(int Increment) => CurrentEntry = Mathf.Clamp(CurrentEntry + Increment, 0, EntryCount - 1);
    }
}
