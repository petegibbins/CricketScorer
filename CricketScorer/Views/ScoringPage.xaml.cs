using CricketScorer.Core.Models;
using CricketScorer.Core.Services;
using CricketScorer.Helpers;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CricketScorer.Views;

public partial class ScoringPage : ContentPage
{

    private Match currentMatch;
    private int ballsInCurrentOver = 0;
    private Over currentOver = new Over();
    private bool matchEnded = false;
    private bool showBowlerPopup;
    private List<string> availableBowlers = new();
    private bool isSelectingBowler = false;
    private bool pendingEndOver = false; // Flag to check if end over is pending so we can show the popup again if needed
    private ObservableCollection<string> bowlerNames = new();

    public ScoringPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
        NavigationPage.SetHasNavigationBar(this, false);
        NavigationPage.SetHasBackButton(this, false);
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

        ScoreLabel.Text = $"Score: {currentMatch.Runs}/{currentMatch.Wickets}";
        Debug.WriteLine($"!!! Score label text: {ScoreLabel.Text}");
        OverLabel.Text = $"Overs: {currentMatch.OversDetails.Count}/{currentMatch.TotalOvers}";
        Debug.WriteLine($"!!! Over label text: {OverLabel.Text}");
        BattingTeamLabel.Text = currentMatch.IsFirstInnings ? currentMatch.TeamA : currentMatch.TeamB;
        Debug.WriteLine($"!!! Batting label = {BattingTeamLabel.Text}");

        var overs = currentMatch.OversDetails.ToList();
        var teamPlayers = currentMatch.IsFirstInnings ? currentMatch.TeamARoster : currentMatch.TeamBRoster;
        Debug.WriteLine($"!!! Is first innings? {currentMatch.IsFirstInnings.ToString()}");
        // ✅ NEW: Use override if one exists
        string batter1 = "???";
        string batter2 = "???";
        int pairIndex = currentMatch.CurrentPairIndex;

        if (teamPlayers.Count >= (pairIndex * 2 + 2))
        {
            var pairOverride = currentMatch.GetActivePairOverrides()
                .FirstOrDefault(p => p.PairIndex == pairIndex);

            if (pairOverride != null)
            {
                batter1 = pairOverride.Batter1;
                batter2 = pairOverride.Batter2;
            }
            else
            {
                batter1 = teamPlayers[pairIndex * 2];
                batter2 = teamPlayers[pairIndex * 2 + 1];
            }

            BattingPairLabel.Text = $"{batter1} & {batter2}";
        }
        else
        {
            BattingPairLabel.Text = "No Active Batters";
        }

        if (!currentMatch.IsFirstInnings)
        {
            var formatter = new Formatter();
            TargetLabel.Text = formatter.FormatTargetLabel(currentMatch);
        }
        else
        {
            TargetLabel.Text = string.Empty;
        }
        currentOver.Batter1 = batter1;
        currentOver.Batter2 = batter2;

        Debug.WriteLine($"!!! Current Batters: {currentOver.Batter1} & {currentOver.Batter2}");
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

            if (ballsRemaining > currentMatch.BallsPerOver)
            {
                double requiredRate = runsRemaining / (ballsRemaining / (double)currentMatch.BallsPerOver);
                RequiredRunRateLabel.Text = $"Required Run Rate: {requiredRate:0.00}";
                RequiredRunRateLabel.IsVisible = true;
            }
            else
            {
                RequiredRunRateLabel.Text = $"{runsRemaining} runs required";
                RequiredRunRateLabel.IsVisible = false;
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

        if (currentMatch.IsFirstInnings)
        {
            currentMatch.TeamAScore = currentMatch.Runs;
        }
        else
        {
            currentMatch.TeamBScore = currentMatch.Runs;
        }
        ballsInCurrentOver++;
        currentOver.Deliveries.Add(ball);
        currentOver.IsFirstInning = currentMatch.IsFirstInnings;

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

            // If the wicket was the last ball of the over, we need to hold the EndOver pop up until the dismissal is confirmed
            if (pendingEndOver)
            {
                pendingEndOver = false;
                EndOver(); // Now run it safely
            }
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
            if (WicketPopup.IsVisible)
            {
                pendingEndOver = true;
            }
            else
            {
                EndOver();
            }
        }
    }

    private async void EndOver()
    {
        if (currentOver.Deliveries.Count > 0)
        {
            currentMatch.OversDetails.Add(currentOver);
            currentMatch.OverBowlers.Add(currentMatch.CurrentBowler);
            currentOver = new Over(); // New over
        }

        ballsInCurrentOver = 0;
        UpdateScoreDisplay();

        if (currentMatch.OversDetails.Count >= currentMatch.TotalOvers)
        {
            bool endNow = await DisplayAlert("Overs Complete", "The maximum number of overs is reached. End the innings now?", "Yes", "No");
            if (endNow)
            {
                await EndInningsManually();
                return;
            }
        }

        int oversBowled = currentMatch.OversDetails.Count;
        var pairList = currentMatch.GetActivePairOverrides();

        if (currentMatch.OversPerPair >= 1 &&
            pairList.Count > 0 &&
            oversBowled % currentMatch.OversPerPair == 0)
        {
            currentMatch.CurrentPairIndex = (currentMatch.CurrentPairIndex + 1) % pairList.Count;

            var currentPair = pairList[currentMatch.CurrentPairIndex];
            await DisplayAlert("Batting Pair Changed", $"New pair: {currentPair.Batter1} and {currentPair.Batter2}", "OK");
            UpdateScoreDisplay();
        }

        await SelectNextBowler();
    }

    private void ShowBattingPairPopup()
    {

        var allBatters = currentMatch.IsFirstInnings ? currentMatch.TeamARoster : currentMatch.TeamBRoster;

        var batters = allBatters
            .Distinct()
            .Select(name => new BatterItem { Name = name, IsSelected = false })
            .ToList();

        BattingPairList.ItemsSource = batters;
        ConfirmBattingPairButton.IsEnabled = false;

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
        var selected = (BattingPairList.ItemsSource as List<BatterItem>)
                        ?.Where(x => x.IsSelected)
                        .Select(x => x.Name)
                        .ToList();

        if (selected.Count != 2)
        {
            DisplayAlert("Invalid", "Please select exactly 2 batters.", "OK");
            return;
        }

        // Store override instead of overwriting TeamAPlayers
        var existing = currentMatch.GetActivePairOverrides().FirstOrDefault(p => p.PairIndex == currentMatch.CurrentPairIndex);
        if (existing != null)
        {
            existing.Batter1 = selected[0];
            existing.Batter2 = selected[1];
        }
        else
        {
            currentMatch.GetActivePairOverrides().Add(new PairOverride
            {
                PairIndex = currentMatch.CurrentPairIndex,
                Batter1 = selected[0],
                Batter2 = selected[1]
            });
        }

        BattingPairPopup.IsVisible = false;
        BattingPairDim.IsVisible = false;

        var teamList = currentMatch.IsFirstInnings ? currentMatch.TeamAPlayers : currentMatch.TeamBPlayers;

        int i1 = currentMatch.CurrentPairIndex * 2;
        int i2 = i1 + 1;

        // Make sure the list is long enough
        while (teamList.Count <= i2)
        {
            teamList.Add("Unknown");
        }

        teamList[i1] = selected[0];
        teamList[i2] = selected[1];
        currentOver.Batter1 = selected[0];
        currentOver.Batter2 = selected[1];

        UpdateBattingPairDisplay();

        Debug.WriteLine($"Updated batters: {currentOver.Batter1} & {currentOver.Batter2}");
        Debug.WriteLine("Current batting list:");
    }


    private async void OnBattingPairLabelTapped(object sender, TappedEventArgs e)
    {
        // Load the batting options into the popup (if needed)
        ShowBattingPairPopup(); // or however you're loading this

        BattingPairPopup.IsVisible = true;
        BattingPairDim.IsVisible = true;
    }

    private void OnBatterTapped(object sender, TappedEventArgs e)
    {
        Frame frame = null;

        if (sender is Frame f)
        {
            frame = f;
        }
        else if (sender is VisualElement ve && ve.Parent is Frame pf)
        {
            frame = pf;
        }
        if (frame?.BindingContext is BatterItem item)
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
            var source = currentMatch.IsFirstInnings ? currentMatch.TeamBRoster : currentMatch.TeamARoster;

            bowlerNames.Clear();
            foreach (var name in source)
                bowlerNames.Add(name);

            BowlerList.ItemsSource = bowlerNames;


            List<string> bowlers = currentMatch.IsFirstInnings ? currentMatch.TeamBRoster : currentMatch.TeamARoster; // Bowling team is the fielding side
            if (bowlers.Count == 0)
            {
                string name = await DisplayPromptAsync("Bowler's Name", "Enter name of the bowler:");
                if (!bowlerNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    bowlerNames.Add(name);
                    source.Add(name);
                    await PlayerNameService.AddAsync(name);
                }
                return;
            }


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
            var batters = currentMatch.IsFirstInnings ? currentMatch.TeamAPlayers : currentMatch.TeamBPlayers;
            currentOver.Batter1 = batters[currentMatch.CurrentPairIndex * 2];
            currentOver.Batter2 = batters[currentMatch.CurrentPairIndex * 2 + 1];
            currentOver.IsFirstInning = currentMatch.IsFirstInnings;

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


    private void OnAddNewBowlerClicked(object sender, EventArgs e)
    {
        if (!NewBowlerEntry.IsVisible)
        {
            NewBowlerEntry.Text = string.Empty;
            NewBowlerEntry.IsVisible = true;
            NewBowlerEntry.Focus();
            AddNewBowlerButton.Text = "Confirm Add";
        }
        else
        {
            var name = NewBowlerEntry.Text?.Trim();
            if (!string.IsNullOrWhiteSpace(name) && !bowlerNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                bowlerNames.Add(name);
                BowlerList.ItemsSource = null;
                BowlerList.ItemsSource = bowlerNames;

                NewBowlerEntry.IsVisible = false;
                AddNewBowlerButton.Text = "Add New Bowler";
                NewBowlerEntry.Text = string.Empty;

                // Optional: auto-select newly added
                currentMatch.CurrentBowler = name;
                currentMatch.OverBowlers.Add(currentMatch.CurrentBowler);
            }
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

        if (currentMatch.IsFirstInnings)
        {
            currentMatch.IsFirstInnings = false;
            currentMatch.CurrentPairIndex = 0;
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
            UpdateBattingPairDisplay();
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

        // Don't end the match early if it's the second innings
        if (!currentMatch.IsFirstInnings)
        {
            // If all overs have been bowled, end the game
            if (currentMatch.OversDetails.Count >= currentMatch.TotalOvers)
            {
                await EndGame();
            }
            else
            {
                // Update the target label
                var formatter = new Formatter();
                TargetLabel.Text = formatter.FormatTargetLabel(currentMatch);
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