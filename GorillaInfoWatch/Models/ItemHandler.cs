using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class ItemHandler
    {
        public int CurrentEntry = 0, EntryCount = 5;

        public ItemHandler() { }
        public ItemHandler(int entryCount) => EntryCount = entryCount;

        public bool HandleButton(InputType button)
        {
            switch (button)
            {
                case InputType.Up:
                    Change(-1);
                    return true;
                case InputType.Down:
                    Change(1);
                    return true;
                default:
                    return false;
            }
        }

        public virtual void Change(int Increment) => CurrentEntry = Mathf.Clamp(CurrentEntry + Increment, 0, EntryCount - 1);
    }
}
