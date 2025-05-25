using System;
using GorillaInfoWatch.Models;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Widgets
{
    // TODO: make sliders based around a generic class, int for snap, float for smooth
    /// <summary>
    /// A snap slider, used commonly alongside a WidgetSnapSlider, though can be used down to the OnApplied action
    /// </summary>
    public class SnapSlider : MonoBehaviour
    {
        public Action OnApplied;

        public WidgetSnapSlider Widget;

        private BoxCollider collider;

        private Transform needle, min, max;

        private GorillaTriggerColliderHandIndicator index_finger;

        public static SnapSlider Current;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            needle = transform.Find("Button");
            min = transform.Find("Min");
            max = transform.Find("Max");
        }

        public void ApplySlider(WidgetSnapSlider widget)
        {
            if (Widget == widget && widget != null) return;

            // prepare transition
            if (Widget != null && string.Join("", Widget.Parameters ?? []) == string.Join("", widget.Parameters ?? []) && Widget.Command != null && widget.Command != null && Widget.Command.Target == widget.Command.Target && Widget.Command.Method == widget.Command.Method)
            {
                widget.Value = Widget.Value;
            }

            // apply transition
            Widget = widget;
            if (Widget != null)
            {
                gameObject.SetActive(true);
                OnApplied = () => Widget.Command?.Invoke(Widget.Value, Widget.Parameters ?? []);
                SetNeedlePosition();
                return;
            }

            gameObject.SetActive(false);
            OnApplied = null;
        }

        public void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && (index_finger == null || index_finger == component) && (Current == null || Current == this) && Time.realtimeSinceStartup > (Button.PressTime + 0.33f))
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float clampedPreciseValue = Mathf.Clamp01((local.z - min.localPosition.z) / (max.localPosition.z * 2f));
                int split = Mathf.Abs(Widget.Start - Widget.End);
                float oneDivSplit = 1f / split;
                int targetValue = (int)(Mathf.RoundToInt(clampedPreciseValue / oneDivSplit) * oneDivSplit * split);

                if (Widget.Value != targetValue)
                {
                    Widget.Value = targetValue;

                    OnApplied?.Invoke();

                    SetNeedlePosition();

                    if (index_finger != null) GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.2f, 0.02f);
                }

                if (index_finger == null)
                {
                    index_finger = component;
                    Current = this;
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.25f, 0.05f);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && index_finger == component)
            {
                index_finger = null;
                Current = null;
                Button.PressTime = Time.realtimeSinceStartup + 0.1f;
            }
        }

        private void SetNeedlePosition()
        {
            int split = Mathf.Abs(Widget.Start - Widget.End);
            needle.transform.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, Widget.Value / (float)split);
        }
    }
}
