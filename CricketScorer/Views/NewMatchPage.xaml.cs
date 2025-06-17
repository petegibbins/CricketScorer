using CricketScorer.Helpers;
using CricketScorer.Core.Models;

namespace CricketScorer.Views;

public partial class NewMatchPage : ContentPage
{
    private Match currentMatch;

    public NewMatchPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
    }

    private async void OnStartScoringClicked(object sender, EventArgs e)
    {

        int overs = int.TryParse(OversEntry.Text, out var o) ? o : 0;

        // Validate Overs
        if (overs <= 0)
        {
            await DisplayAlert("Invalid Input", "Please enter a valid number of overs greater than 0.", "OK");
            return;
        }


        int startingRuns = int.TryParse(RunsEntry.Text, out var r) ? r : 200;

        currentMatch.StartingRuns = startingRuns;
        currentMatch.Runs = startingRuns;
        currentMatch.Format = FormatPicker.SelectedIndex == 1 ? Match.MatchFormat.Hundred : Match.MatchFormat.Standard;
        currentMatch.TotalOvers = overs;
        int oversPerPair = (int)OversPerPairPicker.SelectedItem;
        currentMatch.OversPerPair = oversPerPair;

        await Navigation.PushAsync(new PlayerSetupPage(currentMatch));
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