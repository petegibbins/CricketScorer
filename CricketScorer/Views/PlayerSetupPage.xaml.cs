using CricketScorer.Core.Models;
using CricketScorer.Core.Services;
using CricketScorer.Helpers;
using CricketScorer.Views;
using System.Text.RegularExpressions;

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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TeamALabel.Text = _match.TeamA;
        TeamBLabel.Text = _match.TeamB;
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
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        var upButton = new Button { Text = "⬆", FontSize = 20, WidthRequest = 50 };
        var downButton = new Button { Text = "⬇", FontSize = 20, WidthRequest = 50 };
        var removeButton = new Button { Text = "❌", TextColor = Colors.Red, FontSize = 20, WidthRequest = 50 };

        var buttonRow = new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { upButton, downButton, removeButton },
            HorizontalOptions = LayoutOptions.Center
        };

        var container = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { entry, buttonRow },
            Padding = new Thickness(0, 5)
        };

        // Button actions
        upButton.Clicked += (s, e) =>
        {
            var index = entryList.IndexOf(entry);
            if (index > 0)
            {
                entryList.RemoveAt(index);
                entryList.Insert(index - 1, entry);

                stack.Children.Remove(container);
                stack.Children.Insert(index - 1, container);

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

                stack.Children.Remove(container);
                stack.Children.Insert(index + 1, container);

                UpdateReorderButtonStates(stack, entryList);
            }
        };

        removeButton.Clicked += (s, e) =>
        {
            entryList.Remove(entry);
            stack.Children.Remove(container);
            UpdateReorderButtonStates(stack, entryList);
        };

        entryList.Add(entry);
        stack.Children.Add(container);

        // Ensure correct button state
        UpdateReorderButtonStates(stack, entryList);
    }

    private void UpdateReorderButtonStates(Layout stack, List<Entry> entryList)
    {
        for (int i = 0; i < entryList.Count; i++)
        {
            var entry = entryList[i];
            var container = stack.Children[i] as Layout;
            var buttons = (container as VerticalStackLayout)?.Children[1] as HorizontalStackLayout;

            if (buttons == null || buttons.Children.Count < 3) continue;

            var upButton = buttons.Children[0] as Button;
            var downButton = buttons.Children[1] as Button;

            upButton.IsEnabled = i != 0;
            downButton.IsEnabled = i != entryList.Count - 1;
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

    private async void OnStartMatchClicked(object sender, EventArgs e)
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

        await Navigation.PushAsync(new ScoringPage(_match));
    }
}
