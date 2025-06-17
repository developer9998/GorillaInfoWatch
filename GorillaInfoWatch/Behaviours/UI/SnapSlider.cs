using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    // TODO: make sliders based on a generic class, int for snap, float for smooth
    /// <summary>
    /// A snap slider, used commonly alongside a WidgetSnapSlider, though can be used down to the OnApplied action
    /// </summary>
    public class SnapSlider : MonoBehaviour
    {
        public Action OnApplied;

        public Models.Widgets.Widget_SnapSlider Widget;

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

        public void ApplySlider(Models.Widgets.Widget_SnapSlider widget)
        {
            if (Widget == widget && widget != null) return;

            // prepare transition
            if (Widget != null && Widget.Command != null && widget.Command != null && Widget.Command.Target == widget.Command.Target && Widget.Command.Method == widget.Command.Method)
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
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && (index_finger == null || index_finger == component) && (Current == null || Current == this) && Time.realtimeSinceStartup > PushButton.PressTime)
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float clampedPreciseValue = Mathf.Clamp01((local.z - min.localPosition.z) / (max.localPosition.z * 2f));
                int split = Mathf.Abs(Widget.StartValue - Widget.EndValue);
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
                PushButton.PressTime = Time.realtimeSinceStartup + 0.5f;
            }
        }

        private void SetNeedlePosition()
        {
            int split = Mathf.Abs(Widget.StartValue - Widget.EndValue);
            needle.transform.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, Widget.Value / (float)split);
        }
    }
}
