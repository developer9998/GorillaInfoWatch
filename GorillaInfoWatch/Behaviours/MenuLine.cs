using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
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

        private readonly List<Widget> currentWidgets = [];

        private readonly HashSet<Widget> regularWidgets = [];

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

        public void Build(ScreenLine line, bool applyWidgets)
        {
            Logging.Info($"Text: \"{line.Text}\" Apply Widgets: {applyWidgets}");
            Text.text = line.Text;

            if (applyWidgets)
            {
                List<Widget> newWidgets = line.Widgets;

                if (newWidgets is null)
                {
                    Logging.Fatal("newWidgets is null!!! It should at least be an empty collection");
                    return;
                }

                int intake = newWidgets.Count - currentWidgets.Count;
                if (intake > 0)
                {
                    Logging.Info($"Extending widget list +{intake}");
                    currentWidgets.AddRange(Enumerable.Repeat<Widget>(null, intake));
                    Logging.Info($"Total count of {currentWidgets.Count}");
                }

                for(int i = 0; i < currentWidgets.Count; i++)
                {
                    Logging.Debug(i);

                    Widget currentWidget = currentWidgets.ElementAtOrDefault(i);
                    Widget newWidget = newWidgets.ElementAtOrDefault(i);

                    if (newWidget is null)
                    {
                        Logging.Info($"{i} : null widget");

                        if (currentWidget is not null)
                        {
                            Logging.Info("Clearing existing widget");
                            if (currentWidget.gameObject is not null)
                                Destroy(currentWidget.gameObject);
                            if (regularWidgets.Contains(currentWidget))
                                regularWidgets.Remove(currentWidget);
                            currentWidgets[i] = null;
                        }

                        continue;
                    }

                    bool equivalent = currentWidget is not null && newWidget.Equals(currentWidget);

                    Logging.Info($"{i} : {newWidget.GetType().Name} with {((newWidget is not null && newWidget.gameObject is not null) ? newWidget.gameObject.name : "null object/widget")}: {equivalent}");

                    if (/*!equivalent*/true)
                    {
                        // Logging.Info("Not equivalent");

                        if (currentWidget is not null && currentWidget.gameObject is not null)
                        {
                            Logging.Info("Clearing existing widget");
                            if (currentWidget.gameObject is not null)
                                Destroy(currentWidget.gameObject);
                            if (regularWidgets.Contains(currentWidget))
                                regularWidgets.Remove(currentWidget);
                        }

                        newWidget.CreateObject(this);
                        currentWidgets[i] = newWidget;
                        currentWidget = newWidget;
                        Logging.Info("Updated current widget");

                        if (currentWidget.Init())
                        {
                            regularWidgets.Add(currentWidget);
                        }
                        Logging.Info("Initialized new widget");
                    }

                    if (currentWidget.AllowModification)
                    {
                        Logging.Info("Modifying widget");
                        currentWidget.ModifyObject();
                    }
                }
            }
        }

        public void Update()
        {
            if (regularWidgets.Count > 0)
            {
                foreach (var widget in regularWidgets)
                {
                    widget.Update();
                }
            }
        }
    }
}
