using CricketScorer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CricketScorer.Helpers
{
    public static class TeamService
    {
        private static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "teams.json");

        public static async Task<List<SavedTeam>> GetAllTeamsAsync()
        {
            if (!File.Exists(FilePath)) return new List<SavedTeam>();
            var json = await File.ReadAllTextAsync(FilePath);
            return JsonSerializer.Deserialize<List<SavedTeam>>(json) ?? new();
        }

        public static async Task SaveTeamAsync(SavedTeam team)
        {
            var teams = await GetAllTeamsAsync();
            teams.RemoveAll(t => t.TeamName == team.TeamName); // Overwrite existing
            teams.Add(team);
            var json = JsonSerializer.Serialize(teams);
            await File.WriteAllTextAsync(FilePath, json);
        }

        public static async Task<SavedTeam?> GetTeamByNameAsync(string teamName)
        {
            var teams = await GetAllTeamsAsync();
            return teams.FirstOrDefault(t => t.TeamName == teamName);
        }
    }

}
