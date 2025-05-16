using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class PairStat
    {
        public required string Batter1 { get; set; }
        public required string Batter2 { get; set; }
        public int RunsScored { get; set; }

        public int WicketsLost { get; set; }

        public string Pair => $"{Batter1} & {Batter2}";
    }
}
