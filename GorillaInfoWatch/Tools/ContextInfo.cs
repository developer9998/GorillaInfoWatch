using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Tools;

public static class ContextInfo
{
    private static bool? inVr;

    public static bool InVR
    {
        get
        {
            if (!inVr.HasValue)
            {
                List<ISubsystem> xrDisplaySubsystems = [];
                SubsystemManager.GetSubsystems(xrDisplaySubsystems);
                inVr = xrDisplaySubsystems.Count != 0;
            }

            return inVr.Value;
        }
    }
}