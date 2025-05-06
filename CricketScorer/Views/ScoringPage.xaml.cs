using System.Diagnostics;
using CricketScorer.Helpers;
using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class ScoringPage : ContentPage
{

    private Match currentMatch;
    private int ballsInCurrentOver = 0;
    private Over currentOver = new Over();
    private bool isFirstInnings = true;
    private bool matchEnded = false;
    private string currentBowler = string.Empty;
    private bool showBowlerPopup = false;
    private List<string> availableBowlers = new();


    public ScoringPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
        UpdateScoreDisplay();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);
        showBowlerPopup = true;
        BowlerPopup.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Start page invisible
        this.Opacity = 0;

        // Fade to fully visible over 500 milliseconds
        await this.FadeTo(1, 500, Easing.CubicInOut);

        if (currentMatch.OversDetails.Count == 0 && string.IsNullOrEmpty(currentBowler))
        {
            await SelectNextBowler();
            UpdateScoreDisplay();
        }
    }

    private void UpdateScoreDisplay()
    {
        ScoreLabel.Text = $"Score: {currentMatch.Runs}/{currentMatch.Wickets}";
        OverLabel.Text = $"Overs: {currentMatch.OversDetails.Count}/{currentMatch.TotalOvers}";
        BattingTeamLabel.Text = isFirstInnings ? currentMatch.TeamA : currentMatch.TeamB;

        var overs = currentMatch.OversDetails.ToList();
        var batters = isFirstInnings ? currentMatch.TeamAPlayers : currentMatch.TeamBPlayers;

        // NEW: Update batting pair label
        if (batters.Count >= (currentMatch.CurrentPairIndex * 2 + 2))
        {
            string batter1 = batters[currentMatch.CurrentPairIndex * 2];
            string batter2 = batters[currentMatch.CurrentPairIndex * 2 + 1];
            BattingPairLabel.Text = $"{batter1} & {batter2}";
        }
        else
        {
            BattingPairLabel.Text = "No Active Batters";
        }

        if (!isFirstInnings)
        {
            int target = currentMatch.TeamAScore + 1; // Must beat Team A's total
            int runsNeeded = target - currentMatch.Runs;

            if (runsNeeded > 0)
            {
                TargetLabel.Text = $"Needs {runsNeeded} more to win";
            }
            else
            {
                TargetLabel.Text = $"{currentMatch.TeamB} has won!";
            }
        }
        else
        {
            TargetLabel.Text = ""; // Hide during first innings
        }

        // Add the current over (even if incomplete) to the list temporarily
        if (currentOver.Deliveries.Count > 0)
        {
            overs.Add(currentOver);
        }

        if (overs.Count >= 1)
        {
            var lastOver = overs.Last();
            //LastOver1Label.Text = $"Over {overs.Count}: " + string.Join(" ", lastOver.Balls.Select(b => b.ToString()));
            LastOver1Label.FormattedText = BuildOverFormattedString(overs.Count >= 1 ? overs[^1] : null, overs.Count);
        }
        else
        {
            LastOver1Label.Text = "-";
        }

        if (overs.Count >= 2)
        {
            var secondLastOver = overs[^2];
            LastOver2Label.FormattedText = BuildOverFormattedString(overs.Count >= 2 ? overs[^2] : null, overs.Count - 1);
        }
        else
        {
            LastOver2Label.Text = "-";
        }
    }

    private FormattedString BuildOverFormattedString(Over over, int overNumber)
    {
        var formatted = new FormattedString();

        if (over == null || over.Deliveries.Count == 0)
        {
            formatted.Spans.Add(new Span { Text = "-" });
            return formatted;
        }

        formatted.Spans.Add(new Span
        {
            Text = $"Over {overNumber}: ",
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.DarkBlue
        });

        foreach (var ball in over.Deliveries)
        {
            string ballText = ball.ToString() + " ";

            formatted.Spans.Add(new Span
            {
                Text = ballText,
                TextColor = ball.IsWicket ? Colors.Red : Colors.Black,
                FontAttributes = FontAttributes.None
            });
        }

        return formatted;
    }

    private void AddRuns(int runs, bool isWide = false, bool isNoBall = false)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        currentMatch.Runs += runs;
        if (isFirstInnings)
        {
            currentMatch.TeamAScore = currentMatch.Runs;
        }
        else
        {
            currentMatch.TeamBScore = currentMatch.Runs;
        }
        ballsInCurrentOver++;

        currentOver.Deliveries.Add(new Ball { Runs = runs, IsWide = isWide, IsNoBall = isNoBall, Batters = BattingPairLabel.Text });



        CheckForMatchWin();
        CheckOverComplete();
        UpdateScoreDisplay();
    }

    private void OnOneRunClicked(object sender, EventArgs e)
    {
        AddRuns(1);
    }

    private void OnFourRunsClicked(object sender, EventArgs e)
    {
        AddRuns(4);
    }

    private void OnSixRunsClicked(object sender, EventArgs e)
    {
        AddRuns(6);
    }

    private async void OnWicketClicked(object sender, EventArgs e)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        currentMatch.Wickets++;
        currentMatch.Runs -= 5; // Softball cricket rule: lose 5 runs
        if (currentMatch.Runs < 0)
            currentMatch.Runs = 0; // Prevent negative total score if needed

        ballsInCurrentOver++;
        currentOver.Deliveries.Add(new Ball { Runs = 0, IsWicket = true });

        await DisplayAlert("Wicket!", "-5 runs penalty applied.", "OK");
        CheckForMatchWin();
        CheckOverComplete();
        UpdateScoreDisplay();
    }

    private void OnEndOverClicked(object sender, EventArgs e)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        EndOver();
    }

    private void CheckOverComplete()
    {
        if (ballsInCurrentOver >= 6)
        {
            EndOver();
        }
    }

    private async void EndOver()
    {
        if (currentOver.Deliveries.Count > 0)
        {
            currentMatch.OversDetails.Add(currentOver);
            currentMatch.OverBowlers.Add(currentBowler);
            currentOver = new Over(); // Start a fresh over
        }

        ballsInCurrentOver = 0;
        UpdateScoreDisplay();

        // NEW: Check if time to swap batting pair
        int oversBowled = currentMatch.OversDetails.Count;
        if (oversBowled % currentMatch.OversPerPair == 0)
        {
            bool swap = await DisplayAlert("Swap Batting Pair", "Swap to next batting pair now?", "Yes", "No");
            if (swap)
            {
                currentMatch.CurrentPairIndex++;
                UpdateScoreDisplay(); // Refresh new batters
            }
        }

        if (currentMatch.OversDetails.Count >= currentMatch.TotalOvers)
        {
            bool endNow = await DisplayAlert("Overs Complete", "The maximum number of overs is reached. End the innings now?", "Yes", "No");
            if (endNow)
            {
                await EndInningsManually();
            }
        }
        await SelectNextBowler();
    }

    private async Task SelectNextBowler()
    {
        try
        {
            List<string> bowlers = isFirstInnings ? currentMatch.TeamBPlayers : currentMatch.TeamAPlayers; // Bowling team is the fielding side
            if (bowlers.Count == 0)
            {
                await DisplayAlert("No Bowlers", "No bowlers available!", "OK");
                return;
            }

            BowlerList.ItemsSource = null;
            BowlerList.ItemsSource = bowlers;

            BowlerPopup.Opacity = 0;
            BowlerPopup.IsVisible = true;

            DimBackground.IsVisible = true;
            BowlerPopup.Opacity = 0;
            BowlerPopup.IsVisible = true;

            await BowlerPopup.FadeTo(1, 250, Easing.CubicOut);
            await BowlerPopup.ScaleTo(1.1, 150);
            await BowlerPopup.ScaleTo(1.0, 100);
            ScrollHintLabel.IsVisible = bowlers.Count > 5;
            ScrollFadeOverlay.IsVisible = bowlers.Count > 5;

            // Set the batters for this over
            var batters = isFirstInnings ? currentMatch.TeamAPlayers : currentMatch.TeamBPlayers;
            currentOver.Batter1 = batters[currentMatch.CurrentPairIndex * 2];
            currentOver.Batter2 = batters[currentMatch.CurrentPairIndex * 2 + 1];
            currentOver.Bowler = currentBowler;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EXCEPTION] {ex}");
        }
    }



    private void OnBowlerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedBowler)
        {
            currentBowler = selectedBowler;
            currentMatch.OverBowlers.Add(currentBowler);
            UpdateBowlerDisplay();
            BowlerPopup.IsVisible = false;
            showBowlerPopup = false;
            BowlerPopup.IsVisible = true;  // show
            DimBackground.IsVisible = false;
        }
    }

    private void OnBowlerTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.Content is Label label && label.Text is string selectedBowler)
        {
            currentBowler = selectedBowler;
            currentMatch.OverBowlers.Add(currentBowler);
            UpdateBowlerDisplay();

            BowlerPopup.IsVisible = false;
            DimBackground.IsVisible = false;
        }
    }
    private async void OnBowlerNameTapped(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Change Bowler", "Do you want to change the current bowler?", "Yes", "Cancel");

        if (!confirm) return;

        // Show the same popup you use after End Over
        await SelectNextBowler();
    }

    private void OnCancelBowlerPopup(object sender, EventArgs e)
    {
        BowlerPopup.IsVisible = false;
        DimBackground.IsVisible = false;
        showBowlerPopup = false;
        BowlerPopup.IsVisible = false; // hide
    }

    private void UpdateBowlerDisplay()
    {
        CurrentBowlerLabel.Text = $"{currentBowler}";
    }

    private void OnWideClicked(object sender, EventArgs e)
    {
        AddRuns(2, true);
    }

    private void OnNoBallClicked(object sender, EventArgs e)
    {
        AddRuns(2, false, true);
    }


    private void OnDotClicked(object sender, EventArgs e)
    {
        AddRuns(0);
    }

    private async void OnUndoClicked(object sender, EventArgs e)
    {
        // Step 1: Check if there are any balls to undo
        if (currentOver.Deliveries.Count > 0)
        {
            var lastBall = currentOver.Deliveries.Last();

            // Step 2: Adjust match score based on last ball
            if (lastBall.IsWide || lastBall.IsNoBall)
            {
                currentMatch.Runs -= 2; // Wides and No-balls are +2 runs
            }
            else if (lastBall.IsWicket)
            {
                currentMatch.Wickets--;
                currentMatch.Runs += 5; // Wicket penalty was -5, now undo +5
            }
            else
            {
                currentMatch.Runs -= lastBall.Runs;
            }

            // Step 3: Remove the ball
            currentOver.Deliveries.Remove(lastBall);

            ballsInCurrentOver--;
            if (ballsInCurrentOver < 0) ballsInCurrentOver = 0; // Safety

            UpdateScoreDisplay();
        }
        else if (currentMatch.OversDetails.Count > 0)
        {
            // Step 4: If no balls in current over, pop back to previous over
            currentOver = currentMatch.OversDetails.Last();
            currentMatch.OversDetails.Remove(currentOver);

            if (currentOver.Deliveries.Count > 0)
            {
                var lastBall = currentOver.Deliveries.Last();

                if (lastBall.IsWide || lastBall.IsNoBall)
                {
                    currentMatch.Runs -= 2;
                }
                else if (lastBall.IsWicket)
                {
                    currentMatch.Wickets--;
                    currentMatch.Runs += 5;
                }
                else
                {
                    currentMatch.Runs -= lastBall.Runs;
                }

                currentOver.Deliveries.Remove(lastBall);
                ballsInCurrentOver = currentOver.Deliveries.Count;
            }

            UpdateScoreDisplay();
        }
        else
        {
            await DisplayAlert("Undo", "No balls to undo!", "OK");
        }
    }
    private async void OnEndInningsClicked(object sender, EventArgs e)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        bool confirm = await DisplayAlert("End Innings", "Are you sure you want to end this innings?", "Yes", "No");

        if (confirm)
        {
            await EndInningsManually();
        }
    }

    private async Task EndInningsManually()
    {
        if (currentOver.Deliveries.Count > 0)
        {
            currentMatch.OversDetails.Add(currentOver);
            currentOver = new Over();
        }

        if (isFirstInnings)
        {
            isFirstInnings = false;
            currentMatch.IsFirstInningsComplete = true;
            currentMatch.TeamAScore = currentMatch.Runs;
            currentMatch.TeamAWickets = currentMatch.Wickets;
            currentMatch.TeamAOvers = currentMatch.OversDetails.Count;

            currentMatch.Runs = 200; // Reset for Team B innings
            currentMatch.Wickets = 0;
            currentMatch.OversDetails.Clear();
            
            ballsInCurrentOver = 0;
            // End of the first innings, new over in the 2nd innings
            currentOver = new Over();

            // Reset the pointer to the batting pair, back to the first pair.
            currentMatch.CurrentPairIndex = 0;

            await DisplayAlert("New Innings", $"{currentMatch.TeamB} now batting to chase {currentMatch.TeamAScore} runs!", "OK");
            await SelectNextBowler();
            UpdateScoreDisplay();
        }
        else
        {
            currentMatch.TeamBScore = currentMatch.Runs;
            currentMatch.TeamBWickets = currentMatch.Wickets;
            currentMatch.TeamBOvers = currentMatch.OversDetails.Count;
            await EndGame();
        }
    }

    private async void CheckForMatchWin()
    {
        if (!isFirstInnings)
        {
            int target = currentMatch.TeamAScore + 1;

            if (currentMatch.Runs >= target && !matchEnded)
            {
                matchEnded = true;
                currentMatch.TeamBScore = currentMatch.Runs;
                await DisplayAlert("Match Won", $"{currentMatch.TeamB} has won the match!", "OK");

                bool endNow = await DisplayAlert("End Match?", "Would you like to end the match now?", "Yes", "No");
                if (endNow)
                {
                    await EndGame();
                }
            }
        }
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

    private async Task EndGame()
    {
        currentMatch.IsMatchComplete = true;
        try
        {
            Debug.WriteLine($"Overs count: {currentMatch.OversDetails.Count}");

            foreach (var over in currentMatch.OversDetails)
            {
                Debug.WriteLine($"Over by: {over.Bowler}, Balls: {over.Deliveries?.Count}");
                foreach (var ball in over.Deliveries)
                {
                    Debug.WriteLine($"Ball: {over.Batter1} & {over.Batter2}, Runs: {ball.Runs}, Wicket: {ball.IsWicket}");
                }
            }

            // Both innings complete
            MatchResult result = MatchConverter.BuildMatchResult(currentMatch);
            await MatchSaver.SaveMatchResultAsync(result);
            await Navigation.PushAsync(new SummaryPage(result));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save match result: {ex.Message}", "OK");
            // Optional: log deeper details
            System.Diagnostics.Debug.WriteLine($"[EXCEPTION] {ex}");
        }
    }

    private async void OnCustomRunClicked(object sender, EventArgs e)
    {
        if (matchEnded) return;

        string result = await DisplayPromptAsync("Custom Runs", "Enter number of runs:",
                                                  accept: "OK", cancel: "Cancel",
                                                  keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(result))
        {
            if (int.TryParse(result, out int customRuns) && customRuns >= 0)
            {
                AddRuns(customRuns);
            }
            else
            {
                await DisplayAlert("Invalid Input", "Please enter a valid number of runs.", "OK");
            }
        }
    }

    private void OnTwoRunsClicked(object sender, EventArgs e)
    {
        AddRuns(2);
    }
}