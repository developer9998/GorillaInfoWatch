using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI
{
    [RequireComponent(typeof(Collider)), DisallowMultipleComponent]
    internal class CustomPushButton : MonoBehaviour
    {
        public Action<bool> OnButtonPush;

        public float Debounce = 0.25f;

        public bool Active = true;

        public void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (!collider.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) || handIndicator.isLeftHand == Watch.LocalWatch.InLeftHand || (PushButton.PressTime > Time.realtimeSinceStartup - Debounce)) return;

            if (Active)
            {
                PushButton.PressTime = Time.realtimeSinceStartup;

                AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(handIndicator.isLeftHand);
                handPlayer.PlayOneShot(Main.EnumToAudio[Sounds.widgetButton], 0.2f);

                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);

                OnButtonPush?.SafeInvoke(handIndicator.isLeftHand);

                return;
            }

            PushButton.PressTime = Time.realtimeSinceStartup + Debounce - (GorillaTagger.Instance.tapHapticDuration / 2f);
            GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength * 2f, GorillaTagger.Instance.tapHapticDuration / 2f);
        }
    }
}
