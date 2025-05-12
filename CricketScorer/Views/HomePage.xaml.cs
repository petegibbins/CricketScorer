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

        private async void OnNewMatchDefaultTeamsClicked(object sender, EventArgs e)
        {
            var match = new Match
            {
                TeamA = "Twistas",
                TeamB = "Bury",
                MatchDate = DateTime.Now,
                TeamAPlayers = new List<string> { "Kitch", "Katrina", "Gemma", "Di", "Emily", "Kate", "Bianka", "Babs" },
                TeamBPlayers = new List<string> { "Lisa", "Marta", "Antonia", "Lana", "Maya", "Nina", "Olive", "Paula" },
                TeamAScore = 200,
                TeamBScore = 200,
                Format = Match.MatchFormat.Standard,
                StartingRuns = 200,
                TotalOvers = 8,
                Runs = 200
            };
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
                TeamB = "Bury",
                MatchDate = DateTime.Now,
                TeamAPlayers = new List<string> { "Kitch", "Katrina", "Gemma", "Di", "Emily", "Kate", "Bianka", "Babs" },
                TeamBPlayers = new List<string> { "Lisa", "Marta", "Antonia", "Lana", "Maya", "Nina", "Olive", "Paula" },
                TeamAScore = 200,
                TeamBScore = 200,
                Format = Match.MatchFormat.Standard,
                TotalOvers = 8,
            };

            // Batting pairs per innings
            var pairsA = new List<(string, string)>
                {
                    ("Kitch", "Katrina"),
                    ("Gemma", "Di"),
                    ("Emily", "Kate"),
                    ("Bianka", "Babs")
                };

            var pairsB = new List<(string, string)>
                {
                    ("Lisa", "Marta"),
                    ("Antonia", "Lana"),
                    ("Maya", "Nina"),
                    ("Olive", "Paula")
                };

            var bowlersA = new List<string> { "Lisa", "Marta", "Antonia", "Lana" };
            var bowlersB = new List<string> { "Kitch", "Katrina", "Gemma", "Di" };

            var rand = new Random();

            // First innings (Team A batting)
            for (int i = 0; i < 8; i++)
            {
                var over = new Over
                {
                    Bowler = bowlersA[i % bowlersA.Count],
                    Batter1 = pairsA[i / 2].Item1,
                    Batter2 = pairsA[i / 2].Item2,
                    IsFirstInning = true,
                    Deliveries = new List<Ball>()
                };

                for (int b = 0; b < 6; b++)
                {
                    var isWicket = rand.NextDouble() < 0.15;
                    var runs = isWicket ? 0 : rand.Next(0, 3); // 0–2 runs

                    over.Deliveries.Add(new Ball
                    {
                        Batters = (b % 2 == 0) ? over.Batter1 : over.Batter2,
                        Runs = runs,
                        IsWicket = isWicket,
                        DismissalType = "Caught"
                    });

                    match.TeamAScore += runs;
                    if (isWicket)
                        match.TeamAScore -= 5;
                }

                match.FirstInningsOvers.Add(over);
            }

            // Second innings (Team B batting)
            for (int i = 0; i < 8; i++)
            {
                var over = new Over
                {
                    Bowler = bowlersB[i % bowlersB.Count],
                    Batter1 = pairsB[i / 2].Item1,
                    Batter2 = pairsB[i / 2].Item2,
                    IsFirstInning = false,
                    Deliveries = new List<Ball>()
                };

                for (int b = 0; b < 6; b++)
                {
                    var isWicket = rand.NextDouble() < 0.1;
                    var runs = isWicket ? 0 : rand.Next(0, 4); // 0–3 runs

                    over.Deliveries.Add(new Ball
                    {
                        Batters = (b % 2 == 0) ? over.Batter1 : over.Batter2,
                        Runs = runs,
                        IsWicket = isWicket,
                        DismissalType = isWicket ? "Run Out" : null
                    });

                    match.TeamBScore += runs;
                    if (isWicket)
                        match.TeamBScore -= 5;
                }

                match.SecondInningsOvers.Add(over);
            }
            match.IsFirstInningsComplete = true;
            match.IsFirstInnings = false;
            match.Runs = match.TeamBScore;
            match.Wickets = match.TeamBWickets;
            match.OversDetails = match.SecondInningsOvers;
            match.TeamAOvers = match.FirstInningsOvers.Count;
            match.TeamBOvers = match.SecondInningsOvers.Count;
            await Navigation.PushAsync(new ScoringPage(match));
        }
    

    private async void OnStartDemo100MatchClicked(object sender, EventArgs e)
        {
            var match = new Match
            {
                TeamA = "Twistas",
                TeamB = "Bury",
                MatchDate = DateTime.Now,
                TeamAPlayers = new List<string> { "Kitch", "Katrina", "Gemma", "Di", "Emily", "Kate", "Bianka", "Babs" },
                TeamBPlayers = new List<string> { "Lisa", "Marta", "Antonia", "Lana", "Maya", "Nina", "Olive", "Paula" },
                TeamAScore = 200,
                TeamBScore = 200,
                Format = Match.MatchFormat.Hundred,
                TotalOvers = 20,
            };

            // Batting pairs per innings
            var pairsA = new List<(string, string)>
                {
                    ("Kitch", "Katrina"),
                    ("Gemma", "Di"),
                    ("Emily", "Kate"),
                    ("Bianka", "Babs")
                };

            var pairsB = new List<(string, string)>
                {
                    ("Lisa", "Marta"),
                    ("Antonia", "Lana"),
                    ("Maya", "Nina"),
                    ("Olive", "Paula")
                };

            var bowlersA = new List<string> { "Lisa", "Marta", "Antonia", "Lana", "Lisa", "Marta", "Antonia", "Lana", "Lisa", "Marta", "Lisa", "Marta", "Antonia", "Lana", "Lisa", "Marta", "Antonia", "Lana", "Lisa", "Marta" };
            var bowlersB = new List<string> { "Kitch", "Katrina", "Gemma", "Di", "Kitch", "Katrina", "Gemma", "Di", "Kitch", "Katrina", "Kitch", "Katrina", "Gemma", "Di", "Kitch", "Katrina", "Gemma", "Di", "Kitch", "Katrina" };

            var rand = new Random();

            // First innings (Team A batting)
            for (int i = 0; i < match.TotalOvers; i++)
            {
                var over = new Over
                {
                    Bowler = bowlersA[i % bowlersA.Count],
                    Batter1 = pairsA[i / 5].Item1,
                    Batter2 = pairsA[i / 5].Item2,
                    IsFirstInning = true,
                    Deliveries = new List<Ball>()
                };

                for (int b = 0; b < match.BallsPerOver; b++)
                {
                    var isWicket = rand.NextDouble() < 0.15;
                    var runs = isWicket ? 0 : rand.Next(0, 3); // 0–2 runs

                    over.Deliveries.Add(new Ball
                    {
                        Batters = (b % 2 == 0) ? over.Batter1 : over.Batter2,
                        Runs = runs,
                        IsWicket = isWicket,
                        DismissalType = "Caught"
                    });

                    match.TeamAScore += runs;
                    if (isWicket)
                        match.TeamAScore -= 5;
                }

                match.FirstInningsOvers.Add(over);
            }

            // Second innings (Team B batting)
            for (int i = 0; i < match.TotalOvers; i++)
            {
                var over = new Over
                {
                    Bowler = bowlersB[i % bowlersB.Count],
                    Batter1 = pairsB[i / 5].Item1,
                    Batter2 = pairsB[i / 5].Item2,
                    IsFirstInning = false,
                    Deliveries = new List<Ball>()
                };

                for (int b = 0; b < match.BallsPerOver; b++)
                {
                    var isWicket = rand.NextDouble() < 0.1;
                    var runs = isWicket ? 0 : rand.Next(0, 4); // 0–3 runs

                    over.Deliveries.Add(new Ball
                    {
                        Batters = (b % 2 == 0) ? over.Batter1 : over.Batter2,
                        Runs = runs,
                        IsWicket = isWicket,
                        DismissalType = isWicket ? "Run Out" : null
                    });

                    match.TeamBScore += runs;
                    if (isWicket)
                        match.TeamBScore -= 5;
                }

                match.SecondInningsOvers.Add(over);
            }
            match.IsFirstInningsComplete = true;
            match.IsFirstInnings = false;
            match.Runs = match.TeamBScore;
            match.Wickets = match.TeamBWickets;
            match.OversDetails = match.SecondInningsOvers;
            match.TeamAOvers = match.FirstInningsOvers.Count;
            match.TeamBOvers = match.SecondInningsOvers.Count;
            await Navigation.PushAsync(new ScoringPage(match));
        }
    }

    }