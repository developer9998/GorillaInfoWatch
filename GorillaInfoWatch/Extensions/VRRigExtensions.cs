using UnityEngine;

namespace GorillaInfoWatch.Extensions;

internal static class VRRigExtensions
{
    public static AudioSource GetHandPlayer(this VRRig rig, bool isLeftHand) =>
            isLeftHand ? rig.leftHandPlayer : rig.rightHandPlayer;
}