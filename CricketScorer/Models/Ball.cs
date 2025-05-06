using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Models
{
    public class Ball
    {
        public string Batters { get; set; }     // The batter who faced the ball
        public int Runs { get; set; }
        public bool IsWicket { get; set; }
        public bool IsWide { get; set; }
        public bool IsNoBall { get; set; }
        //public  string Batter1 { get; set; }
        //public  string Batter2 { get; set; }

        public override string ToString()
        {
            if (IsWide) return "Wd";
            if (IsNoBall) return "Nb";
            if (IsWicket) return "W";
            if (Runs == 0) return ".";
            return Runs.ToString();
        }
    }
}
