using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class SnapSlider : MonoBehaviour
    {
        public Action OnApplied;

        public Widget_SnapSlider Widget;

        [SerializeField, HideInInspector]
        private BoxCollider collider;

        [SerializeField, HideInInspector]
        private MeshRenderer renderer;

        [SerializeField, HideInInspector]
        private Transform needle, min, max;

        private GorillaTriggerColliderHandIndicator index_finger;

        public static SnapSlider Current;

        [SerializeField, HideInInspector]
        private Gradient colour = Gradients.Button;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            needle = transform.Find("Button");

            renderer = needle.GetComponent<MeshRenderer>();
            renderer.materials[1] = new(renderer.materials[1]);
            // TODO: use material property block

            min = transform.Find("Min");
            max = transform.Find("Max");
        }

        public void ApplySlider(Widget_SnapSlider widget)
        {
            if (widget is not null)
            {
                Widget = widget;

                gameObject.SetActive(true);
                OnApplied = () => Widget.Command?.Invoke(Widget.Value, Widget.Parameters ?? []);
                colour = Widget.Colour ?? Gradients.Button;
                SetNeedlePosition();
                return;
            }

            Widget = null;
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
            float value = Widget.Value / (float)split;
            needle.transform.localPosition = Vector3.Lerp(min.localPosition, max.localPosition, value);
            renderer.materials[1].color = colour.Evaluate(value);
        }
    }
}
