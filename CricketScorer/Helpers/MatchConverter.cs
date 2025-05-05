using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CricketScorer.Models;

namespace CricketScorer.Helpers
{
    public static class MatchConverter
    {
        public static MatchResult BuildMatchResult(Match match)
        {
            var result = new MatchResult
            {
                TeamA = match.TeamA,
                TeamB = match.TeamB,
                TeamAScore = match.TeamAScore,
                TeamBScore = match.TeamBScore,
                ResultText = GenerateResultText(match),
                DatePlayed = DateTime.Now
            };

            result.BattingPairs = GetPairStats(match);
            result.BowlingStats = GetBowlerStats(match);

            return result;
        }

        private static string GenerateResultText(Match match)
        {
            if (match.TeamAScore > match.TeamBScore)
                return $"{match.TeamA} won by {match.TeamAScore - match.TeamBScore} runs";
            else if (match.TeamBScore > match.TeamAScore)
                return $"{match.TeamB} won by {match.TeamBScore - match.TeamAScore} runs";
            else
                return "Match tied";
        }

        private static List<PairStat> GetPairStats(Match match)
        {
            var pairStats = new Dictionary<(string, string), int>();

            foreach (var over in match.OversDetails)
            {
                foreach (var ball in over.Deliveries)
                {
                    var key = GetSortedPairKey(ball.Batter1, ball.Batter2);

                    if (!pairStats.ContainsKey(key))
                        pairStats[key] = 0;

                    pairStats[key] += ball.Runs;
                    if (ball.IsWicket)
                        pairStats[key] -= 5;
                }
            }

            return pairStats.Select(p => new PairStat
            {
                Batter1 = p.Key.Item1,
                Batter2 = p.Key.Item2,
                RunsScored = p.Value
            }).ToList();
        }

        private static List<BowlerStat> GetBowlerStats(Match match)
        {
            var bowlerMap = new Dictionary<string, BowlerStat>();

            foreach (var over in match.OversDetails)
            {
                if (!bowlerMap.ContainsKey(over.Bowler))
                    bowlerMap[over.Bowler] = new BowlerStat
                    {
                        Bowler = over.Bowler,
                        RunsConceded = 0,
                        Wickets = 0
                    };

                foreach (var ball in over.Deliveries)
                {
                    bowlerMap[over.Bowler].RunsConceded += ball.Runs;
                    if (ball.IsWicket)
                        bowlerMap[over.Bowler].Wickets += 1;
                }
            }

            return bowlerMap.Values.ToList();
        }

        private static (string, string) GetSortedPairKey(string b1, string b2)
        {
            return string.Compare(b1, b2) <= 0 ? (b1, b2) : (b2, b1);
        }
    }

}
