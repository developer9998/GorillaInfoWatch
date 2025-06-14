using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetSymbol(Symbol symbol) : Widget
    {
        public Symbol Value = symbol;

        internal Image image;

        public override void CreateObject(Behaviours.Line menuLine)
        {
            gameObject = Object.Instantiate(menuLine.Symbol, menuLine.Symbol.transform.parent);

            gameObject.name = "Symbol";
            gameObject.SetActive(true);

            if (gameObject.TryGetComponent(out image))
                image.enabled = true;
        }

        public override void ModifyObject()
        {
            if (image || gameObject.TryGetComponent(out image))
            {
                image.sprite = Value.Sprite;
                image.material = Value.Material;
                image.color = Value.Colour;
            }
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not WidgetSymbol widgetSymbol)
                return false;

            return Value.Sprite == widgetSymbol.Value.Sprite && Value.Material == widgetSymbol.Value.Material && Value.Colour == widgetSymbol.Value.Colour;
        }
    }
}