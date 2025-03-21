using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System.Collections.Generic;

namespace GorillaInfoWatch.Behaviours
{
    public class Panel : MonoBehaviour
    {
        private Transform Head;

        private bool IsFacingUp => Vector3.Distance(GTPlayer.Instance.leftControllerTransform.right, Vector3.up) > 1.75f;

        public void Awake()
        {
            Head = GTPlayer.Instance.headCollider.transform;

            RenderPipelineManager.beginCameraRendering += (context, camera) => ApplyRotation();
            // RenderPipelineManager.endCameraRendering += (ScriptableRenderContext arg1, Camera arg2) => ApplyRotation();

            // https://github.com/TheKnownPerson/HandController/blob/79960c20fe79d01a14f67c4a700ae072ebc7ac9c/Plugin.cs#L103

            var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(xrDisplaySubsystems);

            if (xrDisplaySubsystems.Count == 0)
            {
                enabled = false;
                gameObject.SetActive(true);
                gameObject.transform.localPosition += Vector3.up * 3f;
            }
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
