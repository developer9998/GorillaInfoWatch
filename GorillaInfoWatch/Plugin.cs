using BepInEx;
using Bepinject;
using GorillaInfoWatch.Patches;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            new DataManager();
            new Logging(Logger);

            Harmony harmony = new Harmony(Constants.Guid);
            harmony.PatchAll(typeof(Plugin).Assembly);

            Type rigCacheType = typeof(GorillaTagger).Assembly.GetType("VRRigCache");
            harmony.Patch(AccessTools.Method(rigCacheType, "Start"), prefix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.Prepare)));
            harmony.Patch(AccessTools.Method(rigCacheType, "AddRigToGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.AddPatch)));
            harmony.Patch(AccessTools.Method(rigCacheType, "RemoveRigFromGorillaParent"), postfix: new HarmonyMethod(typeof(RigPatches), nameof(RigPatches.RemovePatch)));

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);
        }
    }
}
