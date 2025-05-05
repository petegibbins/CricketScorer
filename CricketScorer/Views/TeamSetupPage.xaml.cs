using System.Collections.ObjectModel;
using CricketScorer.Models;

namespace CricketScorer.Views
{
    public partial class TeamSetupPage : ContentPage
    {
        private readonly Match match;
        private readonly bool isTeamA;
        private ObservableCollection<string> players = new();

        public TeamSetupPage(Match match, bool isTeamA)
        {
            InitializeComponent();
            this.match = match;
            this.isTeamA = isTeamA;

            Title = isTeamA ? "Team A Setup" : "Team B Setup";
        }

        private void OnAddPlayerClicked(object sender, EventArgs e)
        {
            string? name = PlayerNameEntry.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                players.Add(name);
                PlayersCollectionView.ItemsSource = players;
                PlayerNameEntry.Text = string.Empty;
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            string? teamName = TeamNameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(teamName) || players.Count < 2)
            {
                await DisplayAlert("Missing Info", "Please enter a team name and at least 2 players.", "OK");
                return;
            }

            if (isTeamA)
            {
                match.TeamA = teamName;
                match.TeamAPlayers = players.ToList();
                await Navigation.PushAsync(new TeamSetupPage(match, false)); // Move to Team B
            }
            else
            {
                match.TeamB = teamName;
                match.TeamBPlayers = players.ToList();
                await Navigation.PushAsync(new NewMatchPage(match)); // Ready for match settings
            }
        }
    }
}