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
    private bool showBowlerPopup;
    private List<string> availableBowlers = new();
    private bool isSelectingBowler = false;


    public ScoringPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
        //UpdateScoreDisplay();
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);
        // showBowlerPopup = true;
        // BowlerPopup.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Start page invisible
        this.Opacity = 0;

        // Fade to fully visible over 500 milliseconds
        await this.FadeTo(1, 500, Easing.CubicInOut);

        if (!string.IsNullOrEmpty(currentMatch.CurrentBowler))
        {
            currentOver = currentMatch.CurrentOver ?? new Over();
            UpdateBowlerDisplay(); // optional
        }

        if (currentMatch.OversDetails.Count == 0 && string.IsNullOrEmpty(currentMatch.CurrentBowler))
        {
            await SelectNextBowler();   
        }
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (currentMatch.IsFirstInningsComplete)
        {
            isFirstInnings = false;
        }
        currentOver.IsFirstInning = isFirstInnings;

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

        LastOversLabel.FormattedText = BuildFormattedLastOversString();
        UpdateRequiredRunRate();
    }

    private FormattedString BuildFormattedLastOversString()
    {
        var formatted = new FormattedString();
        var overs = currentMatch.OversDetails.ToList();

        bool isAlreadyIncluded = overs.Contains(currentOver);
        bool isCurrentOverActive = currentOver?.Deliveries?.Count > 0;

        if (!isAlreadyIncluded && isCurrentOverActive)
            overs.Add(currentOver);

        var lastThree = overs.TakeLast(3).ToList();

        for (int i = 0; i < lastThree.Count; i++)
        {
            var over = lastThree[i];
            bool isComplete = over.Deliveries.Count == currentMatch.BallsPerOver;
            bool isLast = i == lastThree.Count - 1;

            foreach (var ball in over.Deliveries)
            {
                var text = ball.IsWicket ? "w" :
                           ball.Runs == 0 ? "." :
                           ball.IsNoBall ? "nb" :
                           ball.IsWide ? "wd" :
                           ball.Runs.ToString();

                formatted.Spans.Add(new Span
                {
                    Text = text + " ",
                    TextColor = ball.IsWicket ? Colors.Red : Colors.Black,
                    FontFamily = "Courier New"
                });
            }

            // Add pipe if the over is complete (or it's not the last in-progress over)
            if (isComplete || !isLast)
            {
                formatted.Spans.Add(new Span
                {
                    Text = "| ",
                    TextColor = Colors.Gray,
                    FontFamily = "Courier New"
                });
            }
        }

        return formatted;
    }


    private void UpdateRequiredRunRate()
    {
        if (!currentMatch.IsFirstInnings)
        {
            double runsRemaining = currentMatch.TeamAScore - currentMatch.TeamBScore + 1;
            int ballsBowled = currentMatch.OversDetails.Sum(o => o.Deliveries.Count);
            int ballsRemaining = currentMatch.TotalOvers * currentMatch.BallsPerOver - ballsBowled;

            if (ballsRemaining > (double)currentMatch.BallsPerOver)
            {
                double requiredRate = runsRemaining / (ballsRemaining / (double)currentMatch.BallsPerOver);
                RequiredRunRateLabel.Text = $"Required Run Rate: {requiredRate:0.00}";
                RequiredRunRateLabel.IsVisible = true;
            }
            else
            {
                RequiredRunRateLabel.Text = "Innings Complete";
                RequiredRunRateLabel.IsVisible = true;
            }
        }
        else
        {
            RequiredRunRateLabel.IsVisible = false;
        }
    }

    private void AddRuns(Ball ball)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        currentMatch.Runs += ball.Runs;

        if (ball.IsWicket)
        {
         currentMatch.Runs -= 5; // Wicket penalty  
        }

        if (isFirstInnings)
        {
            currentMatch.TeamAScore = currentMatch.Runs;
        }
        else
        {
            currentMatch.TeamBScore = currentMatch.Runs;
        }
        ballsInCurrentOver++;
        currentOver.Deliveries.Add(ball);
        currentOver.IsFirstInning = isFirstInnings;

        CheckForMatchWin();
        CheckOverComplete();
        UpdateScoreDisplay();
    }

    private void AddRuns(int runs, bool isWide = false, bool isNoBall = false)
    {
        AddRuns(new Ball
        {
            Runs = runs,
            IsWide = isWide,
            IsNoBall = isNoBall
        });
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


    private void OnWicketClicked(object sender, EventArgs e)
    {
        var options = new List<string> { "Bowled", "Caught", "Run Out", "LBW", "Stumped", "Hit Wicket", "Other" };
        var dismissalItems = options.Select(label => new DismissalOption { Label = label }).ToList();

        DismissalList.ItemsSource = dismissalItems;
        WicketPopup.IsVisible = true;
        WicketDim.IsVisible = true;
        AddRuns(new Ball() { IsWicket = true});
    }


    private void OnDismissalTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is DismissalOption tapped)
        {
            var list = DismissalList.ItemsSource as List<DismissalOption>;
            foreach (var option in list)
                option.IsSelected = false;

            tapped.IsSelected = true;

            // Refresh the UI
            DismissalList.ItemsSource = null;
            DismissalList.ItemsSource = list;

            // Proceed with recording the wicket
            var ball = new Ball
            {
                Runs = 0,
                IsWicket = true,
                DismissalType = tapped.Label,
                Batters = $"{currentOver.Batter1} & {currentOver.Batter2}"
            };

            AddRuns(ball);
            currentMatch.Wickets++;

            WicketPopup.IsVisible = false;
            WicketDim.IsVisible = false;

            UpdateScoreDisplay();
        }
    }

    private void OnCancelDismissalPopup(object sender, EventArgs e)
    {
        WicketPopup.IsVisible = false;
        WicketDim.IsVisible = false;
    }

    private void OnEndOverClicked(object sender, EventArgs e)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        EndOver();
    }

    private void CheckOverComplete()
    {
        if (currentOver.Deliveries.Count == currentMatch.BallsPerOver)
        {
            EndOver();
        }
    }

    private async void EndOver()
    {
        if (currentOver.Deliveries.Count > 0)
        {
            currentMatch.OversDetails.Add(currentOver);
            currentMatch.OverBowlers.Add(currentMatch.CurrentBowler);
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

    private void ShowBattingPairPopup()
    {
        var batters = currentMatch.IsFirstInnings
            ? currentMatch.TeamAPlayers
            : currentMatch.TeamBPlayers;


        BattingPairList.ItemsSource = batters
            .Select(name => new BatterItem { Name = name, IsSelected = false })
            .ToList();

        BattingPairPopup.IsVisible = true;
        BattingPairDim.IsVisible = true;
    }

    private void OnChangeBattingPairClicked(object sender, EventArgs e)
    {
        ShowBattingPairPopup();
    }
    private void OnBattingPairSelected(object sender, SelectionChangedEventArgs e)
    {
        ConfirmBattingPairButton.IsEnabled = e.CurrentSelection.Count == 2;
    }

    private void OnConfirmBattingPair(object sender, EventArgs e)
    {
        var items = BattingPairList.ItemsSource as List<BatterItem>;
        var selected = items.Where(x => x.IsSelected).ToList();

        if (selected.Count != 2)
        {
            DisplayAlert("Invalid", "Please select exactly 2 batters.", "OK");
            return;
        }

        currentOver.Batter1 = selected[0].Name;
        currentOver.Batter2 = selected[1].Name;

        BattingPairPopup.IsVisible = false;
        BattingPairDim.IsVisible = false;

        UpdateBattingPairDisplay();

        Debug.WriteLine($"Updated batters: {currentOver.Batter1} & {currentOver.Batter2}");
    }

    private void OnBatterTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is BatterItem item)
        {
            var list = (BattingPairList.ItemsSource as List<BatterItem>);
            if (item.IsSelected)
            {
                item.IsSelected = false;
            }
            else if (list.Count(x => x.IsSelected) < 2)
            {
                item.IsSelected = true;
            }
            else
            {
                DisplayAlert("Limit", "You can only select 2 batters.", "OK");
            }

            // Refresh UI — MAUI doesn’t auto-update bindings in list
            BattingPairList.ItemsSource = null;
            BattingPairList.ItemsSource = list;

            ConfirmBattingPairButton.IsEnabled = list.Count(x => x.IsSelected) == 2;
        }
    }

    private void UpdateBattingPairDisplay()
    {
        BattingPairLabel.Text = $"{currentOver.Batter1} & {currentOver.Batter2}";
    }

    private void OnCancelBattingPair(object sender, EventArgs e)
    {
        BattingPairPopup.IsVisible = false;
        BattingPairDim.IsVisible = false;
    }
    private async Task SelectNextBowler()
    {
        if (isSelectingBowler) return; // already running or visible

        isSelectingBowler = true;

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
            currentOver.IsFirstInning = isFirstInnings;

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EXCEPTION] {ex}");
        }
        finally
        {
            isSelectingBowler = false; // reset flag when done
        }
    }



    private void OnBowlerSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedBowler)
        {
            currentMatch.CurrentBowler = selectedBowler;
            currentMatch.OverBowlers.Add(currentMatch.CurrentBowler);
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
            currentMatch.CurrentBowler = selectedBowler;
            currentMatch.OverBowlers.Add(currentMatch.CurrentBowler);
            currentOver.Bowler = currentMatch.CurrentBowler;

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
        isSelectingBowler = false; // reset flag
    }

    private void UpdateBowlerDisplay()
    {
        CurrentBowlerLabel.Text = $"{currentMatch.CurrentBowler}";
    }

    private async void OnWideClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Wide Ball", "Total runs scored including penalty (default is 2):",
                                                 initialValue: "2",
                                                 keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(result) && int.TryParse(result, out int totalRuns) && totalRuns >= 1)
        {
            AddRuns(new Ball
            {
                Runs = totalRuns,
                IsWide = true
            });
        }
        else
        {
            await DisplayAlert("Invalid", "Please enter a valid number greater than or equal to 1.", "OK");
        }
    }

    private async void OnNoBallClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("No Ball", "Total runs scored including penalty (default is 2):",
                                                 initialValue: "2",
                                                 keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(result) && int.TryParse(result, out int totalRuns) && totalRuns >= 1)
        {
            AddRuns(new Ball
            {
                Runs = totalRuns,
                IsNoBall = true
            });
        }
        else
        {
            await DisplayAlert("Invalid", "Please enter a valid number greater than or equal to 1.", "OK");
        }
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
            currentMatch.FirstInningsOvers = currentMatch.OversDetails.ToList();
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
            currentMatch.SecondInningsOvers = currentMatch.OversDetails.ToList();
            await EndGame();
        }
    }

    private async void CheckForMatchWin()
    {
        currentMatch.CurrentOver = currentOver;
        await MatchSaver.SaveMatchState(currentMatch);

        if (!isFirstInnings)
        {
            int target = currentMatch.TeamAScore + 1;

            if (currentMatch.Runs >= target && !matchEnded)
            {
                matchEnded = true;
                currentMatch.TeamBScore = currentMatch.Runs;

                // Fix: Ensure last over is added, even if incomplete
                if (currentOver.Deliveries.Count > 0)
                {
                    currentMatch.OversDetails.Add(currentOver);
                }

                currentMatch.SecondInningsOvers = currentMatch.OversDetails.ToList();
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

            // Ensure the last over is added, even if incomplete
            if (!currentMatch.OversDetails.Contains(currentOver) && currentOver.Deliveries.Count > 0)
            {
                currentMatch.OversDetails.Add(currentOver);
            }

            // Both innings complete
            currentMatch.CurrentOver = currentOver;
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
            int.TryParse(result, out int customRuns);
            // Simulate some custom run scenarios for testing
            if (customRuns == -100)
            {
                throw new InvalidOperationException("Simulated exception crash (-100)");
            }
            else if (customRuns == -200)
            {
                Environment.FailFast("Simulated app termination (-200)");
            }
            else if (customRuns == -300)
            {
                throw new DivideByZeroException("Divide by zero simulated (-300)");
            }

            if ((customRuns >= 0))
            {
                // Actually add the custom runs
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