using System;
using GorillaInfoWatch.Behaviours;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class WidgetSnapSlider(int start = 0, int end = 100, int value = 0) : IWidget, IWidgetValue<int>, IWidgetObject
    {
        public EWidgetType WidgetType => EWidgetType.Interaction;

        public int Value { get; set; } = value;

        public object[] Parameters { get; set; }

        public Action<int, object[]> Command { get; set; }

        public int Start = start;

        public int End = end;

        public int ObjectFits(GameObject game_object)
        {
            if (game_object && game_object.name == "SnapSlider" && game_object.TryGetComponent(out SnapSlider slider))
            {
                return (slider.Widget != null && slider.Widget.Command.Target == Command.Target && slider.Widget.Command.Method == Command.Method && slider.Widget.Start == Start && slider.Widget.End == End) ? 2 : 1;
            }

            return 1;
        }

        public void CreateObject(MenuLine line, out GameObject game_object)
        {
            game_object = UnityEngine.Object.Instantiate(line.SnapSlider.gameObject, line.SnapSlider.transform.parent);
            game_object.name = "SnapSlider";
            game_object.SetActive(true);
        }

        public void ModifyObject(GameObject game_object)
        {
            if (game_object.TryGetComponent(out SnapSlider snapSlider))
            {
                snapSlider.ApplySlider(this);
            }
        }
    }
}