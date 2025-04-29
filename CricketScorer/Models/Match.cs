using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Models
{

    public class Match
    {
        public string TeamA { get; set; }
        public List<string> TeamABatters { get; set; } = new();

        public string TeamB { get; set; }
        public List<string> TeamBBatters { get; set; } = new();
        public List<Player> Players { get; set; } = [];
        public int TotalOvers { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public List<Over> OversDetails { get; set; } = [];
        public int CurrentPairIndex { get; set; } = 0; // 0 = first pair
        public int OversPerPair { get; set; } = 2;      // Change pair every 2 overs
        public List<string> GetCurrentBatters(bool isFirstInnings)
        {
            return isFirstInnings ? TeamABatters : TeamBBatters;
        }

    }


}
