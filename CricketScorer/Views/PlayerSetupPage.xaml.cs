using CricketScorer.Core.Models;

namespace CricketScorer.Views;

public partial class PlayerSetupPage : ContentPage
{
    private List<string> batters = new List<string>();

    public PlayerSetupPage()
    {
        InitializeComponent();
    }

    private void OnAddPlayerClicked(object sender, EventArgs e)
    {
        string? name = PlayerNameEntry.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            batters.Add(name);
            PlayersCollectionView.ItemsSource = null; // Refresh list
            PlayersCollectionView.ItemsSource = batters;
            PlayerNameEntry.Text = string.Empty;
        }
        else
        {
            DisplayAlert("Error", "Please enter a valid player name.", "OK");
        }
    }

    private async void OnStartMatchClicked(object sender, EventArgs e)
    {
        if (batters.Count < 2)
        {
            await DisplayAlert("Error", "You need at least 2 batters to start the match.", "OK");
            return;
        }

        var match = new Match
        {
            TeamA = "Team A",  // Could let user enter later
            TeamB = "Team B",
            TotalOvers = 20   // Could let user pick later
            
        };

        await Navigation.PushAsync(new ScoringPage(match)); // Or ScoringPage if you prefer
    }
}