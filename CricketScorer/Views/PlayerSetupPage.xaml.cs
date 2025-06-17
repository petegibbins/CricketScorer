using CricketScorer.Core.Models;
using CricketScorer.Core.Services;
using CricketScorer.Helpers;
using CricketScorer.Views;
using System.Text.RegularExpressions;
using Microsoft.Maui.Graphics;

namespace CricketScorer;

public partial class PlayerSetupPage : ContentPage
{
    private Core.Models.Match _match;
    private List<Entry> _teamAEntries = new();
    private List<Entry> _teamBEntries = new();

    public PlayerSetupPage(Core.Models.Match match)
    {
        InitializeComponent();
        _match = match;

        PrefillPlayerFields(_match.TeamAPlayers, TeamAPlayersStack, _teamAEntries);
        PrefillPlayerFields(_match.TeamBPlayers, TeamBPlayersStack, _teamBEntries);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        TeamALabel.Text = _match.TeamA;
        TeamBLabel.Text = _match.TeamB;

        await ReloadTeamPickers();
    }

    private async void OnTeamAPickerChanged(object sender, EventArgs e)
    {
        if (TeamAPicker.SelectedItem is string selectedTeamName)
        {
            var team = await TeamService.GetTeamByNameAsync(selectedTeamName);
            if (team != null)
            {
                _match.TeamA = team.TeamName;
                _teamAEntries.Clear(); // Clear old fields
                TeamAPlayersStack.Children.Clear();
                PrefillPlayerFields(team.Players, TeamAPlayersStack, _teamAEntries);
                TeamALabel.Text = team.TeamName;
            }
        }
    }

    private async void OnTeamBPickerChanged(object sender, EventArgs e)
    {
        if (TeamBPicker.SelectedItem is string selectedTeamName)
        {
            var team = await TeamService.GetTeamByNameAsync(selectedTeamName);
            if (team != null)
            {
                _match.TeamB = team.TeamName;
                _teamBEntries.Clear(); // Clear old fields
                TeamBPlayersStack.Children.Clear();
                PrefillPlayerFields(team.Players, TeamBPlayersStack, _teamBEntries);
                TeamBLabel.Text = team.TeamName;
            }
        }
    }

    private async void OnSaveTeamAClicked(object sender, EventArgs e)
    {
        string teamName = await DisplayPromptAsync("Save Team A", "Enter a name for this team:");
        if (!string.IsNullOrWhiteSpace(teamName))
        {
            var team = new SavedTeam
            {
                TeamName = teamName,
                Players = _teamAEntries
                    .Select(e => e.Text?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
            };
            await TeamService.SaveTeamAsync(team);
            await ReloadTeamPickers();
            TeamALabel.Text = team.TeamName;
        }
    }

    private async void OnSaveTeamBClicked(object sender, EventArgs e)
    {
        string teamName = await DisplayPromptAsync("Save Team B", "Enter a name for this team:");
        if (!string.IsNullOrWhiteSpace(teamName))
        {
            var team = new SavedTeam
            {
                TeamName = teamName,
                Players = _teamBEntries
                    .Select(e => e.Text?.Trim())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
            };
            await TeamService.SaveTeamAsync(team);
            await ReloadTeamPickers();
            TeamBLabel.Text = team.TeamName;
        }
    }

    private async Task ReloadTeamPickers()
    {
        var teams = await TeamService.GetAllTeamsAsync();
        TeamAPicker.ItemsSource = teams.Select(t => t.TeamName).ToList();
        TeamBPicker.ItemsSource = teams.Select(t => t.TeamName).ToList();
    }


    private void GeneratePlayerFields(Layout stack, List<Entry> entryList)
    {
        for (int i = 0; i < 6; i++)
        {
            AddPlayerField(stack, entryList);
        }
    }
    private void PrefillPlayerFields(List<string> existingPlayers, Layout stack, List<Entry> entryList)
    {
        if (existingPlayers != null && existingPlayers.Any())
        {
            foreach (var player in existingPlayers)
            {
                AddPlayerField(stack, entryList, player);
            }
        }
        else
        {
            // Default to 6 empty fields
            for (int i = 0; i < 6; i++)
            {
                AddPlayerField(stack, entryList);
            }
        }
    }

    private void AddPlayerField(Layout stack, List<Entry> entryList, string initialValue = "")
    {
        var entry = new Entry
        {
            Placeholder = $"Player {entryList.Count + 1}",
            FontSize = 18,
            Text = initialValue,
            Margin = new Thickness(0, 0, 10, 0)
        };

        var upButton = new Button
        {
            Text = "⬆",
            FontSize = 18,
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Color.FromArgb("#4CAF50"),
            TextColor = Colors.White,
            CornerRadius = 6,
            Padding = new Thickness(0)
        };

        var downButton = new Button
        {
            Text = "⬇",
            FontSize = 18,
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Color.FromArgb("#2196F3"),
            TextColor = Colors.White,
            CornerRadius = 6,
            Padding = new Thickness(0)
        };

        var removeButton = new Button
        {
            Text = "❌",
            FontSize = 18,
            WidthRequest = 44,
            HeightRequest = 44,
            BackgroundColor = Color.FromArgb("#ffffff"),
            TextColor = Colors.White,
            CornerRadius = 6,
            Padding = new Thickness(0)
        };

        var buttonStack = new HorizontalStackLayout
        {
            Spacing = 5,
            Children = { upButton, downButton, removeButton }
        };

        var rowGrid = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto }
        }
        };

        rowGrid.Add(entry, 0, 0);
        rowGrid.Add(buttonStack, 1, 0);

        // Button actions
        upButton.Clicked += (s, e) =>
        {
            var index = entryList.IndexOf(entry);
            if (index > 0)
            {
                entryList.RemoveAt(index);
                entryList.Insert(index - 1, entry);

                stack.Children.Remove(rowGrid);
                stack.Children.Insert(index - 1, rowGrid);

                UpdateReorderButtonStates(stack, entryList);
            }
        };

        downButton.Clicked += (s, e) =>
        {
            var index = entryList.IndexOf(entry);
            if (index < entryList.Count - 1)
            {
                entryList.RemoveAt(index);
                entryList.Insert(index + 1, entry);

                stack.Children.Remove(rowGrid);
                stack.Children.Insert(index + 1, rowGrid);

                UpdateReorderButtonStates(stack, entryList);
            }
        };

        removeButton.Clicked += (s, e) =>
        {
            entryList.Remove(entry);
            stack.Children.Remove(rowGrid);
            UpdateReorderButtonStates(stack, entryList);
        };

        entryList.Add(entry);
        stack.Children.Add(rowGrid);

        UpdateReorderButtonStates(stack, entryList);
    }


    private void UpdateReorderButtonStates(Layout stack, List<Entry> entryList)
    {
        for (int i = 0; i < entryList.Count; i++)
        {
            var entry = entryList[i];
            var container = stack.Children[i] as Grid;
            if (container == null || container.ColumnDefinitions.Count < 2) continue;

            var buttonStack = container.Children[1] as HorizontalStackLayout;
            if (buttonStack == null || buttonStack.Children.Count < 3) continue;

            var upButton = buttonStack.Children[0] as Button;
            var downButton = buttonStack.Children[1] as Button;

            bool isFirst = i == 0;
            bool isLast = i == entryList.Count - 1;

            if (upButton != null)
            {
                upButton.IsEnabled = !isFirst;
                upButton.BackgroundColor = isFirst ? Colors.LightGray : Color.FromArgb("#4CAF50");
            }

            if (downButton != null)
            {
                downButton.IsEnabled = !isLast;
                downButton.BackgroundColor = isLast ? Colors.LightGray : Color.FromArgb("#2196F3");
            }
        }
    }

    private void OnAddTeamAPlayerClicked(object sender, EventArgs e)
    {
        AddPlayerField(TeamAPlayersStack, _teamAEntries);
    }

    private void OnAddTeamBPlayerClicked(object sender, EventArgs e)
    {
        AddPlayerField(TeamBPlayersStack, _teamBEntries);
    }

    private async void OnStartMatchTeamAFirstClicked(object sender, EventArgs e)
    {
        await StartMatchAsync(battingFirst: "A");
    }

    private async void OnStartMatchTeamBFirstClicked(object sender, EventArgs e)
    {
        await StartMatchAsync(battingFirst: "B");
    }

    private async Task StartMatchAsync(string battingFirst)
    {
        _match.TeamAPlayers = _teamAEntries
            .Select(e => e.Text?.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        _match.TeamBPlayers = _teamBEntries
            .Select(e => e.Text?.Trim())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        _match.TeamARoster = _match.TeamAPlayers.ToList();
        _match.TeamBRoster = _match.TeamBPlayers.ToList();

        // Generate Team A pairs
        _match.TeamAPairOverrides = new List<PairOverride>();
        for (int i = 0; i < _match.TeamARoster.Count; i += 2)
        {
            var b1 = _match.TeamARoster[i];
            var b2 = (i + 1 < _match.TeamARoster.Count) ? _match.TeamARoster[i + 1] : "No Partner";
            _match.TeamAPairOverrides.Add(new PairOverride { Batter1 = b1, Batter2 = b2 });
        }

        // Generate Team B pairs
        _match.TeamBPairOverrides = new List<PairOverride>();
        for (int i = 0; i < _match.TeamBRoster.Count; i += 2)
        {
            var b1 = _match.TeamBRoster[i];
            var b2 = (i + 1 < _match.TeamBRoster.Count) ? _match.TeamBRoster[i + 1] : "No Partner";
            _match.TeamBPairOverrides.Add(new PairOverride { Batter1 = b1, Batter2 = b2 });
        }

        // Save players for autocomplete
        foreach (var name in _match.TeamAPlayers.Concat(_match.TeamBPlayers))
        {
            await PlayerNameService.AddAsync(name);
        }

        // Set batting order
        _match.BattingFirst = battingFirst == "A" ? _match.TeamA : _match.TeamB;
        if (battingFirst == "B")
        {
            // Swap teams if B is batting first
            var tempTeam = _match.TeamA;
            var tempPlayers = _match.TeamAPlayers;
            var tempRoster = _match.TeamARoster;
            var tempPairOverrides = _match.TeamAPairOverrides;
            
            _match.TeamA = _match.TeamB;
            _match.TeamAPlayers = _match.TeamBPlayers;
            _match.TeamARoster = _match.TeamBRoster;
            _match.TeamAPairOverrides = _match.TeamBPairOverrides;

            _match.TeamB = tempTeam;
            _match.TeamBPlayers = tempPlayers;
            _match.TeamBRoster = tempRoster;
            _match.TeamBPairOverrides = tempPairOverrides;
          
        }

        await Navigation.PushAsync(new ScoringPage(_match));
    }
}
