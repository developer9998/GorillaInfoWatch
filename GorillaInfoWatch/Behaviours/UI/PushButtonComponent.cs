using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PushButtonComponent : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public PushButton Widget;

        private BoxCollider collider;
        private MeshRenderer renderer;

        private Gradient colour;
        // private Color colour, bumped_colour;

        private bool bumped;

        public static float PressTime;

        private float currentValue = -1, targetValue;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]);

            colour = Gradients.Button;
        }

        public void ApplyButton(PushButton widget)
        {
            // prepare transition
            if (Widget is not null)
            {
                bumped = false;
                renderer.materials[1].color = colour.Evaluate(0);
            }

            // apply transition

            if (widget is not null)
            {
                Widget = widget;
                gameObject.SetActive(true);
                OnPressed = () => Widget.Command?.Invoke(Widget.Parameters ?? []);
                colour = Widget.Colour ?? Gradients.Button;
                targetValue = bumped ? 1 : 0;
                if (currentValue == -1)
                {
                    currentValue = targetValue;
                    renderer.materials[1].color = colour.Evaluate(currentValue);
                }

                return;
            }

            Widget = null;
            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void LateUpdate()
        {
            if (currentValue != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue, targetValue, Time.deltaTime / 0.15f);
                float animatedValue = AnimationCurves.EaseInSine.Evaluate(currentValue);
                renderer.materials[1].color = colour.Evaluate(animatedValue);
            }

            if (bumped && Time.unscaledTime > PressTime)
            {
                bumped = false;
                targetValue = 0;
                OnReleased?.Invoke();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (PressTime > Time.realtimeSinceStartup || !collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) || component.isLeftHand == InfoWatch.LocalWatch.InLeftHand)
                return;

            PressTime = Time.realtimeSinceStartup + 0.25f;

            // base functionality

            bumped = true;
            targetValue = 1;
            currentValue = targetValue;
            renderer.materials[1].color = colour.Evaluate(targetValue);

            Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            // additional func
            OnPressed?.Invoke();
        }
    }
}
