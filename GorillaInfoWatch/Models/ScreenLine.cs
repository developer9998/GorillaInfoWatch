using System.Collections.Generic;

namespace GorillaInfoWatch.Models
{
    public class ScreenLine(string text, params List<IWidget> widgets)
    {
        //public bool Visible = true;

        public string Text = text;

        public List<IWidget> Widgets = widgets != null && widgets.Count > 0 ? widgets : [];
    }
}