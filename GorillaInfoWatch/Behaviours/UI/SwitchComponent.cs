using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    [RequireComponent(typeof(BoxCollider)), DisallowMultipleComponent]
    public class SwitchComponent : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        private Widget_Switch currentWidget;

        private Transform needle, min, max;

        private MeshRenderer renderer;

        //private Color unpressedColour, pressedColour;

        private bool bumped;

        private float currentValue = -1, targetValue;

        public void Awake()
        {
            //unpressedColour = new Color32(191, 188, 170, 255);
            //pressedColour = new Color32(132, 131, 119, 255);

            needle = transform.Find("Button");
            min = transform.Find("Min");
            max = transform.Find("Max");

            renderer = needle.GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]);
        }

        public void SetWidget(Widget_Switch widget)
        {
            if (widget != null && currentWidget == widget) return;

            // prepare transition
            if (currentWidget != null)
            {
                currentWidget.Value = false;
                //renderer.materials[1].color = unpressedColour;
            }

            // apply transition
            currentWidget = widget;
            if (currentWidget != null)
            {
                gameObject.SetActive(true);
                OnPressed = () => currentWidget.Command?.Invoke(currentWidget.Value, currentWidget.Parameters ?? []);
                bumped = widget.Value;

                // currentValue = bumped ? 1 : 0;
                targetValue = bumped ? 1 : 0;
                if (currentValue == -1)
                {
                    currentValue = targetValue;
                    needle.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, currentValue);
                    renderer.materials[1].color = currentWidget.Colour.Evaluate(currentValue);
                }

                // needle.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, currentValue);
                // renderer.materials[1].color = currentWidget.Colour.Evaluate(currentValue);

                return;
            }

            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void Update()
        {
            if (currentValue != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue, targetValue, Time.deltaTime / 0.15f);
                float animatedValue = AnimationCurves.EaseInOutSine.Evaluate(currentValue);
                needle.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, animatedValue);
                renderer.materials[1].color = currentWidget.Colour.Evaluate(animatedValue);
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (currentWidget is null)
                return;

            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && Time.realtimeSinceStartup > PushButtonComponent.PressTime)
            {
                PushButtonComponent.PressTime = Time.realtimeSinceStartup + 0.25f;

                // base functionality
                bumped ^= true;
                targetValue = bumped ? 1 : 0;

                Singleton<Main>.Instance.PressSwitch(this, component.isLeftHand);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                // additional func
                if (currentWidget != null)
                {
                    currentWidget.Value = bumped;
                }
                OnPressed?.Invoke();
            }
        }
    }
}
