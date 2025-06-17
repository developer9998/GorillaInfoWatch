using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PushButton : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public Widget_PushButton Widget;

        private BoxCollider collider;
        private MeshRenderer renderer;
        private Material material;

        private Gradient colour;

        private bool bumped;

        public static float PressTime;

        private float? currentValue = null;
        private float targetValue;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            material = new Material(renderer.materials[1]);
            renderer.materials[1] = material;

            colour = Gradients.Button;
        }

        public void ApplyButton(Widget_PushButton widget)
        {
            if (widget is not null)
            {
                Widget = widget;
                gameObject.SetActive(true);
                OnPressed = delegate ()
                {
                    Widget.Command?.Invoke(Widget.Parameters ?? []);
                };

                colour = Widget.Colour ?? Gradients.Button;

                if (!currentValue.HasValue)
                    currentValue = targetValue;

                material.color = colour.Evaluate(currentValue.GetValueOrDefault(0));

                return;
            }

            Widget = null;
            currentValue = null;
            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void OnDisable()
        {
            currentValue = null;
        }

        public void Update()
        {
            if (currentValue.HasValue && currentValue != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue.Value, targetValue, Time.deltaTime / 0.1f);
                float animatedValue = Mathf.Clamp01(AnimationCurves.EaseInSine.Evaluate(currentValue.Value));
                material.color = colour.Evaluate(animatedValue);
            }

            if (bumped && Time.realtimeSinceStartup > PressTime)
            {
                bumped = false;
                targetValue = 0;
                OnReleased?.Invoke();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (PressTime > Time.realtimeSinceStartup || !collider.TryGetComponent(out HandIndicator component) || component.isLeftHand == InfoWatch.LocalWatch.InLeftHand)
                return;

            PressTime = Time.realtimeSinceStartup + 0.25f;

            // base functionality

            bumped = true;
            targetValue = 1;
            currentValue = 1;
            material.color = colour.Evaluate(targetValue);

            Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            // additional func
            OnPressed?.Invoke();
        }
    }
}
