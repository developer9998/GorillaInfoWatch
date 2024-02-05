using GorillaInfoWatch.Tools;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchTrigger : MonoBehaviour
    {
        public GameObject Menu;
        public Configuration Config;

        private AudioSource AudioSource;

        private readonly float Debounce = 0.33f;
        private float TouchTime;

        private bool IsFacingUp => Vector3.Distance(Player.Instance.leftControllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(Player.Instance.headCollider.transform.forward, (transform.position - Player.Instance.headCollider.transform.position).normalized) > 0.7f;

        public void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            Menu.SetActive(false);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsFacingUp && InView && other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) && !handIndicator.isLeftHand && Time.time > (TouchTime + Debounce))
            {
                TouchTime = Time.time;
                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

                Menu.SetActive(!Menu.activeSelf);

                AudioSource.PlayOneShot(AudioSource.clip, 0.32f * Config.ActivationVolume.Value);
            }
        }
    }
}
