using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;

namespace GorillaInfoWatch.Models;

public readonly struct SectionLine(string text, LineRestrictions restrictions, params List<Widget_Base> widgets)
{
    public string Text { get; } = text;

    public LineRestrictions Restrictions { get; } = restrictions;

    public List<Widget_Base> Widgets { get; } = widgets is not null ? widgets : [];

    public SectionLine(string text, params List<Widget_Base> widgets) : this(text, LineRestrictions.None, widgets)
    {

    }
}