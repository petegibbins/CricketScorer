using System.Threading.Tasks;

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
            await Navigation.PushAsync(new NewMatchPage());
        }
    }
}