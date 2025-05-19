using System.Collections.ObjectModel;
using CricketScorer.Core.Models;

namespace CricketScorer.Views
{
    public partial class TeamSetupPage : ContentPage
    {
        private readonly Match match;
        private readonly bool isTeamA;
        private ObservableCollection<string> players = new();

        public TeamSetupPage(Match match)
        {
            InitializeComponent();
            this.match = match;

            Title = "Teams setup";
        }

        private async void OnAddPlayersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new PlayerSetupPage(match)); // or pass relevant context
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
            string? teamAName = TeamANameEntry.Text?.Trim();
            string? teamBName = TeamBNameEntry.Text?.Trim();


        }
    }
}