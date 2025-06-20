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
        TeamAScoreLabel.Text = $"Score: {matchResult.TeamAScore}/{matchResult.TeamAWickets} (Batting {matchResult.TeamABattingRuns}, Extras {matchResult.TeamAExtras})";
        TeamAPairStatsList.ItemsSource = matchResult.TeamABattingPairs;
        TeamABowlerStatsList.ItemsSource = matchResult.TeamBBowlingStats.OrderBy(x=>x.Bowler); // Team B bowled to Team A

        TeamBHeader.Text = $"{matchResult.TeamB} Summary";
        TeamBScoreLabel.Text = $"Score: {matchResult.TeamBScore}/{matchResult.TeamBWickets} (Batting {matchResult.TeamBBattingRuns}, Extras {matchResult.TeamBExtras})";
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

    private async void OnBackHomeClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync(); // Navigate back to the home page
    }
    public async Task ShowFullScorecard(MatchResult result)
    {
        var html = HtmlScorecardService.GenerateHtml(result);
        var path = Path.Combine(FileSystem.CacheDirectory, "scorecard.html");
        File.WriteAllText(path, html);

        await Navigation.PushAsync(new ScorecardWebViewPage(path));
    }

    private async void OnViewScorecardClicked(object sender, EventArgs e)
    {
        await ShowFullScorecard(result); // your current match result
    }
}