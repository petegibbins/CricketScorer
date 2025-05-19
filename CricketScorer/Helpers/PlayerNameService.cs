using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CricketScorer.Helpers
{
        public static class PlayerNameService
        {
            private const string FileName = "saved_players.json";

            public static async Task<List<string>> LoadAsync()
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                if (!File.Exists(path)) return new List<string>();

                var json = await File.ReadAllTextAsync(path);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }

            public static async Task AddAsync(string name)
            {
                var names = await LoadAsync();
                if (!names.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    names.Add(name);
                    await SaveAsync(names);
                }
            }

            private static async Task SaveAsync(List<string> names)
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, FileName);
                var json = JsonSerializer.Serialize(names.OrderBy(n => n));
                await File.WriteAllTextAsync(path, json);
            }
        }

}
