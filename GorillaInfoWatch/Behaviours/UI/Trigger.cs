using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours.UI;

public class Trigger : MonoBehaviour
{
    public Panel panel;

    private AudioSource _audioDevice;

    private readonly float _debounce = 0.3f;

    public void Start()
    {
        _audioDevice = GetComponent<AudioSource>();

        if (!XRUtility.IsXRSubsystemActive) panel.SetActive(true);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (panel.Upright && panel.InView && other.TryGetComponent(out HandIndicator handIndicator) && Main.Instance.CheckInteractionInterval(WatchInteractionSource.Screen, _debounce))
        {
            GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

            _audioDevice.PlayOneShot(_audioDevice.clip, 0.4f);

            Watch localWatch = Watch.LocalWatch;

            if (localWatch.MenuStateMachine.CurrentState is Menu_Notification subState && subState.notification is Notification notification)
            {
                Notifications.OpenNotification(notification, true);
                localWatch.MenuStateMachine.SwitchState(subState.previousState);

                panel.SetActive(true);
                return;
            }

            panel.SetActive(!panel.Active);
        }
    }
}