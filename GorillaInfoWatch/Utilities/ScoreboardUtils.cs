using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Utilities
{
    public static class ScoreboardUtils
    {
        private static List<GorillaPlayerScoreboardLine> Lines => GorillaScoreboardTotalUpdater.allScoreboardLines;
        
        public static GorillaPlayerScoreboardLine FindLine(Player player) => Lines.FirstOrDefault(line => line.linePlayer != null && line.linePlayer.UserId == player.UserId);
    }
}
