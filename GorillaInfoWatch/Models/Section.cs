using GorillaInfoWatch.Models.Interfaces;
using System.Collections.Generic;

namespace GorillaInfoWatch.Models;

public readonly struct Section(SectionDefinition definition, IEnumerable<SectionLine> lines)
{
    public SectionDefinition Definition { get; } = definition;
    public IEnumerable<SectionLine> Lines { get; } = lines;

    // line list constructors

    public Section(string title, IEnumerable<SectionLine> lines) : this(new SectionDefinition(title), lines)
    {

    }

    public Section(string title, string description, IEnumerable<SectionLine> lines) : this(new SectionDefinition(title, description), lines)
    {

    }

    public Section(IEnumerable<SectionLine> lines) : this(null, null, lines)
    {

    }

    // line provider constructors

    public Section(SectionDefinition definition, ISectionLines lineProvider) : this(definition, lineProvider.SectionLines)
    {

    }

    public Section(string title, ISectionLines lineProvider) : this(title, lineProvider.SectionLines)
    {

    }

    public Section(string title, string description, ISectionLines lineProvider) : this(title, description, lineProvider.SectionLines)
    {

    }

    public Section(ISectionLines lineProvider) : this(lineProvider.SectionLines)
    {

    }
}