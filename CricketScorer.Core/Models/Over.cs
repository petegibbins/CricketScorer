using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Models
{
    public class Over
    {
        public string Bowler { get; set; }
        public string Batter1 { get; set; }
        public string Batter2 { get; set; }
        public List<Ball> Deliveries { get; set; } = new();

        public bool IsFirstInning { get; set; } = true; // True if this is the first innings
    }
}
