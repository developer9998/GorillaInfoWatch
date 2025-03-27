using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaInfoWatch.Utilities
{
    public static class RuntimeUtils
    {
        public static bool InVR
        {
            get
            {
                // https://github.com/TheKnownPerson/HandController/blob/79960c20fe79d01a14f67c4a700ae072ebc7ac9c/Plugin.cs#L103
                var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                return xrDisplaySubsystems.Count > 0;
            }
        }
    }
}