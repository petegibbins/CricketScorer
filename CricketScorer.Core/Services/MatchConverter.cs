using CricketScorer.Core.Models;

namespace CricketScorer.Core.Services;

public static class MatchConverter
{
    public static MatchResult BuildMatchResult(Match match)
    {
        var result = new MatchResult
        {
            TeamA = match.TeamA,
            TeamB = match.TeamB,
            DatePlayed = match.MatchDate
        };

        // First innings: Team A bats
        result.TeamAInnings = match.FirstInningsOvers;
        result.TeamABattingPairs = GroupBattingPairs(match.TeamAPlayers, match.FirstInningsOvers);
        result.TeamABowlingStats = SummariseBowling(match.SecondInningsOvers);
        result.TeamABattingRuns = TotalBattingRuns(match.FirstInningsOvers);
        result.TeamAExtras = TotalExtras(match.FirstInningsOvers);
        int teamAWickets = match.FirstInningsOvers.SelectMany(o => o.Deliveries).Count(b => b.IsWicket);
        result.TeamAScore = result.TeamABattingRuns + result.TeamAExtras - (teamAWickets * 5);

        // Second innings: Team B bats
        result.TeamBInnings = match.SecondInningsOvers;
        result.TeamBBattingPairs = GroupBattingPairs(match.TeamBPlayers, match.SecondInningsOvers);
        result.TeamBBowlingStats = SummariseBowling(match.FirstInningsOvers);
        result.TeamBBattingRuns = TotalBattingRuns(match.SecondInningsOvers);
        result.TeamBExtras = TotalExtras(match.SecondInningsOvers);
        int teamBWickets = match.SecondInningsOvers.SelectMany(o => o.Deliveries).Count(b => b.IsWicket);
        result.TeamBScore = result.TeamBBattingRuns + result.TeamBExtras - (teamBWickets * 5);

        // All pair stats flat list
        result.BattingPairs = result.TeamABattingPairs.Concat(result.TeamBBattingPairs).ToList();

        // All bowler stats flat list
        result.BowlingStats = result.TeamABowlingStats.Concat(result.TeamBBowlingStats).ToList();

        // Result text
        result.ResultText = GetResultText(result);

        return result;
    }

    private static int TotalBattingRuns(List<Over> overs)
    {
        return overs
            .SelectMany(o => o.Deliveries)
            .Where(b => !b.IsWide && !b.IsNoBall)
            .Sum(b => b.Runs);
    }

    private static int TotalExtras(List<Over> overs)
    {
        return overs
            .SelectMany(o => o.Deliveries)
            .Where(b => b.IsWide || b.IsNoBall)
            .Sum(b => b.Runs);
    }

    private static List<PairStat> GroupBattingPairs(List<string> players, List<Over> overs)
    {
        var grouped = overs
            .GroupBy(o => $"{o.Batter1} & {o.Batter2}")
            .Select(g =>
            {
                var firstOver = g.FirstOrDefault();
                var deliveries = g.SelectMany(o => o.Deliveries).ToList();
                return new PairStat
                {
                    Batter1 = firstOver?.Batter1 ?? "Unknown",
                    Batter2 = firstOver?.Batter2 ?? "Unknown",
                    RunsScored = deliveries.Where(b => !b.IsWide && !b.IsNoBall).Sum(b => b.Runs),
                    WicketsLost = deliveries.Count(b => b.IsWicket)
                };
            })
            .ToList();

        return grouped;
    }

    private static List<BowlerStat> SummariseBowling(List<Over> overs)
    {
        return overs
            .GroupBy(o => o.Bowler)
            .Select(g =>
            {
                var deliveries = g.SelectMany(o => o.Deliveries).ToList();
                return new BowlerStat
                {
                    Bowler = g.Key,
                    RunsConceded = deliveries.Sum(b => b.Runs),
                    Wickets = deliveries.Count(b =>
                                                b.IsWicket &&
                                                (b.DismissalType == "Bowled" ||
                                                 b.DismissalType == "Caught" ||
                                                 b.DismissalType == "LBW" ||
                                                 b.DismissalType == "Stumped" ||
                                                 b.DismissalType == "Hit Wicket")),
                    ExtrasConceded = deliveries.Where(b => b.IsWide || b.IsNoBall).Sum(b => b.Runs)
                };
            })
            .ToList();
    }

    private static string GetResultText(MatchResult result)
    {
        if (result.TeamAScore > result.TeamBScore)
            return $"{result.TeamA} won by {result.TeamAScore - result.TeamBScore} run{(result.TeamAScore - result.TeamBScore == 1 ? "" : "s")}";
        else if (result.TeamBScore > result.TeamAScore)
            return $"{result.TeamB} won by {result.TeamBScore - result.TeamAScore} run{(result.TeamBScore - result.TeamAScore == 1 ? "" : "s")}";
        else
            return "Match tied";
    }
}
