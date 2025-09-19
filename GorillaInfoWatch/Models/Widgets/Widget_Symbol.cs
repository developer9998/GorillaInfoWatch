using GorillaInfoWatch.Behaviours.UI;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Symbol : Widget_Base
    {
        public override bool Modify => Settings != null;
        public override float Depth => 3f;

        public readonly Symbol Settings;

        internal Image image;

        internal Widget_Symbol()
        {

        }

        public Widget_Symbol(Symbol settings)
        {
            Settings = settings;
        }

        public override void Object_Construct(WatchLine menuLine)
        {
            if (Object == null || !Object)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.Symbol, menuLine.Symbol.transform.parent);
                Object.name = "Symbol";
                Object.SetActive(true);
            }

            if (Object.TryGetComponent(out image))
                image.enabled = true;
        }

        public override void Object_Modify()
        {
            if (image || (Object && Object.TryGetComponent(out image)))
            {
                image.sprite = Settings.Sprite;
                image.material = Settings.Material;
                image.color = Settings.Colour;
            }
        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null)
                return false;

            if (widget is not Widget_Symbol widgetSymbol)
                return false;

            return Settings.Equals(widgetSymbol.Settings) && ControllerType == widgetSymbol.ControllerType;
        }
    }
}