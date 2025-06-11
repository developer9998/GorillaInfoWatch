using System;
using GorillaInfoWatch.Models.Widgets;
using GorillaInfoWatch.Tools;
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

        private bool bumped; //, toggle;

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
            Logging.Info($"Button {renderer is null}");
            Logging.Info($"{text is null} {renderer is null}");

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

                Logging.Info("show");
                return;
            }

            Widget = null;
            gameObject.SetActive(false);
            OnPressed = null;
            OnReleased = null;

            Logging.Info("hide");
        }

        public void LateUpdate()
        {
            if (bumped && Time.realtimeSinceStartup > (PressTime + 0.33f))
            {
                bumped = false;
                renderer.materials[1].color = colour;
                OnReleased?.Invoke();
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent(out GorillaTriggerColliderHandIndicator component) && !component.isLeftHand && Time.realtimeSinceStartup > (PressTime + 0.33f))
            {
                PressTime = Time.realtimeSinceStartup;

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
}
