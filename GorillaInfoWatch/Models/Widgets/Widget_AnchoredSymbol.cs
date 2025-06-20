using GorillaExtensions;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaInfoWatch.Models.Widgets
{
    public class Widget_AnchoredSymbol(Symbol symbol, float offset = 44) : Widget_Symbol(symbol)
    {
        public override bool UseBehaviour => true;

        public override void Behaviour_Enable()
        {
            image.enabled = true;

            LayoutElement layoutElement = image.gameObject.GetOrAddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            RectTransform rectTransform = image.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithX(offset).WithY(31.25f);
            rectTransform.localPosition = rectTransform.localPosition.WithZ(-1);
            // rect_tform.sizeDelta = new Vector2(90, 90);
        }
    }
}