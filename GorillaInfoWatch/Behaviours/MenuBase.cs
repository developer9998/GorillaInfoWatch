using GorillaExtensions;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class MenuBase : MonoBehaviour
    {
        private Transform Head;
        private Quaternion Rotation;

        public void Start()
        {
            Head = Player.Instance.headCollider.transform;
        }

        public void Update()
        {
            CalculateRotation();

            transform.rotation = Rotation;
        }

        public void LateUpdate()
        {
            // Turn off the menu if we're not looking at it, or if our hand is facing down
            bool isFacingUp = Vector3.Distance(Player.Instance.leftControllerTransform.right, Vector3.up) > 1.75f;
            if (!isFacingUp) gameObject.SetActive(false);
        }

        private void CalculateRotation()
        {
            Vector3 forward = transform.position - Head.position;
            Vector3 eulerAngles = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.WithZ(0);
            Quaternion rotation = Quaternion.Euler(eulerAngles);
            Rotation = new Quaternion(0f, rotation.y, 0f, rotation.w);
        }
    }
}
