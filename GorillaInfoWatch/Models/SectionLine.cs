using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;

namespace GorillaInfoWatch.Models;

public class SectionLine(string text, LineOptions options, params List<Widget_Base> widgets)
{
    public string Text { get; } = text;

    public LineOptions Options { get; } = options;

    public List<Widget_Base> Widgets { get; } = widgets is not null ? widgets : [];

    public SectionLine(string text, params List<Widget_Base> widgets) : this(text, LineOptions.None, widgets)
    {

    }
}