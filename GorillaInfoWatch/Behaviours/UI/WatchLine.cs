using GorillaExtensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class WatchLine : MonoBehaviour
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
            SnapSlider = transform.Find("Grid/Slider").gameObject.GetOrAddComponent<SnapSlider>();
            Switch = transform.Find("Grid/Switch").gameObject.GetOrAddComponent<Switch>();
            Symbol = transform.Find("Grid/Symbol").gameObject;
        }

        public void Start()
        {
            Button.gameObject.SetActive(false);
            SnapSlider.gameObject.SetActive(false);
            Switch.gameObject.SetActive(false);
            Symbol.SetActive(false);
        }

        public void Build(InfoLine line, bool applyWidgets)
        {
            // Logging.Info($"Text: \"{line.Text}\"");
            // Logging.Info($"Apply Widgets: {applyWidgets}");

            Text.text = line.Text;

            if (applyWidgets)
            {
                List<Widget_Base> newWidgets = line.Widgets;

                if (newWidgets is null)
                {
                    //Logging.Fatal("newWidgets is null!!! It should at least be an empty collection");
                    return;
                }

                // Logging.Info($"Widgets: {string.Join(", ", newWidgets.Select(widget => widget.GetType().Name))}");

                int intake = newWidgets.Count - currentWidgets.Count;
                if (intake > 0)
                {
                    //Logging.Info($"Extending widget list +{intake}");
                    currentWidgets.AddRange(Enumerable.Repeat<Widget_Base>(null, intake));
                    //Logging.Info($"Total count of {currentWidgets.Count}");
                }

                for (int i = 0; i < currentWidgets.Count; i++)
                {
                    Widget_Base currentWidget = currentWidgets.ElementAtOrDefault(i);

                    Widget_Base newWidget = newWidgets.ElementAtOrDefault(i);

                    if (newWidget is null)
                    {
                        //Logging.Info($"{i} : null widget");

                        if (currentWidget is not null)
                        {
                            // Logging.Info("Clearing existing widget");

                            if (regularWidgets.Contains(currentWidget))
                            {
                                currentWidget.Behaviour_Disable();
                                regularWidgets.Remove(currentWidget);
                            }

                            if (currentWidget.gameObject is not null && currentWidget.gameObject)
                            {
                                Destroy(currentWidget.gameObject);
                                currentWidget.gameObject = null;
                            }

                            currentWidgets[i] = null;
                        }

                        continue;
                    }

                    bool equivalent = currentWidget != null && currentWidget.gameObject != null && currentWidget.gameObject && newWidget.GetType() == currentWidget.GetType();// && newWidget.Equals(currentWidget);

                    //Logging.Info($"add {i} : {newWidget.GetType().Name}");
                    //Logging.Info($"pos {i} : {(currentWidget != null && currentWidget.gameObject is not null && currentWidget.gameObject ? currentWidget.gameObject.name : "null widget/object")}: {equivalent}");

                    if (equivalent)
                    {
                        //Destroy(newWidget.gameObject);
                        newWidget.gameObject = currentWidget.gameObject;
                        //Logging.Info(newWidget.gameObject.name);
                        newWidget.gameObject.SetActive(true);

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
                        //Logging.Info("Not equivalent");

                        if (currentWidget is not null && currentWidget.gameObject is not null)
                        {
                            //Logging.Info("Clearing existing widget");

                            if (regularWidgets.Contains(currentWidget))
                            {
                                currentWidget.Behaviour_Disable();
                                regularWidgets.Remove(currentWidget);
                            }

                            if (currentWidget.gameObject is not null && currentWidget.gameObject)
                            {
                                Destroy(currentWidget.gameObject);
                                currentWidget.gameObject = null;
                            }
                        }

                        newWidget.Object_Construct(this);
                        //Logging.Info(newWidget.gameObject.name);
                        currentWidgets[i] = newWidget;
                        currentWidget = newWidget;
                        //Logging.Info("Updated current widget");

                        if (newWidget.UseBehaviour)
                        {
                            newWidget.Behaviour_Enable();
                            regularWidgets.Add(currentWidget);
                        }

                        //Logging.Info("Initialized new widget");
                    }

                    if (currentWidget.gameObject is null || !currentWidget.gameObject)
                    {
                        currentWidget.Object_Construct(this);
                    }

                    currentWidget.gameObject.SetActive(true);

                    if (currentWidget.AllowModification)
                    {
                        try
                        {
                            currentWidget.Object_Modify();
                        }
                        catch (Exception ex)
                        {
                            // Logging.Fatal($"Widget could not modify");
                            Logging.Error(ex);
                        }
                        finally
                        {
                            //Logging.Info("Modified widget");
                        }
                    }
                }
            }
        }

        public void Update()
        {
            if (regularWidgets.Count == 0)
            {
                widgetIndex = 0;
                return;
            }

            for (int i = 0; i < widgetsPerFrame; i++)
            {
                try
                {
                    if (regularWidgets.ElementAtOrDefault(i) is Widget_Base widget && widget.Enabled) widget.Behaviour_Update();
                }
                catch (Exception ex)
                {
                    // Logging.Fatal("Exception due to widget update");
                    Logging.Error(ex);
                }
                widgetIndex = (widgetIndex + 1) % regularWidgets.Count;
            }
        }
    }
}
