using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class SummaryPage : ContentPage
{
    private Match completedMatch;
    private int teamATotal, teamAWkts, teamAOvers;

    public SummaryPage(Match match, int teamATotal, int teamAWickets, int teamAOvers)
    {
        InitializeComponent();
        completedMatch = match;
        this.teamATotal = teamATotal;
        this.teamAWkts = teamAWickets;
        this.teamAOvers = teamAOvers;
        DisplaySummary();
    }
    private void DisplaySummary()
    {
        MatchResultLabel.Text = $"{completedMatch.TeamA} vs {completedMatch.TeamB}";

        string teamAStats = $"{completedMatch.TeamA}: {teamATotal}/{teamAWkts} in {teamAOvers} overs";
        string teamBStats = $"{completedMatch.TeamB}: {completedMatch.Runs}/{completedMatch.Wickets} in {completedMatch.OversDetails.Count} overs";

        DetailedScoreLabel.Text = $"{teamAStats}\n{teamBStats}";

        // Optional: Show who won
        if (completedMatch.Runs > teamATotal)
        {
            DetailedScoreLabel.Text += $"\n\n{completedMatch.TeamB} WON!";
        }
        else if (completedMatch.Runs < teamATotal)
        {
            DetailedScoreLabel.Text += $"\n\n{completedMatch.TeamA} WON!";
        }
        else
        {
            DetailedScoreLabel.Text += $"\n\nMatch Tied!";
        }
    }

    private async void OnBackHomeClicked(object sender, EventArgs e)
    {
        // Go back to the Home Page
        await Navigation.PopToRootAsync();
    }
}