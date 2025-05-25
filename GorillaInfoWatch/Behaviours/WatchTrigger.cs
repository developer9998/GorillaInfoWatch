using GorillaInfoWatch.Utilities;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchTrigger : MonoBehaviour
    {
        public GameObject Menu;

        private AudioSource AudioSource;

        private readonly float Debounce = 0.33f;
        private float TouchTime;

        private bool IsFacingUp => Vector3.Distance(GTPlayer.Instance.leftControllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(GTPlayer.Instance.headCollider.transform.forward, (transform.position - GTPlayer.Instance.headCollider.transform.position).normalized) > 0.64f;

        public void Start()
        {
            AudioSource = GetComponent<AudioSource>();

            Menu.SetActive(!RuntimeUtils.InVR);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (IsFacingUp && InView && other.TryGetComponent(out GorillaTriggerColliderHandIndicator handIndicator) && !handIndicator.isLeftHand && Time.time > (TouchTime + Debounce))
            {
                TouchTime = Time.time;
                GorillaTagger.Instance.StartVibration(handIndicator.isLeftHand, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.tapHapticDuration);

                Menu.SetActive(!Menu.activeSelf);

                AudioSource.PlayOneShot(AudioSource.clip, 0.4f);
            }
        }
    }
}
