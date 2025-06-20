using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Widgets;
using System;
using UnityEngine;
using UnityEngine.UI;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class PushButton : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public Widget_PushButton Widget;

        [SerializeField]
        private BoxCollider collider;

        [SerializeField]
        private MeshRenderer renderer;

        [SerializeField]
        private Gradient colour = Gradients.Button;

        [SerializeField]
        private bool bumped;

        public static float PressTime;

        [SerializeField]
        private float? currentValue = null;

        [SerializeField]
        private float targetValue;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]);
            // TODO: use material property block
        }

        public void AssignWidget(Widget_PushButton widget)
        {
            if (widget is not null)
            {
                Widget = widget;
                gameObject.SetActive(true);

                currentValue ??= targetValue;
                colour = Widget.Colour ?? Gradients.Button;

                OnPressed = delegate ()
                {
                    Widget.Command?.Invoke(Widget.Parameters ?? []);
                };

                if (!currentValue.HasValue || currentValue.Value == targetValue)
                    renderer.materials[1].color = colour?.Evaluate(currentValue.Value) ?? Color.white;

                if (GetComponentInChildren<Image>(true) is Image image)
                {
                    image.gameObject.SetActive(widget.Symbol is not null);
                    if (image.gameObject.activeSelf)
                    {
                        image.sprite = widget.Symbol.Sprite;
                        image.material = widget.Symbol.Material;
                        image.color = widget.Symbol.Colour;
                    }
                }

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
            targetValue = 0;
        }

        public void Update()
        {
            if (!currentValue.HasValue || currentValue.Value != targetValue)
            {
                currentValue = Mathf.MoveTowards(currentValue.GetValueOrDefault(targetValue), targetValue, Time.deltaTime / 0.05f);
                float animatedValue = Mathf.Clamp01(AnimationCurves.EaseInSine.Evaluate(currentValue.Value));
                renderer.materials[1].color = colour.Evaluate(animatedValue);
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
            renderer.materials[1].color = colour.Evaluate(targetValue);

            Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            // additional func
            OnPressed?.Invoke();
        }
    }
}
