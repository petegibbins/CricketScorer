using CricketScorer.Models;

namespace CricketScorer.Views;

public partial class ScoringPage : ContentPage
{

    private Match currentMatch;
    private int ballsInCurrentOver = 0;
    private Over currentOver = new Over();
    private bool isFirstInnings = true;
    private int teamATotalRuns = 0;
    private int teamAWickets = 0;
    private int teamAOvers = 0;
    private bool matchEnded = false;

    public ScoringPage(Match match)
    {
        InitializeComponent();
        currentMatch = match;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        ScoreLabel.Text = $"Score: {currentMatch.Runs}/{currentMatch.Wickets}";
        OverLabel.Text = $"Overs: {currentMatch.OversDetails.Count}";

        var overs = currentMatch.OversDetails.ToList();


        if (!isFirstInnings)
        {
            int target = teamATotalRuns + 1; // Must beat Team A's total
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
        if (currentOver.Balls.Count > 0)
        {
            overs.Add(currentOver);
        }

        if (overs.Count >= 1)
        {
            var lastOver = overs.Last();
            LastOver1Label.Text = $"Over {overs.Count}: " + string.Join(" ", lastOver.Balls.Select(b => b.ToString()));
        }
        else
        {
            LastOver1Label.Text = "-";
        }

        if (overs.Count >= 2)
        {
            var secondLastOver = overs[^2];
            LastOver2Label.Text = $"Over {overs.Count - 1}: " + string.Join(" ", secondLastOver.Balls.Select(b => b.ToString()));
        }
        else
        {
            LastOver2Label.Text = "-";
        }
    }

    private void AddRuns(int runs, bool isWide = false, bool isNoBall = false)
    {
        if (matchEnded) return; // Prevent scoring if match already won
        currentMatch.Runs += runs;

        ballsInCurrentOver++;

        currentOver.Balls.Add(new Ball { Runs = runs, IsWide = isWide, IsNoBall = isNoBall });
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
        currentOver.Balls.Add(new Ball { Runs = 0, IsWicket = true });

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

    private void EndOver()
    {
        if (currentOver.Balls.Count > 0)
        {
            currentMatch.OversDetails.Add(currentOver);
            currentOver = new Over(); // Start a fresh over
        }

        ballsInCurrentOver = 0;
        UpdateScoreDisplay();
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
        if (currentOver.Balls.Count > 0)
        {
            var lastBall = currentOver.Balls.Last();

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
            currentOver.Balls.Remove(lastBall);

            ballsInCurrentOver--;
            if (ballsInCurrentOver < 0) ballsInCurrentOver = 0; // Safety

            UpdateScoreDisplay();
        }
        else if (currentMatch.OversDetails.Count > 0)
        {
            // Step 4: If no balls in current over, pop back to previous over
            currentOver = currentMatch.OversDetails.Last();
            currentMatch.OversDetails.Remove(currentOver);

            if (currentOver.Balls.Count > 0)
            {
                var lastBall = currentOver.Balls.Last();

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

                currentOver.Balls.Remove(lastBall);
                ballsInCurrentOver = currentOver.Balls.Count;
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
            if (currentOver.Balls.Count > 0)
            {
                currentMatch.OversDetails.Add(currentOver);
                currentOver = new Over();
            }

            if (isFirstInnings)
            {
                // Team A has finished batting
                isFirstInnings = false;

                // Save Team A's final score (store it separately)
                teamATotalRuns = currentMatch.Runs;
                teamAWickets = currentMatch.Wickets;
                teamAOvers = currentMatch.OversDetails.Count;

                // Reset match for Team B innings
                currentMatch.Runs = 200; // New starting runs
                currentMatch.Wickets = 0;
                currentMatch.OversDetails.Clear();
                ballsInCurrentOver = 0;
                currentOver = new Over();

                await DisplayAlert("New Innings", $"{currentMatch.TeamB} now batting to chase {teamATotalRuns} runs!", "OK");

                UpdateScoreDisplay();
            }
            else
            {
                // Both innings complete — now finish match
                await Navigation.PushAsync(new SummaryPage(currentMatch, teamATotalRuns, teamAWickets, teamAOvers));
            }
        }
    }
    private async void CheckForMatchWin()
    {
        if (!isFirstInnings)
        {
            int target = teamATotalRuns + 1;

            if (currentMatch.Runs >= target && !matchEnded)
            {
                matchEnded = true;

                await DisplayAlert("Match Won", $"{currentMatch.TeamB} has won the match!", "OK");

                bool endNow = await DisplayAlert("End Match?", "Would you like to end the match now?", "Yes", "No");
                if (endNow)
                {
                    await Navigation.PushAsync(new SummaryPage(currentMatch, teamATotalRuns, teamAWickets, teamAOvers));
                }
            }
        }
    }

    private void OnButtonPressed(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            btn.ScaleTo(0.95, 50); // shrink to 95% size in 50ms
        }
    }

    private void OnButtonReleased(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            btn.ScaleTo(1.0, 50); // return to normal size
        }
    }
}