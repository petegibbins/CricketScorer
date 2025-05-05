using System.Threading.Tasks;
using CricketScorer.Helpers;
using CricketScorer.Models;

namespace CricketScorer.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnNewMatchClicked(object sender, EventArgs e)
        {
            Match match = new Match();
            await Navigation.PushAsync(new NewMatchPage(match));
        }

        private async void Button_Pressed(object sender, EventArgs e)
        {
            if (sender is VisualElement element)
            {
                await ButtonAnimations.ShrinkOnPress(element);
            }
        }

        private async void Button_Released(object sender, EventArgs e)
        {
            if (sender is VisualElement element)
            {
                await ButtonAnimations.ExpandOnRelease(element);
            }
        }

        private async void OnPlayerSetUpClicked(object sender, EventArgs e)
        {
            var match = new Match();
            await Navigation.PushAsync(new TeamSetupPage(match, true)); // Start with Team A
        }

        private async void OnStartDemoMatchClicked(object sender, EventArgs e)
        {
            var match = new Match
            {
                TeamA = "Twistas",
                TeamAPlayers = new List<string>
            {
                "Kat", "Kitch", "Bianka", "Sarahrah", "Gemma", "Em", "Teja", "Di"
            },
                TeamB = "Bury",
                TeamBPlayers = new List<string>
            {
                "Lisa", "Antonia", "Marta", "Sue", "Sally", "Ann", "Flo", "Mavis"
            },
                TotalOvers = 8,
                OversPerPair = 2,
                Runs = 200
            };

            await Navigation.PushAsync(new ScoringPage(match));
        }
    }
}