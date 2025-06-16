using GorillaInfoWatch.Behaviours.UI;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Models.Widgets
{
    public class SnapSlider : Widget
    {
        public int Value;

        public Action<int> Command;

        public int StartValue;

        public int EndValue;

        public Gradient Colour;

        public bool ReadOnly;

        public SnapSlider(Action<int> action, int start = 0, int end = 100)
        {
            Command = action;
            StartValue = start;
            EndValue = end;

            Colour = new Gradient();
            Colour.SetKeys
            (
                [new GradientColorKey(Color.HSVToRGB(0f / 360f, 64f / 100f, 73f / 100f), 0), new GradientColorKey(Color.HSVToRGB(128f / 255, 64f / 255, 73f / 255), 1)],
                [new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1)]
            );
        }

        public override void CreateObject(InfoWatchLine menuLine)
        {
            gameObject = UnityEngine.Object.Instantiate(menuLine.SnapSlider.gameObject, menuLine.SnapSlider.transform.parent);

            gameObject.name = "SnapSlider";
            gameObject.SetActive(true);
        }

        public override void ModifyObject()
        {
            if (gameObject.TryGetComponent(out SnapSliderComponent component))
                component.ApplySlider(this);
        }

        public override bool Equals(Widget widget)
        {
            if (widget is null)
                return false;

            if (widget is not SnapSlider widgetSnapSlider)
                return false;

            return Command.Target == widgetSnapSlider.Command.Target && Command.Method.Equals(widgetSnapSlider.Command.Method) && StartValue == widgetSnapSlider.StartValue && EndValue == widgetSnapSlider.EndValue;
        }
    }
}