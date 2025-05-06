using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class SummaryPage : ContentPage
{
    public SummaryPage(MatchResult matchResult)
    {
        InitializeComponent();

        ResultLabel.Text = matchResult.ResultText;
        ScoreSummaryLabel.Text = $"{matchResult.TeamA}: {matchResult.TeamAScore}   {matchResult.TeamB}: {matchResult.TeamBScore}";

        var pairDisplay = matchResult.BattingPairs
            .Select(p => new
            {
                Pair = $"{p.Batter1} & {p.Batter2}",
                RunsScored = p.RunsScored
            }).ToList();

        PairStatsList.ItemsSource = pairDisplay;

        BowlerStatsList.ItemsSource = matchResult.BowlingStats;
    }
}