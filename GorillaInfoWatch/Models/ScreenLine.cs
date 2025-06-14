using GorillaInfoWatch.Models.Widgets;
using System.Collections.Generic;

namespace GorillaInfoWatch.Models
{
    public class ScreenLine(string text, params List<Widget> widgets)
    {
        //public bool Visible = true;

        public string Text = text;

        public List<Widget> Widgets = widgets != null && widgets.Count > 0 ? widgets : [];
    }
}