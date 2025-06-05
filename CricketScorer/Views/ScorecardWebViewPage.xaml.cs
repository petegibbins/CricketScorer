using System.Net.Http;

namespace CricketScorer.Views;

public partial class ScorecardWebViewPage : ContentPage
{
    private readonly string _htmlPath;

    public ScorecardWebViewPage(string htmlPath)
    {
        InitializeComponent();
        _htmlPath = htmlPath;

        if (File.Exists(htmlPath))
        {
            var html = File.ReadAllText(htmlPath);
            HtmlWebView.Source = new HtmlWebViewSource { Html = html };
        }
        else
        {
            HtmlWebView.Source = new HtmlWebViewSource { Html = "<h3>Error loading scorecard.</h3>" };
        }
    }
    private async void OnShareClicked(object sender, EventArgs e)
    {
        if (!File.Exists(_htmlPath))
        {
            await DisplayAlert("Error", "Scorecard file not found.", "OK");
            return;
        }

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Softball Match Scorecard",
            File = new ShareFile(_htmlPath)
        });
    }
}