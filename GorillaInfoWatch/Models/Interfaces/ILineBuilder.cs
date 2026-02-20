using System.Collections.Generic;

namespace GorillaInfoWatch.Models.Interfaces;

public interface ILineBuilder
{
    public IEnumerable<SectionLine> SectionLines { get; }
}