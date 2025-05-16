using System.Text.Json;
using CommunityToolkit.Maui.Alerts;
using System.Diagnostics;
using CricketScorer.Core.Models;
public static class MatchSaver
{
    public static async Task SaveMatchResultAsync(MatchResult result)
    {
        string fileName = $"match {result.TeamA} vs {result.TeamB} {DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(filePath, json);
        // Optional: Clean up old saves to avoid clutter
        var oldFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "INPROGRESS_*.json")
                                .OrderByDescending(File.GetLastWriteTime);


        foreach (var oldFile in oldFiles)
        {
            File.Delete(oldFile);
        }

        // Optional: Display toast or alert
        await Toast.Make("Match Saved").Show();
    }
    public static async Task SaveMatchState(Match currentMatch)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var teams = $"{currentMatch.TeamA} vs {currentMatch.TeamB}".Replace(" ", "_");
            var fileName = $"INPROGRESS_{teams}_{timestamp}.json";

            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            var json = JsonSerializer.Serialize(currentMatch);
            File.WriteAllText(filePath, json);


            // Optional: Clean up old saves to avoid clutter
            var oldFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "INPROGRESS_*.json")
                                    .OrderByDescending(File.GetLastWriteTime)
                                    .Skip(3); // keep latest 3

            foreach (var oldFile in oldFiles)
            {
                File.Delete(oldFile);
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Crash Recovery] Save failed: {ex.Message}");
        }
    }

    public static Match TryLoadSavedMatch(Match match)
    {
        try
        {
            var folder = FileSystem.AppDataDirectory;
            var files = Directory.GetFiles(folder, "INPROGRESS_*.json");

            if (files.Length == 0)
                return null;

            // Sort by last modified time descending and pick the latest
            var latestFile = files
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .First()
                .FullName;

            var json = File.ReadAllText(latestFile);
            return JsonSerializer.Deserialize<Match>(json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Crash Recovery] Load failed: {ex.Message}");
            return null;
        }
    }

    // Deletes old INPROGRESS match files
    public static void PurgeMatchFiles()
    {
        try
        {
            var folder = FileSystem.AppDataDirectory;
            var files = Directory.GetFiles(folder, "INPROGRESS_*.json");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Crash Recovery] Delete failed: {ex.Message}");
        }
    }

}

