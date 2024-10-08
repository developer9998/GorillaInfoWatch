﻿using GorillaInfoWatch.Interfaces;
using GorillaInfoWatch.Models;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class WatchTrigger : MonoBehaviour, IRelations
    {
        public Relations Relations { get; set; }

        public GameObject Menu;

        private AudioSource AudioSource;

        private readonly float Debounce = 0.33f;
        private float TouchTime;

        private bool IsFacingUp => Vector3.Distance(Player.Instance.leftControllerTransform.right, Vector3.up) > 1.82f;
        private bool InView => Vector3.Dot(Player.Instance.headCollider.transform.forward, (transform.position - Player.Instance.headCollider.transform.position).normalized) > 0.64f;

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

                AudioSource.PlayOneShot(AudioSource.clip, 0.4f);
            }
        }
    }
}
