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

        private readonly List<Widget_Base> currentWidgets = [];

        private readonly HashSet<Widget_Base> regularWidgets = [];

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
                List<Widget_Base> newWidgets = line.Widgets;

                if (newWidgets is null)
                {
                    Logging.Fatal("newWidgets is null!!! It should at least be an empty collection");
                    return;
                }

                int intake = newWidgets.Count - currentWidgets.Count;
                if (intake > 0)
                {
                    Logging.Info($"Extending widget list +{intake}");
                    currentWidgets.AddRange(Enumerable.Repeat<Widget_Base>(null, intake));
                    Logging.Info($"Total count of {currentWidgets.Count}");
                }

                for (int i = 0; i < currentWidgets.Count; i++)
                {
                    Widget_Base currentWidget = currentWidgets.ElementAtOrDefault(i);

                    Widget_Base newWidget = newWidgets.ElementAtOrDefault(i);

                    if (newWidget is null)
                    {
                        Logging.Info($"{i} : null widget");

                        if (currentWidget is not null)
                        {
                            Logging.Info("Clearing existing widget");
                            if (currentWidget.Object is not null)
                                Destroy(currentWidget.Object);
                            if (regularWidgets.Contains(currentWidget))
                            {
                                currentWidget.Behaviour_Disable();
                                regularWidgets.Remove(currentWidget);
                            }
                            currentWidgets[i] = null;
                        }

                        continue;
                    }

                    Logging.Info($"add {i} : {newWidget.GetType().Name}");
                    Logging.Info($"pos {i} : {(currentWidget is not null && currentWidget.Object is not null ? currentWidget.Object.name : "null widget/object")}");

                    bool equivalent = currentWidget is not null && newWidget.GetType() == currentWidget.GetType() && newWidget.Equals(currentWidget);
                    Logging.Info($"equivalent: {equivalent}");

                    if (!equivalent && currentWidget is not null && currentWidget.Object is not null)
                    {
                        Logging.Info("Clearing existing widget");
                        
                        if (regularWidgets.Contains(currentWidget))
                        {
                            currentWidget.Behaviour_Disable();
                            regularWidgets.Remove(currentWidget);
                        }

                        if (currentWidget.Object is not null)
                            Destroy(currentWidget.Object);
                    }
                    else if (equivalent)
                    {
                        newWidget.Object = currentWidget.Object;

                        if (regularWidgets.Contains(currentWidget))
                        {
                            currentWidget.Behaviour_Disable();
                            regularWidgets.Remove(currentWidget);
                        }
                    }
                   
                    currentWidgets[i] = newWidget;
                    currentWidget = newWidget;

                    Logging.Info("Updated current widget");

                    newWidget.Object_Construct(this);

                    if (newWidget.UseBehaviour)
                    {
                        newWidget.Behaviour_Enable();
                        regularWidgets.Add(currentWidget);
                    }

                    Logging.Info(equivalent ? "Recycled existing widget" : "Initialized new widget");

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
                if (regularWidgets.ElementAtOrDefault(i) is Widget_Base widget)
                    widget.Behaviour_Update();
                widgetIndex++;
            }
        }
    }
}
