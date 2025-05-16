using CricketScorer.Views;

namespace CricketScorer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new HomePage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            App.Current.UserAppTheme = AppTheme.Light;
        }
    }
}
