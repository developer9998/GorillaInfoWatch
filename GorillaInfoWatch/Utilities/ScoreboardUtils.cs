using Photon.Realtime;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Utilities
{
    public static class ScoreboardUtils
    {
        public static GorillaPlayerScoreboardLine FindLine(Player player) => Object.FindObjectsOfType<GorillaPlayerScoreboardLine>().FirstOrDefault(line => line.linePlayer.UserId == player.UserId);
    }
}
