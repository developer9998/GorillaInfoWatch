using System.Collections.Generic;

namespace GorillaInfoWatch.Models.Interfaces;

public interface ISectionLineProvider
{
    public IEnumerable<SectionLine> SectionLines { get; }
}