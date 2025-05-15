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

                // Split overs by innings
                TeamAInnings = match.FirstInningsOvers,

                TeamBInnings = match.SecondInningsOvers
            };

            result.TeamABattingRuns = CalculateBattingRuns(match.FirstInningsOvers);
            result.TeamAExtras = CalculateExtras(match.FirstInningsOvers);

            result.TeamBBattingRuns = CalculateBattingRuns(match.SecondInningsOvers);
            result.TeamBExtras = CalculateExtras(match.SecondInningsOvers);

            result.TeamABattingPairs = GetPairStats(match.FirstInningsOvers);
            result.TeamBBattingPairs = GetPairStats(match.SecondInningsOvers);

            result.TeamABowlingStats = GetBowlerStats(match.SecondInningsOvers); // Team A bowled to Team B
            result.TeamBBowlingStats = GetBowlerStats(match.FirstInningsOvers); // Team B bowled to Team A


            return result;
        }

        private static int CalculateBattingRuns(List<Over> overs)
        {
            return overs.SelectMany(o => o.Deliveries)
                        .Where(b => !b.IsNoBall && !b.IsWide && !b.IsWicket)
                        .Sum(b => b.Runs);
        }

        private static int CalculateExtras(List<Over> overs)
        {
            return overs.SelectMany(o => o.Deliveries)
                        .Where(b => b.IsNoBall || b.IsWide)
                        .Sum(b => b.Runs);
        }

        private static List<PairStat> GetPairStats(List<Over> overs)
        {
            var pairTotals = new Dictionary<(string, string), int>();
            var pairWickets = new Dictionary<(string, string), int>();

            foreach (var over in overs)
            {
                var pair = GetSortedPairKey(over.Batter1, over.Batter2);
                if (!pairTotals.ContainsKey(pair))
                {
                    pairTotals[pair] = 0;
                    pairWickets[pair] = 0;
                }

                foreach (var ball in over.Deliveries)
                {
                    pairTotals[pair] += ball.Runs;
                    if (ball.IsWicket)
                    {
                        pairTotals[pair] -= 5;
                        pairWickets[pair] += 1;
                    }
                }


            }

            return pairTotals.Select(p => new PairStat
            {
                Batter1 = p.Key.Item1,
                Batter2 = p.Key.Item2,
                RunsScored = p.Value,
                WicketsLost = pairWickets[p.Key]
            }).ToList();
        }


        private static string GenerateResultText(Match match)
        {
            if (match.TeamAScore > match.TeamBScore)
                if (match.TeamAScore - match.TeamBScore == 1)
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


        private static List<BowlerStat> GetBowlerStats(List<Over> overs)
        {
            var bowlerMap = new Dictionary<string, BowlerStat>();

            foreach (var over in overs)
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
                    if (ball.IsNoBall || ball.IsWide)
                    {
                        bowlerMap[over.Bowler].ExtrasConceded += ball.Runs;
                    }
                    else
                    {
                        bowlerMap[over.Bowler].RunsConceded += ball.Runs;
                    }

                    if (ball.IsWicket && ball.DismissalType != "Stumped")
                        bowlerMap[over.Bowler].Wickets += 1;
                }
            }

            return bowlerMap.Values.ToList();
        }

        private static (string, string) GetSortedPairKey(string b1, string b2)
        {
            if (string.IsNullOrWhiteSpace(b1)) b1 = "?";
            if (string.IsNullOrWhiteSpace(b2)) b2 = "?";
            return string.Compare(b1, b2) <= 0 ? (b1, b2) : (b2, b1);
        }
    }

}
