using BepInEx.Configuration;
using Bepinject;

namespace GorillaInfoWatch.Tools
{
    public class Configuration
    {
        private readonly ConfigFile File;

        public Configuration(BepInConfig config)
        {
            File = config.Config;
        }
    }
}
