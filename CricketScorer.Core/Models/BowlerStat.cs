﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class BowlerStat
    {
        public string Bowler { get; set; }
        public int RunsConceded { get; set; }
        public int Wickets { get; set; }
        public int ExtrasConceded { get; set; }  // NEW
    }
}
