using BepInEx;
using Bepinject;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using Utilla;

namespace GorillaInfoWatch
{
    [ModdedGamemode, BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool InModdedRoom;

        public Plugin()
        {
            new Logging(Logger);
            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);
            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);
        }

        [ModdedGamemodeJoin]
        public void OnModdedJoin() => InModdedRoom = true;

        [ModdedGamemodeLeave]
        public void OnModdedLeave() => InModdedRoom = false;
    }
}
