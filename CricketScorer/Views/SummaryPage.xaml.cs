using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class SummaryPage : ContentPage
{
    public SummaryPage(MatchResult result)
    {
        InitializeComponent();

        ResultLabel.Text = result.ResultText;
        ScoreSummaryLabel.Text = $"{result.TeamA}: {result.TeamAScore}  {result.TeamB}: {result.TeamBScore}";

        TeamAHeader.Text = $"{result.TeamA} Summary";
        TeamAScoreLabel.Text = $"Score: {result.TeamAScore}";
        TeamAPairStatsList.ItemsSource = result.TeamABattingPairs;
        TeamABowlerStatsList.ItemsSource = result.TeamBBowlingStats; // Team B bowled to Team A

        TeamBHeader.Text = $"{result.TeamB} Summary";
        TeamBScoreLabel.Text = $"Score: {result.TeamBScore}";
        TeamBPairStatsList.ItemsSource = result.TeamBBattingPairs;
        TeamBBowlerStatsList.ItemsSource = result.TeamABowlingStats; // Team A bowled to Team B
    }
}