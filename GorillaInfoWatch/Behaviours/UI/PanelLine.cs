using GorillaExtensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Easily the messiest class in the entire mod :3

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PanelLine : MonoBehaviour
    {
        public TMP_Text Text;

        public RectTransform container, containerLeftAlign, containerRightAlign;

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

            container = transform.Find("Widgets") as RectTransform;
            Symbol = container.Find("Symbol").gameObject;
            Button = container.Find("Button").gameObject.GetOrAddComponent<PushButton>();
            Switch = container.Find("Switch").gameObject.GetOrAddComponent<Switch>();
            SnapSlider = container.Find("Slider").gameObject.GetOrAddComponent<SnapSlider>();

            containerLeftAlign = transform.Find("LeftGrid") as RectTransform;
            containerRightAlign = transform.Find("RightGrid") as RectTransform;
        }

        public void Start()
        {
            Symbol.SetActive(false);
            Button.gameObject.SetActive(false);
            Switch.gameObject.SetActive(false);
            SnapSlider.gameObject.SetActive(false);
        }

        public void Build(InfoLine line, bool applyWidgets)
        {
            // Logging.Message(line.Text);

            //Logging.Info($"Text: \"{line.Text}\"");
            //Logging.Info($"Apply Widgets: {applyWidgets}");

            Text.text = line.Text;

            if (!applyWidgets) return;

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

            IEnumerable<Widget_Base> widgetsLeftAlign = newWidgets.Where(widget => widget.Alignment.Classification == "left");
            float marginLeft = widgetsLeftAlign.Sum(widget => widget.Width);
            marginLeft += widgetsLeftAlign.Any() ? 10 : 0;

            IEnumerable<Widget_Base> widgetsRightAlign = newWidgets.Where(widget => widget.Alignment.Classification == "right");
            float marginRight = widgetsRightAlign.Sum(widget => widget.Width);
            marginRight += widgetsRightAlign.Any() ? 10 : 0;

            Text.margin = new Vector4(marginLeft, 0f, marginRight, 0f);

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
                            if (currentWidget.Controller != null)
                            {
                                currentWidget.Controller.OnDisable();
                                // GC.SuppressFinalize(currentWidget.Controller);
                                currentWidget.Controller = null;
                            }

                            regularWidgets.Remove(currentWidget);
                        }

                        if (currentWidget.Object is not null && currentWidget.Object)
                        {
                            Destroy(currentWidget.Object);
                            currentWidget.Object = null;
                        }

                        currentWidgets[i] = null;
                    }

                    continue;
                }

                bool equivalent = currentWidget != null && currentWidget.Object != null && currentWidget.Object && newWidget.GetType() == currentWidget.GetType() && newWidget.Equals(currentWidget);

                // Logging.Info($"{i} : {newWidget.GetType().Name}");

                //Logging.Info($"pos {i} : {(currentWidget != null && currentWidget.gameObject is not null && currentWidget.gameObject ? currentWidget.gameObject.name : "null widget/object")}: {equivalent}");

                if (equivalent)
                {
                    //Logging.Info("Equivalent");

                    //Destroy(newWidget.gameObject);
                    newWidget.Object = currentWidget.Object;
                    newWidget.Controller = currentWidget.Controller;
                    newWidget.Controller?.Widget = newWidget;

                    //Logging.Info(newWidget.gameObject.name);
                    newWidget.Object.SetActive(true);

                    if (regularWidgets.Contains(currentWidget))
                    {
                        currentWidget.Controller?.OnDisable();
                        regularWidgets.Remove(currentWidget);
                    }

                    currentWidgets[i] = newWidget;
                    currentWidget = newWidget;

                    newWidget.Initialize(this);

                    newWidget.Object.transform.SetParent(newWidget.Alignment.Classification switch
                    {
                        "left" => containerLeftAlign,
                        "right" => containerRightAlign,
                        _ => container
                    });

                    if (newWidget.Alignment.Classification == "custom")
                    {
                        float width = container.sizeDelta.x;
                        if (newWidget.Object.transform is RectTransform rectTransform)
                        {
                            float offset = rectTransform.sizeDelta.x * rectTransform.localScale.x * 0.5f;
                            rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithX(Mathf.Lerp(offset, width - offset, newWidget.Alignment.HorizontalAnchor / 100f) + newWidget.Alignment.HorizontalOffset);
                            rectTransform.localPosition = rectTransform.localPosition.WithZ(newWidget.Controller != null ? newWidget.Controller.Depth.GetValueOrDefault(newWidget.Depth) : newWidget.Depth);
                        }
                    }

                    if (currentWidget.Controller != null)
                    {
                        newWidget.Controller?.OnEnable();
                        regularWidgets.Add(newWidget);
                    }
                }
                else
                {
                    //Logging.Info("Not equivalent");

                    if (currentWidget is not null && currentWidget.Object is not null)
                    {
                        //Logging.Info("Clearing existing widget");

                        if (regularWidgets.Contains(currentWidget))
                        {
                            if (currentWidget.Controller != null)
                            {
                                currentWidget.Controller.OnDisable();
                                currentWidget.Controller = null;
                            }
                            regularWidgets.Remove(currentWidget);
                        }

                        if (currentWidget.Object is not null && currentWidget.Object)
                        {
                            Destroy(currentWidget.Object);
                            currentWidget.Object = null;
                        }
                    }

                    newWidget.Initialize(this);

                    if (newWidget.ControllerType is Type controllerType && controllerType.IsSubclassOf(typeof(WidgetController)))
                    {
                        newWidget.Controller = newWidget.ControllerParameters is object[] parameters ? (WidgetController)Activator.CreateInstance(controllerType, args: parameters) : (WidgetController)Activator.CreateInstance(controllerType);
                        newWidget.Controller.Widget = newWidget;
                    }

                    newWidget.Object.transform.SetParent(newWidget.Alignment.Classification switch
                    {
                        "left" => containerLeftAlign,
                        "right" => containerRightAlign,
                        _ => container
                    });

                    if (newWidget.Alignment.Classification == "custom")
                    {
                        float width = container.sizeDelta.x;
                        if (newWidget.Object.transform is RectTransform rectTransform)
                        {
                            float offset = rectTransform.sizeDelta.x * rectTransform.localScale.x * 0.5f;
                            rectTransform.anchoredPosition = rectTransform.anchoredPosition.WithX(Mathf.Lerp(offset, width - offset, newWidget.Alignment.HorizontalAnchor / 100f) + newWidget.Alignment.HorizontalOffset);
                            rectTransform.localPosition = rectTransform.localPosition.WithZ(newWidget.Controller != null ? newWidget.Controller.Depth.GetValueOrDefault(newWidget.Depth) : newWidget.Depth);
                        }
                    }

                    //Logging.Info(newWidget.Object.name);
                    currentWidgets[i] = newWidget;
                    currentWidget = newWidget;
                    //Logging.Info("Updated current widget");

                    if (currentWidget.Controller != null)
                    {
                        newWidget.Controller.OnEnable();
                        regularWidgets.Add(currentWidget);
                    }

                    //Logging.Info("Initialized new widget");
                }

                if (currentWidget.Object is null || !currentWidget.Object)
                {
                    currentWidget.Initialize(this);
                }

                currentWidget.Object.SetActive(true);

                if (currentWidget.Controller != null ? currentWidget.Controller.Modification.GetValueOrDefault(currentWidget.Modification) : currentWidget.Modification)
                {
                    try
                    {
                        currentWidget.Modify();
                    }
                    catch (Exception ex)
                    {
                        // Logging.Fatal($"Widget could not modify");
                        Logging.Error(ex);
                    }
                    finally
                    {
                        Logging.Info("Modified widget");
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
                    Widget_Base widget = regularWidgets.ElementAtOrDefault(widgetIndex);
                    WidgetController controller = widget.Controller;
                    if (controller != null && controller.Enabled)
                    {
                        // Logging.Info($"#{widgetIndex}: {controller.GetType().Name}");
                        controller.Update();
                    }
                }
                catch (Exception)
                {

                }

                widgetIndex = (widgetIndex + 1) % regularWidgets.Count;
            }
        }
    }
}
