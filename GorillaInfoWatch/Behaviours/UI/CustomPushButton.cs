using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using System;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.UI;

[RequireComponent(typeof(Collider)), DisallowMultipleComponent]
internal class CustomPushButton : MonoBehaviour
{
    public Action<bool> OnButtonPush;

    public float Debounce = 0.25f;

    public bool Active = true;

    public void Awake()
    {
        gameObject.SetLayer(UnityLayer.GorillaInteractable);
        GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (!collider.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) || handIndicator.isLeftHand == Watch.LocalWatch.InLeftHand || Main.Instance.CheckInteractionInterval(WatchInteractionSource.Screen, Active ? Debounce : 0.05f)) return;

        if (Active)
        {
            AudioSource handPlayer = GorillaTagger.Instance.offlineVRRig.GetHandPlayer(handIndicator.isLeftHand);
            handPlayer.PlayOneShot(Sounds.widgetButton.AsAudioClip(), 0.2f);

            GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            OnButtonPush?.SafeInvoke(handIndicator.isLeftHand);

            return;
        }

        GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength * 2f, GorillaTagger.Instance.tapHapticDuration / 2f);
    }
}
