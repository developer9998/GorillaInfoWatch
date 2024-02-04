using HarmonyLib;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Utilities
{
    public static class ScoreboardUtils
    {
        private static List<GorillaScoreBoard> Scoreboards => GorillaScoreboardTotalUpdater.allScoreboards;

        public static List<GorillaPlayerScoreboardLine> GetActiveLines()
            => Scoreboards.FirstOrDefault(sB => sB.gameObject.activeInHierarchy && sB.linesParent.activeInHierarchy).lines ?? null;

        public static GorillaPlayerScoreboardLine FindLine(Player player)
            => GetActiveLines().FirstOrDefault(line => line.linePlayer != null && line.linePlayer.UserId == player.UserId);

        public static void RedrawLines()
            => Scoreboards.Do(sB => sB.RedrawPlayerLines());
    }
}
