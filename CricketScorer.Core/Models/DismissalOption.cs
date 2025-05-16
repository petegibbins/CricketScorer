using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class DismissalOption
    {
        public required string Label { get; set; }
        public bool IsSelected { get; set; }
    }
}
