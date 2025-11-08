using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;
using Player = GorillaLocomotion.GTPlayer;

namespace GorillaInfoWatch.Behaviours.UI
{
    public class Trigger : MonoBehaviour
    {
        public Panel Menu;

        public AudioSource AudioSource;

        private bool IsFacingUp => Vector3.Distance(Player.Instance.leftHand.controllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(Player.Instance.headCollider.transform.forward, (transform.position - Player.Instance.headCollider.transform.position).normalized) > 0.64f;

        private float touchTime;

        public void Start()
        {
            AudioSource = GetComponent<AudioSource>();

            Menu.SetActive(!XRUtility.IsXRSubsystemActive);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsFacingUp && InView && other.TryGetComponent(out HandIndicator handIndicator) && Time.realtimeSinceStartup > touchTime)
            {
                touchTime = Time.realtimeSinceStartup + 0.3f;

                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

                AudioSource.PlayOneShot(AudioSource.clip, 0.4f);

                if (Watch.LocalWatch is Watch watch && watch.MenuStateMachine.CurrentState is Menu_Notification subState && subState.notification is Notification notification)
                {
                    Notifications.OpenNotification(notification, true);
                    watch.MenuStateMachine.SwitchState(subState.previousState);

                    Menu.SetActive(true);
                    return;
                }

                Menu.SetActive(!Menu.Active);
            }
        }
    }
}