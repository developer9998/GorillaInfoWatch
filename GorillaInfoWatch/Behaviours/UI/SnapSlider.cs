using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class SnapSlider : MonoBehaviour
    {
        public Action OnSliderAdjusted;

        public Widget_SnapSlider Widget;

        [SerializeField, HideInInspector]
        private BoxCollider collider;

        [SerializeField, HideInInspector]
        private MeshRenderer renderer;

        [SerializeField, HideInInspector]
        private Transform needle, min, max;

        private GorillaTriggerColliderHandIndicator currentHandIndicator;

        public static SnapSlider currentSnapSlider;

        [SerializeField, HideInInspector]
        private Gradient colour = ColourPalette.Button;

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
                OnSliderAdjusted = () => Widget.Command?.Invoke(Widget.Value, Widget.Parameters ?? []);
                colour = Widget.Colour ?? ColourPalette.Button;
                SetNeedlePosition();
                return;
            }

            Widget = null;
            gameObject.SetActive(false);
            OnSliderAdjusted = null;
        }

        public void OnTriggerStay(Collider other)
        {
            if
            (
                Time.realtimeSinceStartup > PushButton.PressTime
                && other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && component.isLeftHand != InfoWatch.LocalWatch.InLeftHand
                && (currentHandIndicator == null || currentHandIndicator == component) && (currentSnapSlider == null || currentSnapSlider == this)
            )
            {
                Vector3 local = transform.InverseTransformPoint(component.transform.position);
                float clampedPreciseValue = Mathf.Clamp01((local.z - min.localPosition.z) / (max.localPosition.z * 2f));
                int split = Mathf.Abs(Widget.StartValue - Widget.EndValue);
                float oneDivSplit = 1f / split;
                int targetValue = (int)(Mathf.RoundToInt(clampedPreciseValue / oneDivSplit) * oneDivSplit * split);

                if (Widget.Value != targetValue)
                {
                    Widget.Value = targetValue;

                    OnSliderAdjusted?.Invoke();

                    SetNeedlePosition();

                    if (currentHandIndicator is not null)
                    {
                        GorillaTagger.Instance.StartVibration(component.isLeftHand, 0.2f, 0.02f);
                        Main.Instance.PressSlider(this, component.isLeftHand);
                    }
                }

                if (currentHandIndicator is null)
                {
                    currentHandIndicator = component;
                    currentSnapSlider = this;
                    GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && currentHandIndicator == component)
            {
                currentHandIndicator = null;
                currentSnapSlider = null;
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
