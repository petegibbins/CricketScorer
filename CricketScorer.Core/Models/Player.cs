using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class Player
    {
        public required string Name { get; set; }
        public int Runs { get; set; }
        public bool IsOut { get; set; }
    }
}
