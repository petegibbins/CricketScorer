using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Models
{

    public class Match
    {
        public required string TeamA { get; set; }
        public required string TeamB { get; set; }
        public List<Player> Players { get; set; } = [];
        public int TotalOvers { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public List<Over> OversDetails { get; set; } = [];
    }


}
