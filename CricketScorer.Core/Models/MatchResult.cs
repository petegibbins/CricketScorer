using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class MatchResult
    {
        public int TeamAWickets;
        public int TeamBWickets;

        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }
        public List<PairStat> BattingPairs { get; set; } = new();
        public List<BowlerStat> BowlingStats { get; set; } = new();
        public string ResultText { get; set; } // "Team A won by 12 runs"
        public DateTime DatePlayed { get; set; } = DateTime.Now;

        public List<PairStat> TeamABattingPairs { get; set; }
        public List<PairStat> TeamBBattingPairs { get; set; }

        public List<BowlerStat> TeamABowlingStats { get; set; }
        public List<BowlerStat> TeamBBowlingStats { get; set; }
        public List<Over> TeamAInnings { get;  set; }
        public List<Over> TeamBInnings { get;  set; }
        public int TeamAExtras { get; set; }
        public int TeamBExtras { get; set; }
        public int TeamABattingRuns { get; set; }
        public int TeamBBattingRuns { get; set; }
    }

}
