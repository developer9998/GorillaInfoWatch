using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    [RequireComponent(typeof(BoxCollider)), DisallowMultipleComponent]
    public class Switch : MonoBehaviour
    {
        public Action OnSwitchFlipped, OnReleased;

        public Widget_Switch currentWidget;

        [SerializeField, HideInInspector]
        private Transform needle, min, max;

        [SerializeField, HideInInspector]
        private MeshRenderer renderer;

        private bool bumped;

        private float? currentValue;
        private float targetValue;

        [SerializeField, HideInInspector]
        private Gradient colour = ColourPalette.CreatePalette(ColourPalette.Red.Evaluate(0), ColourPalette.Green.Evaluate(0));

        public void Awake()
        {
            needle = transform.Find("Button");
            min = transform.Find("Min");
            max = transform.Find("Max");

            renderer = needle.GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]);
            // TODO: use material property block
        }

        public void AssignWidget(Widget_Switch widget)
        {
            if (widget is not null)
            {
                currentWidget = widget;
                gameObject.SetActive(true);

                bumped = widget.Value;
                targetValue = Convert.ToInt32(widget.Value);
                currentValue ??= targetValue;

                colour = widget.Colour;

                OnSwitchFlipped = () =>
                {
                    currentWidget.Command?.Invoke(currentWidget.Value, currentWidget.Parameters ?? []);
                };

                needle.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, targetValue);
                renderer.materials[1].color = colour.Evaluate(targetValue);

                return;
            }

            currentWidget = null;
            gameObject.SetActive(false);
            currentValue = null;
            OnSwitchFlipped = null;
            OnReleased = null;
        }

        public void OnDisable()
        {
            currentValue = null;
            targetValue = currentWidget is not null ? Convert.ToInt32(currentWidget.Value) : 0;
        }

        public void Update()
        {
            if (!currentValue.HasValue || currentValue.Value != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue.GetValueOrDefault(targetValue), targetValue, Time.deltaTime / 0.25f);
                float animatedValue = AnimationCurves.EaseInOutSine.Evaluate(currentValue.Value);
                needle.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, animatedValue);
                renderer.materials[1].color = colour.Evaluate(animatedValue);
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Time.realtimeSinceStartup > PushButton.PressTime && collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && component.isLeftHand != InfoWatch.LocalWatch.InLeftHand)
            {
                PushButton.PressTime = Time.realtimeSinceStartup + 0.25f;

                if (currentWidget is not null && currentWidget.IsReadOnly)
                {
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration / 2f);
                    return;
                }

                bumped ^= true;
                targetValue = Convert.ToInt32(bumped);
                currentValue = currentValue.GetValueOrDefault(targetValue);
                Update();

                Singleton<Main>.Instance.PressSwitch(this, component.isLeftHand);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                if (currentWidget is not null) currentWidget.Value = bumped;
                OnSwitchFlipped?.Invoke();
            }
        }
    }
}
