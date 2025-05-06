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
                DatePlayed = match.MatchDate,
                ResultText = GenerateResultText(match),
                BattingPairs = GetPairStats(match),
                BowlingStats = GetBowlerStats(match)
            };

            return result;
        }

        private static string GenerateResultText(Match match)
        {
            if (match.TeamAScore > match.TeamBScore)
                if(match.TeamAScore - match.TeamBScore == 1)
                    return $"{match.TeamA} won by 1 run";
                else
                    return $"{match.TeamA} won by {match.TeamAScore - match.TeamBScore} runs";
            else if (match.TeamBScore > match.TeamAScore)
                if (match.TeamBScore - match.TeamAScore == 1)
                    return $"{match.TeamB} won by 1 run";
                else
                    return $"{match.TeamB} won by {match.TeamBScore - match.TeamAScore} runs";
            else
                return "Match tied";
        }

        private static List<PairStat> GetPairStats(Match match)
        {
            var pairTotals = new Dictionary<(string, string), int>();

            foreach (var over in match.OversDetails)
            {
                foreach (var ball in over.Deliveries)
                {
                    var pair = GetSortedPairKey(over.Batter1, over.Batter2);
                    if (!pairTotals.ContainsKey(pair))
                        pairTotals[pair] = 0;

                    pairTotals[pair] += ball.Runs;

                    if (ball.IsWicket)
                        pairTotals[pair] -= 5; // softball rule
                }
            }

            return pairTotals.Select(p => new PairStat
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
                if (string.IsNullOrEmpty(over.Bowler)) continue;

                if (!bowlerMap.ContainsKey(over.Bowler))
                {
                    bowlerMap[over.Bowler] = new BowlerStat
                    {
                        Bowler = over.Bowler,
                        RunsConceded = 0,
                        Wickets = 0
                    };
                }

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
            if(string.IsNullOrWhiteSpace(b1)) b1 = "?";
            if (string.IsNullOrWhiteSpace(b2)) b2 = "?";
            return string.Compare(b1, b2) <= 0 ? (b1, b2) : (b2, b1);
        }
    }

}
