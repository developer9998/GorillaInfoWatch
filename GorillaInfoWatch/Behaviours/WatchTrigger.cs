using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.StateMachine;
using GorillaInfoWatch.Tools;
using GorillaLocomotion;
using UnityEngine;
using HandIndicator = GorillaTriggerColliderHandIndicator;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchTrigger : MonoBehaviour
    {
        public GameObject Menu;

        public AudioSource AudioSource;

        private bool IsFacingUp => Vector3.Distance(GTPlayer.Instance.leftControllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(GTPlayer.Instance.headCollider.transform.forward, (transform.position - GTPlayer.Instance.headCollider.transform.position).normalized) > 0.64f;

        private float touchTime;

        public void Start()
        {
            AudioSource = GetComponent<AudioSource>();

            Menu.SetActive(!ContextInfo.InVR);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsFacingUp && InView && other.TryGetComponent(out HandIndicator handIndicator) && Time.realtimeSinceStartup > touchTime)
            {
                touchTime = Time.realtimeSinceStartup + 0.3f;

                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

                AudioSource.PlayOneShot(AudioSource.clip, 0.4f);

                if (InfoWatch.LocalWatch is InfoWatch watch && watch.stateMachine.CurrentState is Menu_Notification subState && subState.notification is Notification notification)
                {
                    Events.OpenNotification(notification, true);
                    watch.stateMachine.SwitchState(subState.previousState);

                    Menu.SetActive(true);
                    return;
                }

                Menu.SetActive(!Menu.activeSelf);
            }
        }
    }
}