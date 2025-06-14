using GorillaInfoWatch.Models.Widgets;
using System;
using TMPro;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Widgets
{
    public class PushButtonComponent : MonoBehaviour
    {
        public Action OnPressed, OnReleased;

        public PushButton Widget;

        private BoxCollider collider;
        private MeshRenderer renderer;
        private TMP_Text text;
        private Color colour, bumped_colour;

        private bool bumped;

        public static float PressTime;

        public void Awake()
        {
            collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            gameObject.SetLayer(UnityLayer.GorillaInteractable);

            renderer = GetComponent<MeshRenderer>();
            renderer.materials[1] = new Material(renderer.materials[1]) { color = colour };

            text = GetComponentInChildren<TMP_Text>();

            colour = new Color32(191, 188, 170, 255);
            bumped_colour = new Color32(132, 131, 119, 255);
        }

        public void ApplyButton(PushButton widget)
        {
            // prepare transition
            if (Widget is not null)
            {
                bumped = false;
                renderer.materials[1].color = colour;
            }

            // apply transition

            if (widget is not null)
            {
                Widget = widget;
                gameObject.SetActive(true);
                if (text) text.text = "";
                OnPressed = () => Widget.Command?.Invoke(Widget.Parameters ?? []);
                renderer.materials[1].color = colour;
                return;
            }

            Widget = null;
            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;
        }

        public void LateUpdate()
        {
            if (bumped && Time.unscaledTime > PressTime)
            {
                bumped = false;
                renderer.materials[1].color = colour;
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
            renderer.materials[1].color = bumped ? bumped_colour : colour;
            Singleton<Main>.Instance.PressButton(this, component.isLeftHand);
            GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

            // additional func
            OnPressed?.Invoke();
        }
    }
}
