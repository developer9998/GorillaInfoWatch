using System;
using GorillaInfoWatch.Models.Widgets;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Widgets
{
    [RequireComponent(typeof(BoxCollider)), DisallowMultipleComponent]
    public class SwitchComponent : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        private Switch currentWidget;

        private Transform needle, min, max;

        private MeshRenderer renderer;

        private TMP_Text text;

        private Color unpressedColour, pressedColour;

        private bool bumped;

        public static float PressTime;

        public void Awake()
        {
            unpressedColour = new Color32(191, 188, 170, 255);
            pressedColour = new Color32(132, 131, 119, 255);

            needle = transform.Find("Button");
            min = transform.Find("Min");
            max = transform.Find("Max");

            renderer = needle.GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]) { color = unpressedColour };

            text = needle.GetComponentInChildren<TMP_Text>();
        }

        public void SetWidget(Switch widget)
        {
            if (widget != null && currentWidget == widget) return;

            // prepare transition
            if (currentWidget != null)
            {
                currentWidget.Value = false;
                renderer.materials[1].color = unpressedColour;
            }

            // apply transition
            currentWidget = widget;
            if (currentWidget != null)
            {
                gameObject.SetActive(true);
                if (text) text.text = "";
                OnPressed = () => currentWidget.Command?.Invoke(currentWidget.Value, currentWidget.Parameters ?? []);
                bumped = widget.Value;
                renderer.materials[1].color = bumped ? pressedColour : unpressedColour;
                SetNeedlePosition();
                return;
            }

            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (currentWidget is null)
                return;

            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && Time.realtimeSinceStartup > (PressTime + 0.33f))
            {
                PressTime = Time.realtimeSinceStartup;

                // base functionality
                bumped ^= true;
                renderer.materials[1].color = bumped ? pressedColour : unpressedColour;
                SetNeedlePosition();

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

        private void SetNeedlePosition()
        {
            needle.localPosition = bumped ? max.localPosition : min.localPosition;
        }
    }
}
