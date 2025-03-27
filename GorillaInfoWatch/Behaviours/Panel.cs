using GorillaLocomotion;
using UnityEngine;
using GorillaInfoWatch.Utilities;
using System;

namespace GorillaInfoWatch.Behaviours
{
    public class Panel : MonoBehaviour
    {
        public Transform Origin, Head;

        public Vector3 OriginOffset;

        private bool startup = false;

        private bool IsFacingUp => Vector3.Distance(GTPlayer.Instance.leftControllerTransform.right, Vector3.up) > 1.75f;

        public void OnEnable()
        {
            if (startup)
            {
                SetPosition();
                SetRotation();
            }
        }

        public void Start()
        {
            Head = GTPlayer.Instance.headCollider.transform;
            startup = true;

            if (!RuntimeUtils.InVR)
            {
                enabled = false;
                gameObject.SetActive(true);
                SetPosition();
                transform.position += (GTPlayer.Instance.headCollider.transform.forward * 0.35f) + (Vector3.up * (GTPlayer.Instance.headCollider.radius * 3f));
                transform.rotation = Quaternion.identity;
            }
        }

        public void LateUpdate()
        {
            SetPosition();
            SetRotation();
            
            // Turn off the menu if we're not looking at it, or if our hand is facing down
            if (!IsFacingUp) gameObject.SetActive(false);
        }

        public void SetPosition()
        {
            if (Origin)
            {
                transform.position = Origin.position + Origin.rotation * Vector3.zero;
            }
        }

        public void SetRotation()
        {
            Vector3 forward = transform.position - Head.position;
            Vector3 euler_angles = Quaternion.LookRotation(forward, Vector3.up).eulerAngles;
            euler_angles.x = Math.Abs(euler_angles.x) > Constants.MenuTiltAngle ? Mathf.Lerp((Mathf.Abs(euler_angles.x) - Constants.MenuTiltAngle) * Constants.MenuTiltAmount * Mathf.Sign(euler_angles.x), euler_angles.x, (Mathf.Abs(euler_angles.x) - Constants.MenuTiltAngle) / Constants.MenuTiltMinimum) : 0f;
            euler_angles.z = 0f;
            transform.rotation = Quaternion.Euler(euler_angles);
        }

        #if DEBUG

        public void Step(float amount)
        {
            transform.position += Vector3.up * amount;
            SetRotation();
        }

        #endif
    }
}
