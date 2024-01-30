using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models
{
    public class MenuDisplayInfo
    {
        public Text Text;
        public Image Background;

        public void SetText(string text) => Text.text = text;
        public void SetBackground(Color backgroundColour) => Background.color = backgroundColour;
    }
}
