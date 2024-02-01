using BepInEx;
using Bepinject;
using GorillaInfoWatch.Tools;
using HarmonyLib;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            new DataManager();
            new Logging(Logger);
            new Harmony(Constants.Guid).PatchAll(typeof(Plugin).Assembly);

            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);
        }
    }
}
