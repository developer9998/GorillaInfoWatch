using GorillaInfoWatch.Utilities;
using UnityEngine;
using Player = GorillaLocomotion.GTPlayer;

namespace GorillaInfoWatch.Behaviours.UI;

public class Panel : MonoBehaviour
{
    public bool Active => gameObject.activeSelf;

    public Transform Origin, Head;

    public Vector3 OriginOffset;

    private bool startup = false;

    private bool IsLeftHand => Watch.LocalWatch?.InLeftHand ?? true;
    private Player.HandState Hand => IsLeftHand ? Player.Instance.leftHand : Player.Instance.rightHand;
    private bool IsFacingUp => Vector3.Distance(Hand.controllerTransform.right * (IsLeftHand ? 1f : -1f), Vector3.up) > 1.75f;

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
        Head = Player.Instance.headCollider.transform;
        startup = true;

        if (!XRUtility.IsXRSubsystemActive)
        {
            enabled = false;
            SetActive(true);

            SetPosition();
            transform.position += Player.Instance.headCollider.transform.forward * 0.35f + Vector3.up * (Player.Instance.headCollider.radius * 3f);
            transform.rotation = Quaternion.identity;
        }
    }

    public void LateUpdate()
    {
        SetPosition();
        SetRotation();

        transform.localScale = Vector3.one * 1.7f * GorillaTagger.Instance.offlineVRRig.lastScaleFactor;

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
