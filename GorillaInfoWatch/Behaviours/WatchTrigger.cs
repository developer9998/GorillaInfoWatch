using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchTrigger : MonoBehaviour
    {
        public GameObject Menu;
        public Configuration Config;
        public TweenExecution TweenExecution;

        private AudioSource AudioSource;

        private Vector3 OriginalScale;
        private TweenInfo AppearTween;

        private readonly float Debounce = 0.33f;
        private float TouchTime;

        private bool IsFacingUp => Vector3.Distance(Player.Instance.leftControllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(Player.Instance.headCollider.transform.forward, (transform.position - Player.Instance.headCollider.transform.position).normalized) > 0.64f;

        public void Start()
        {
            AudioSource = GetComponent<AudioSource>();

            Menu.SetActive(false);
            OriginalScale = Menu.transform.localScale;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsFacingUp && InView && other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) && !handIndicator.isLeftHand && Time.time > (TouchTime + Debounce))
            {
                TouchTime = Time.time;
                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

                if (!Menu.activeSelf)
                {
                    Menu.SetActive(true);
                    Menu.transform.localScale = Vector3.zero;

                    AppearTween = TweenExecution.ApplyTween(0f, 1f, 0.1f, AnimationCurves.EaseType.EaseOutCubic);
                    AppearTween.SetOnUpdated((float value) => Menu.transform.localScale = OriginalScale * value);
                    AppearTween.SetOnCompleted((float value) => Menu.transform.localScale = OriginalScale);
                }
                else
                {
                    Menu.SetActive(true);
                    Menu.transform.localScale = OriginalScale;

                    AppearTween = TweenExecution.ApplyTween(1f, 0f, 0.1f, AnimationCurves.EaseType.EaseInCubic);
                    AppearTween.SetOnUpdated((float value) => Menu.transform.localScale = OriginalScale * value);
                    AppearTween.SetOnCompleted((float value) => Menu.SetActive(false));
                }

                AudioSource.PlayOneShot(AudioSource.clip, 0.32f * Config.ActivationVolume.Value);
            }
        }
    }
}
