using System.Text;
using CricketScorer.Core.Services;
using CricketScorer.Core.Models;

namespace CricketScorer.Views;

public partial class SummaryPage : ContentPage
{
    private readonly MatchResult result;

    public SummaryPage(MatchResult matchResult)
    {
        InitializeComponent();
        this.result = matchResult;
        ResultLabel.Text = matchResult.ResultText;
        ScoreSummaryLabel.Text = $"{matchResult.TeamA}: {matchResult.TeamAScore}  {matchResult.TeamB}: {matchResult.TeamBScore}";

        TeamAHeader.Text = $"{matchResult.TeamA} Summary";
        TeamAScoreLabel.Text = $"Score: {matchResult.TeamAScore} (Batting {matchResult.TeamABattingRuns}, Extras {matchResult.TeamAExtras})";
        TeamAPairStatsList.ItemsSource = matchResult.TeamABattingPairs;
        TeamABowlerStatsList.ItemsSource = matchResult.TeamBBowlingStats.OrderBy(x=>x.Bowler); // Team B bowled to Team A

        TeamBHeader.Text = $"{matchResult.TeamB} Summary";
        TeamBScoreLabel.Text = $"Score: {matchResult.TeamBScore} (Batting {matchResult.TeamBBattingRuns}, Extras {matchResult.TeamBExtras})";
        TeamBPairStatsList.ItemsSource = matchResult.TeamBBattingPairs;
        TeamBBowlerStatsList.ItemsSource = matchResult.TeamABowlingStats.OrderBy(x => x.Bowler); // Team A bowled to Team B
    }
    public string FormatSummaryText()
    {
        Formatter formatter = new();
        return formatter.FormatSummaryText(result);
    }
    private async void OnShareSummaryClicked(object sender, EventArgs e)
    {
        if (result != null)
        {
            var summary = FormatSummaryText();

            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Share Match Summary",
                Text = summary
            });
        }
        else
        {
            await DisplayAlert("Error", "No match summary found to share.", "OK");
        }
    }
}