using GorillaExtensions;
using GorillaInfoWatch.Utilities;
using UnityEngine;
using Player = GorillaLocomotion.GTPlayer;

namespace GorillaInfoWatch.Behaviours.UI;

public class Panel : MonoBehaviour
{
    public Transform Origin, Head, Trigger;

    public Vector3 OriginOffset;

    private bool _initialized = false;

    public bool Active => gameObject.activeSelf;
    public Vector3 UprightVector => Hand.controllerTransform.right * (IsLeftHand ? 1f : -1f) - Vector3.up;
    public bool InView => Vector3.Dot(Player.Instance.headCollider.transform.forward, (Trigger.position - Player.Instance.headCollider.transform.position).normalized) > 0.64f;

    private bool IsLeftHand => Watch.LocalWatch?.InLeftHand ?? true;
    private Player.HandState Hand => IsLeftHand ? Player.Instance.leftHand : Player.Instance.rightHand;

    public void Start()
    {
        _initialized = true;
        Head = Player.Instance.headCollider.transform;

        if (XRUtility.IsXRSubsystemActive) return;

        enabled = false;
        SetActive(true);
        SetPosition();

        transform.position += Player.Instance.headCollider.transform.forward * 0.35f + Vector3.up * (Player.Instance.headCollider.radius * 3f);
        transform.rotation = Quaternion.identity;
    }

    public void OnEnable()
    {
        if (!_initialized) return;

        SetPosition();
        SetRotation();
    }

    public void LateUpdate()
    {
        SetPosition();
        SetRotation();

        transform.localScale = Vector3.one * 1.7f * GorillaTagger.Instance.offlineVRRig.lastScaleFactor;

        // Turn off the menu if we're not looking at it, or if our hand is facing down
        if (UprightVector.IsShorterThan(1.75f)) gameObject.SetActive(false);
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
        Vector3 relativePosition = transform.position - Head.position;
        Vector3 eulerAngles = Quaternion.LookRotation(relativePosition, Vector3.up).eulerAngles;
        // euler_angles.x = Math.Abs(euler_angles.x) > Constants.MenuTiltAngle ? Mathf.Lerp((Mathf.Abs(euler_angles.x) - Constants.MenuTiltAngle) * Constants.MenuTiltAmount * Mathf.Sign(euler_angles.x), euler_angles.x, (Mathf.Abs(euler_angles.x) - Constants.MenuTiltAngle) / Constants.MenuTiltMinimum) : 0f;
        eulerAngles.x = 0f;
        eulerAngles.z = 0f;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    public void SetActive(bool active)
    {
        if (Active == active) return;
        gameObject.SetActive(active);
    }

#if DEBUG

    public void Step(float amount)
    {
        transform.position += Vector3.up * amount;
        SetRotation();
    }

#endif
}
