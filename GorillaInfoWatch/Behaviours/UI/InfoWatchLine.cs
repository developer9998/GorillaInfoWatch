using GorillaExtensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using PushButton = GorillaInfoWatch.Behaviours.UI.PushButtonComponent;
using SnapSlider = GorillaInfoWatch.Behaviours.UI.SnapSliderComponent;
using Switch = GorillaInfoWatch.Behaviours.UI.SwitchComponent;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class InfoWatchLine : MonoBehaviour
    {
        public TMP_Text Text;
        public PushButton Button;
        public SnapSlider SnapSlider;
        public Switch Switch;
        public GameObject Symbol;

        private readonly List<Widget> currentWidgets = [];

        private readonly HashSet<Widget> regularWidgets = [];

        private readonly int widgetsPerFrame = 2;

        private int widgetIndex = 0;

        public void Awake()
        {
            Text = transform.Find("Text").GetComponent<TMP_Text>();
            Button = transform.Find("Grid/Button").gameObject.GetOrAddComponent<PushButton>();
            Button.gameObject.SetActive(false);
            SnapSlider = transform.Find("Grid/Slider").gameObject.GetOrAddComponent<SnapSlider>();
            SnapSlider.gameObject.SetActive(false);
            Switch = transform.Find("Grid/Switch").gameObject.GetOrAddComponent<Switch>();
            Switch.gameObject.SetActive(false);
            Symbol = transform.Find("Grid/Symbol").gameObject;
            Symbol.SetActive(false);
        }

        public void Build(ScreenLine line, bool applyWidgets)
        {
            // Logging.Info($"Text: \"{line.Text}\" Apply Widgets: {applyWidgets}");
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

                for (int i = 0; i < currentWidgets.Count; i++)
                {
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
                            {
                                currentWidget.Behaviour_Disable();
                                regularWidgets.Remove(currentWidget);
                            }
                            currentWidgets[i] = null;
                        }

                        continue;
                    }

                    bool equivalent = currentWidget is not null && newWidget.GetType() == currentWidget.GetType() && newWidget.Equals(currentWidget);

                    Logging.Info($"add {i} : {newWidget.GetType().Name}");
                    Logging.Info($"pos {i} : {(currentWidget is not null && currentWidget.gameObject is not null ? currentWidget.gameObject.name : "null widget/object")}: {equivalent}");

                    if (equivalent)
                    {
                        newWidget.gameObject = currentWidget.gameObject;

                        if (regularWidgets.Contains(currentWidget))
                        {
                            currentWidget.Behaviour_Disable();
                            regularWidgets.Remove(currentWidget);
                        }

                        currentWidgets[i] = newWidget;
                        currentWidget = newWidget;

                        newWidget.Object_Construct(this);

                        if (newWidget.UseBehaviour)
                        {
                            newWidget.Behaviour_Enable();
                            regularWidgets.Add(currentWidget);
                        }
                    }
                    else
                    {
                        Logging.Info("Not equivalent");

                        if (currentWidget is not null && currentWidget.gameObject is not null)
                        {
                            Logging.Info("Clearing existing widget");
                            if (currentWidget.gameObject is not null)
                                Destroy(currentWidget.gameObject);
                            if (regularWidgets.Contains(currentWidget))
                            {
                                currentWidget.Behaviour_Disable();
                                regularWidgets.Remove(currentWidget);
                            }
                        }

                        newWidget.Object_Construct(this);
                        currentWidgets[i] = newWidget;
                        currentWidget = newWidget;
                        Logging.Info("Updated current widget");

                        if(newWidget.UseBehaviour)
                        {
                            newWidget.Behaviour_Enable();
                            regularWidgets.Add(currentWidget);
                        }

                        Logging.Info("Initialized new widget");
                    }

                    if (currentWidget.AllowModification)
                    {
                        try
                        {
                            currentWidget.Object_Modify();
                        }
                        catch (Exception ex)
                        {
                            Logging.Fatal($"Widget could not modify");
                            Logging.Error(ex);
                        }
                        finally
                        {
                            Logging.Info("Modified widget");
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (regularWidgets.Count == 0)
                return;

            for(int i = 0; i < widgetsPerFrame; i++)
            {
                if (widgetIndex >= regularWidgets.Count)
                    widgetIndex = 0;
                regularWidgets.ElementAt(i).Behaviour_Update();
                widgetIndex++;
            }
        }
    }
}
