using CricketScorer.Core.Models;
using System.Text;

public static class HtmlScorecardService
{
    public static string GenerateHtml(MatchResult result)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<html><head><title>Scorecard</title>");
        sb.AppendLine("<style>body { font-family: sans-serif; } table { border-collapse: collapse; width: 100%; margin-bottom: 20px; } th, td { border: 1px solid #ccc; padding: 5px; text-align: left; }</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h2>{result.TeamA} vs {result.TeamB} {result.DatePlayed:dd/MM/yyyy} Scorecard</h2>");

        void AddPairs(List<Over> overs, string team)
        {
            var pairs = GroupByBattingPairs(overs);
            sb.AppendLine($"<h3>{team} Innings</h3>");

            foreach (var pair in pairs)
            {
                sb.AppendLine($"<h4>{pair.Batter1} & {pair.Batter2}</h4>");
                sb.AppendLine("<table><tr><th>Ball</th><th>Outcome</th><th>Bowler</th></tr>");
                int runs = 0, wickets = 0, ballNumber = 1;

                foreach (var (ball, bowler) in pair.Deliveries)
                {
                    string outcome = ball.IsWicket ? "Wicket" : $"{ball.Runs} run(s)";
                    if (ball.IsWide) outcome += " (Wide)";
                    if (ball.IsNoBall) outcome += " (No Ball)";
                    if (!ball.IsWicket) runs += ball.Runs;
                    else wickets++;

                    sb.AppendLine($"<tr><td>{ballNumber++}</td><td>{outcome}</td><td>{bowler}</td></tr>");
                }

                sb.AppendLine("</table>");
                sb.AppendLine($"<p><strong>Total:</strong> {runs} runs, {wickets} wicket(s)</p>");
            }
        }

        AddPairs(result.TeamAInnings, result.TeamA);
        AddPairs(result.TeamBInnings, result.TeamB);

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private class PairStats
    {
        public string Batter1 { get; set; }
        public string Batter2 { get; set; }
        public List<(Ball Ball, string Bowler)> Deliveries { get; set; } = new();
    }

    private static List<PairStats> GroupByBattingPairs(List<Over> overs)
    {
        return overs
            .GroupBy(o => new { o.Batter1, o.Batter2 })
            .Select(g => new PairStats
            {
                Batter1 = g.Key.Batter1,
                Batter2 = g.Key.Batter2,
                Deliveries = g.SelectMany(o =>
                    o.Deliveries.Select(b => (Ball: b, Bowler: o.Bowler ?? ""))).ToList()
            }).ToList();
    }
}