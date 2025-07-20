using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace GorillaInfoWatch.Tools
{
    public static class ContextInfo
    {
        public static bool InVR
        {
            get
            {
                if (!inVr.HasValue)
                {
                    List<XRDisplaySubsystem> xrDisplaySubsystems = [];
                    SubsystemManager.GetInstances(xrDisplaySubsystems);
                    inVr = xrDisplaySubsystems.Count != 0;
                }

                return inVr.Value;
            }
        }

        private static bool? inVr = null;
    }
}