using System.Collections.Generic;

namespace GorillaInfoWatch.Models;

public interface ISectionLineProvider
{
    public IEnumerable<SectionLine> SectionLines { get; }
}