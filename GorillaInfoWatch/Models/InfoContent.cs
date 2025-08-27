using System.Collections.Generic;

namespace GorillaInfoWatch.Models
{
    public abstract class InfoContent
    {
        public abstract int SectionCount { get; }
        public abstract string GetTitleOfSection(int section);
        public abstract IEnumerable<InfoLine> GetLinesAtSection(int section);
    }
}