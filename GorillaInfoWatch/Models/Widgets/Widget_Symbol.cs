using GorillaInfoWatch.Behaviours.UI;
using System;
using System.Linq;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_Symbol : Widget_Base
    {
        public override bool Modification => Settings != null;
        public override float Depth => 3f;

        public readonly Symbol Settings;

        internal Image image;

        private static readonly object[] emptyControllerParams = [];

        internal Widget_Symbol()
        {
            // Used alongside WidgetControllers
        }

        public Widget_Symbol(Symbol settings)
        {
            Settings = settings;
        }

        public override void Initialize(WatchLine menuLine)
        {
            if (Object == null || !Object)
            {
                Object = UnityEngine.Object.Instantiate(menuLine.Symbol, menuLine.Symbol.transform.parent);
                Object.name = "Symbol";
                Object.SetActive(true);
            }

            Object.TryGetComponent(out image);
        }

        public override void Modify()
        {
            if ((image == null || !image) && Object && !Object.TryGetComponent(out image))
            {
                // Logging.Info("NO");
                return;
            }

            image.enabled = true;
            image.sprite = Settings.Sprite;
            image.material = Settings.Material;
            image.color = Settings.Colour;


        }

        public override bool Equals(Widget_Base widget)
        {
            if (widget is null || widget is not Widget_Symbol widgetSymbol)
                return false;

            object[] myControllerParmas = ControllerParameters ?? emptyControllerParams;
            object[] otherControllerParams = widgetSymbol.ControllerParameters ?? emptyControllerParams;

            return /*((Settings == null && widgetSymbol.Settings == null) || (Settings != null && widgetSymbol.Settings != null && Settings.Equals(widgetSymbol.Settings))) &&*/ ControllerType == widgetSymbol.ControllerType && myControllerParmas.SequenceEqual(otherControllerParams);
        }
    }
}