using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Models
{
    public class MatchResult
    {
        public string TeamA { get; set; }
        public string TeamB { get; set; }
        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }
        public List<PairStat> BattingPairs { get; set; } = new();
        public List<BowlerStat> BowlingStats { get; set; } = new();
        public string ResultText { get; set; } // "Team A won by 12 runs"
        public DateTime DatePlayed { get; set; } = DateTime.Now;
    }

}
