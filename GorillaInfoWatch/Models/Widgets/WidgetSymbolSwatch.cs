using GorillaExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class WidgetSymbolSwatch(Symbol symbol, float offset = 44) : WidgetSymbol(symbol)
    {
        public override bool UseBehaviour => true;

        public override void Behaviour_Enable()
        {
            image.enabled = true;

            if (image.GetComponent<LayoutElement>() is null)
            {
                LayoutElement layoutElement = image.gameObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.anchoredPosition3D = rectTransform.anchoredPosition3D.WithX(offset).WithY(31.25f);
                // rect_tform.sizeDelta = new Vector2(90, 90);
            }
        }
    }
}