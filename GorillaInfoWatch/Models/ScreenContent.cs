using System.Collections.Generic;

namespace GorillaInfoWatch.Models
{
    public abstract class ScreenContent
    {
        public abstract int GetSectionCount();
        public abstract string GetTitleOfSection(int section);
        public abstract IEnumerable<ScreenLine> GetLinesAtSection(int section);
    }
}