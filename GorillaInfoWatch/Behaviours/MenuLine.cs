using System;
using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Models;
using TMPro;
using UnityEngine;
using Button = GorillaInfoWatch.Behaviours.Widgets.Button;
using SnapSlider = GorillaInfoWatch.Behaviours.Widgets.SnapSlider;

namespace GorillaInfoWatch.Behaviours
{
    public class MenuLine : MonoBehaviour
    {
        public TMP_Text Text;
        public Button Button;
        public SnapSlider SnapSlider;
        public GameObject Symbol;

        private readonly List<GameObject> widget_objects = [];

        private List<IWidgetBehaviour> widget_behaviours = [];

        public void Awake()
        {
            Text = transform.Find("Text").GetComponent<TMP_Text>();
            Button = transform.Find("Grid/Button").AddComponent<Button>();
            Button.gameObject.SetActive(false);
            SnapSlider = transform.Find("Grid/Slider").AddComponent<SnapSlider>();
            SnapSlider.gameObject.SetActive(false);
            Symbol = transform.Find("Grid/Symbol").gameObject;
            Symbol.SetActive(false);
        }

        public void Build(ScreenLine line, bool set_widgets)
        {
            Text.text = line.Text;

            if (!set_widgets) return;

            IWidgetObject[] widgets = [.. Array.FindAll(line.Widgets.ToArray(), widget => widget is IWidgetObject).Select(widget => widget as IWidgetObject)];
            if (widgets.Length > 0 && widget_objects.Count < widgets.Length)
            {
                widget_objects.AddRange(Enumerable.Repeat<GameObject>(null, widgets.Length - widget_objects.Count));
            }

            for (int i = 0; i < widget_objects.Count; i++)
            {
                IWidgetObject widget = (i < widgets.Length) ? widgets[i] : null;
                if (widget == null)
                {
                    if (widget_objects[i] != null)
                    {
                        Destroy(widget_objects[i]);
                        widget_objects[i] = null;
                    }
                    continue;
                }

                int fits = widget.ObjectFits(widget_objects[i]);
                //Logging.Info($"{i} {widget.GetType().Name} fits = {fits}");

                if (fits < 2 && widget_objects[i] != null)
                {
                    Destroy(widget_objects[i]);
                    if (fits == 0) continue;
                }

                if (fits == 1)
                {
                    widget.CreateObject(this, out GameObject widget_object);
                    widget_objects[i] = widget_object;
                }

                if (widget is IWidgetBehaviour widget_behaviour && (bool)widget_objects[i])
                {
                    widget_behaviour.game_object = widget_objects[i];
                    widget_behaviour.Initialize(widget_objects[i]);
                    widget_behaviours.Add(widget_behaviour);
                }
                if (widget is not IWidgetBehaviour || (widget as IWidgetBehaviour).PerformNativeMethods)
                {
                    widget.ModifyObject(widget_objects[i]);
                }
            }

            var widgets_with_behaviour = Array.FindAll(widgets, widget => widget is IWidgetBehaviour).Select(widget => widget as IWidgetBehaviour);
            var list_removal_candidates = widget_behaviours.Where(behaviour => !widgets_with_behaviour.Contains(behaviour) || !(bool)behaviour.game_object);
            widget_behaviours = [.. widget_behaviours.Except(list_removal_candidates)];
        }

        public void Update()
        {
            if (widget_behaviours.Count > 0)
            {
                foreach (var behaviour in widget_behaviours)
                {
                    behaviour.InvokeUpdate();
                }
            }
        }
    }
}
