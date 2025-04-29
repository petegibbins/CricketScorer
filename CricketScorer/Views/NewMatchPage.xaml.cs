using CricketScorer.Helpers;
using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class NewMatchPage : ContentPage
{

    public NewMatchPage()
    {
        InitializeComponent();
    }

    private async void OnStartScoringClicked(object sender, EventArgs e)
    {
        string teamA = TeamAEntry.Text;
        string teamB = TeamBEntry.Text;
        int overs = int.TryParse(OversEntry.Text, out var o) ? o : 0;

        if (string.IsNullOrWhiteSpace(teamA) || string.IsNullOrWhiteSpace(teamB) || overs <= 0)
        {
            await DisplayAlert("Error", "Please fill in all fields correctly.", "OK");
            return;
        }

        var match = new Match
        {
            TeamA = teamA,
            TeamB = teamB,
            TotalOvers = overs,
            Runs = 200,        // Start at 200 runs
            Wickets = 0,
            Players = new List<Player>(),
            OversDetails = new List<Over>()
        };

        await Navigation.PushAsync(new ScoringPage(match));
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