using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricketScorer.Core.Models
{
    public class Match
    {
        public string TeamA { get; set; }
        public List<string> TeamAPlayers { get; set; } = new();

        public string TeamB { get; set; }
        public List<string> TeamBPlayers { get; set; } = new();
        public int TotalOvers { get; set; }
        public int Runs { get; set; }
        public int Wickets { get; set; }
        public List<Over> OversDetails { get; set; } = [];
        public int CurrentPairIndex { get; set; } = 0; // 0 = first pair
        public int OversPerPair { get; set; } = 2;      // Change pair every 2 overs

        public DateTime MatchDate { get; set; } = DateTime.Now;
        public bool IsMatchComplete { get; set; }

        public int TeamAScore { get; set; }
        public int TeamBScore { get; set; }

        public bool IsFirstInningsComplete { get; set; } = false;
        public bool IsFirstInnings { get; set; } = true; // Start with Team A batting first

        // Track who bowled each over
        public List<string> OverBowlers { get; set; } = new();
        public int TeamAWickets { get;  set; }
        public int TeamAOvers { get;  set; }
        public int TeamBWickets { get;  set; }
        public int TeamBOvers { get;  set; }

        public List<Over> FirstInningsOvers { get; set; } = new();
        public List<Over> SecondInningsOvers { get; set; } = new();

        public List<string> GetCurrentBatters(bool isFirstInnings)
        {
            return isFirstInnings ? TeamAPlayers : TeamBPlayers;
        }

        public string? CurrentBowler { get; set; }

        public Over? CurrentOver { get; set; }
        public MatchFormat Format { get; set; } = MatchFormat.Standard;
        public int StartingRuns { get; set; } = 200;

        public int BallsPerOver => Format == MatchFormat.Hundred ? 5 : 6;

        public List<PairOverride> TeamAPairOverrides { get; set; } = new();
        public List<PairOverride> TeamBPairOverrides { get; set; } = new();

        public List<PairOverride> GetActivePairOverrides()
        {
            return IsFirstInnings ? TeamAPairOverrides : TeamBPairOverrides;
        }

        public List<string> TeamARoster { get; set; } = new();
        public List<string> TeamBRoster { get; set; } = new();
        public string BattingFirst { get; set; }

        public enum MatchFormat
        {
            Standard, // 6-ball overs
            Hundred   // 5-ball overs, 100 total balls
        }

    }


}
