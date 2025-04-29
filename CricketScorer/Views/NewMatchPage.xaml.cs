using CricketScorer.Helpers;
using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class NewMatchPage : ContentPage
{
    private Match currentMatch;

    public NewMatchPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
        // Use currentMatch in the page as needed
        TeamAEntry.Text = currentMatch.TeamA;
        TeamBEntry.Text = currentMatch.TeamB;
    }

    private async void OnStartScoringClicked(object sender, EventArgs e)
    {
        string teamA = TeamAEntry.Text;
        string teamB = TeamBEntry.Text;
        int startingRuns = int.TryParse(RunsEntry.Text, out var r) ? r : 0;
        int oversInMatch = int.TryParse(OversEntry.Text, out var o) ? o : 0;

        if (oversInMatch <= 0)
        {
            await DisplayAlert("Error", "Please enter a valid number of overs.", "OK");
            return;
        }
        else
        {
            currentMatch.TotalOvers = oversInMatch;
            currentMatch.Runs = startingRuns;
        }

        if (string.IsNullOrWhiteSpace(teamA) || string.IsNullOrWhiteSpace(teamB) || oversInMatch <= 0)
        {
            await DisplayAlert("Error", "Please fill in all fields correctly.", "OK");
            return;
        }

        await Navigation.PushAsync(new ScoringPage(currentMatch));
    }

    private async void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await ButtonAnimations.ShrinkOnPress(element);
        }
    }

    private async void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is VisualElement element)
        {
            await ButtonAnimations.ExpandOnRelease(element);
        }
    }
}