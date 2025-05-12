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
        string teamA = TeamAEntry.Text?.Trim();
        string teamB = TeamBEntry.Text?.Trim();
        int overs = int.TryParse(OversEntry.Text, out var o) ? o : 0;
        int startingRuns = int.TryParse(RunsEntry.Text, out var r) ? r : 200;

        currentMatch.TeamA = teamA;
        currentMatch.TeamB = teamB;
        currentMatch.StartingRuns = startingRuns;
        currentMatch.Runs = startingRuns;
        currentMatch.Format = FormatPicker.SelectedIndex == 1 ? Match.MatchFormat.Hundred : Match.MatchFormat.Standard;
        currentMatch.TotalOvers = overs;

        await Navigation.PushAsync(new ScoringPage(currentMatch));
    }

    private void OnFormatChanged(object sender, EventArgs e)
    {
        if (FormatPicker.SelectedIndex == 1) // The Hundred
        {
            OversEntry.Text = "20";
            OversEntry.IsEnabled = false; // Optional: lock the field
        }
        else
        {
            OversEntry.Text = string.Empty;
            OversEntry.IsEnabled = true;
        }
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