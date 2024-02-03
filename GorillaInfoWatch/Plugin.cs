using BepInEx;
using Bepinject;
using GorillaInfoWatch.Patches;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        Assembly GTAssembly => typeof(GorillaTagger).Assembly;
        Type RigPatchType => typeof(RigPatches);

        public Plugin()
        {
            new DataManager();
            new Logging(Logger);

            Harmony harmony = new(Constants.Guid);
            harmony.PatchAll(typeof(Plugin).Assembly);

            Type rigCacheType = GTAssembly.GetType("VRRigCache");
            harmony.Patch(AccessTools.Method(rigCacheType, "Start"), prefix: new HarmonyMethod(RigPatchType, nameof(RigPatches.Prepare)));
            harmony.Patch(AccessTools.Method(rigCacheType, "AddRigToGorillaParent"), postfix: new HarmonyMethod(RigPatchType, nameof(RigPatches.AddPatch)));
            harmony.Patch(AccessTools.Method(rigCacheType, "RemoveRigFromGorillaParent"), postfix: new HarmonyMethod(RigPatchType, nameof(RigPatches.RemovePatch)));

            Type rigContainerType = GTAssembly.GetType("RigContainer");
            harmony.Patch(AccessTools.Method(rigContainerType, "InitializeNetwork"), postfix: new HarmonyMethod(RigPatchType, nameof(RigPatches.NetworkFinalizePatch)));

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);
        }
    }
}
