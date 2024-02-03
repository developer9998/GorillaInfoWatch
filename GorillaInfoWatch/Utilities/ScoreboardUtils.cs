using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace GorillaInfoWatch.Utilities
{
    public static class ScoreboardUtils
    {
        private static List<GorillaScoreBoard> Scoreboards => GorillaScoreboardTotalUpdater.allScoreboards;

        public static GorillaPlayerScoreboardLine FindLine(Player player)
        {
            if (Scoreboards != null || Scoreboards.Count > 0)
            {
                GorillaPlayerScoreboardLine lineCandidate = null;
                foreach (GorillaScoreBoard sB in Scoreboards)
                {
                    if (!sB.isActiveAndEnabled) continue;

                    lineCandidate = sB.lines.FirstOrDefault(line => line.linePlayer != null && line.linePlayer.UserId == player.UserId);
                    if (lineCandidate != null) break;
                }

                return lineCandidate;
            }

            return null;
        }
    }
}
