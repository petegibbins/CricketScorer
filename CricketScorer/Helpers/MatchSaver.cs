using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using CricketScorer.Models;
using CommunityToolkit.Maui.Alerts;

public static class MatchSaver
{
    public static async Task SaveMatchResultAsync(MatchResult result)
    {
        string fileName = $"match_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(filePath, json);

        // Optional: Display toast or alert
        await Toast.Make("Match Saved").Show();
    }
}

