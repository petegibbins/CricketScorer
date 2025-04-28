namespace CricketScorer;

public partial class MainPage : ContentPage
{
    int runs = 0;
    int wickets = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnAddRunClicked(object sender, EventArgs e)
    {
        runs++;
        UpdateScore();
    }

    private void OnAddWicketClicked(object sender, EventArgs e)
    {
        wickets++;
        UpdateScore();
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        runs = 0;
        wickets = 0;
        UpdateScore();
    }

    private void UpdateScore()
    {
        RunsLabel.Text = $"Runs: {runs}";
        WicketsLabel.Text = $"Wickets: {wickets}";
    }
}

