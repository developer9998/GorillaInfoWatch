using System.Collections.Generic;

namespace GorillaInfoWatch.Models.Interfaces;

public interface ISectionLines
{
    public IEnumerable<SectionLine> SectionLines { get; }
}