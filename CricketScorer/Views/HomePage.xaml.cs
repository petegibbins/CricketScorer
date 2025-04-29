using System.Threading.Tasks;
using CricketScorer.Helpers;

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
    }
}