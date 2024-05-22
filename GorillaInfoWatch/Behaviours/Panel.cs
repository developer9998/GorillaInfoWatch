using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Rendering;

namespace GorillaInfoWatch.Behaviours
{
    public class Panel : MonoBehaviour
    {
        private Transform Head;

        private bool IsFacingUp => Vector3.Distance(Player.Instance.leftControllerTransform.right, Vector3.up) > 1.75f;

        public void Start()
        {
            Head = Player.Instance.headCollider.transform;

            RenderPipelineManager.beginCameraRendering += (ScriptableRenderContext arg1, Camera arg2) => ApplyRotation();
        }

        public void ApplyRotation()
        {
            Vector3 forward = transform.position - Head.position;
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.eulerAngles = new Quaternion(0f, rotation.y, 0f, rotation.w).eulerAngles;
        }

        public void LateUpdate()
        {
            // Turn off the menu if we're not looking at it, or if our hand is facing down
            if (!IsFacingUp) gameObject.SetActive(false);
        }
    }
}
