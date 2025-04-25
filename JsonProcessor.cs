using System;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

public class JsonProcessor
{
    private static readonly HttpClient client = new();

    private static readonly Dictionary<int, string> teamNames = new()
    {
        { 1, "Cardinals" }, { 2, "Falcons" }, { 3, "Ravens" }, { 4, "Bills" }, { 5, "Panthers" }, { 6, "Bears" },
        { 7, "Browns" }, { 8, "Cowboys" }, { 9, "Broncos" }, { 10, "Lions" }, { 11, "Packers" }, { 12, "Giants" },
        { 13, "Colts" }, { 14, "Jaguars" }, { 15, "Chiefs" }, { 16, "Dolphins" }, { 17, "Vikings" }, { 18, "Patriots" },
        { 19, "Saints" }, { 20, "Jets" }, { 21, "Raiders" }, { 22, "Eagles" }, { 23, "Steelers" }, { 24, "Chargers" },
        { 25, "Seahawks" }, { 26, "49ers" }, { 27, "Rams" }, { 28, "Buccaneers" }, { 29, "Titans" }, { 30, "Washington" },
        { 31, "Bengals" }, { 32, "Texans" }
    };

    // Method to fetch and process team data based on the team ID
    public async Task<(string teamName, List<TeamGameStats> stats, string teamRecord)> GetTeamDataAsync(int teamId)
    {
        if (!teamNames.TryGetValue(teamId, out string teamName))
        {
            return (null, null, null);
        }

        string url = $"https://sports.snoozle.net/search/nfl/searchHandler?fileType=inline&statType=teamStats&season=2020&teamName={teamId}";

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonData = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Raw JSON Response: " + jsonData); // Debugging line

            using JsonDocument doc = JsonDocument.Parse(jsonData);
            var root = doc.RootElement;

            if (!root.TryGetProperty("matchUpStats", out JsonElement matchUpStats) || matchUpStats.ValueKind != JsonValueKind.Array)
            {
                Console.WriteLine($"No data found for team {teamId}");
                return (teamName, null, null);
            }

            List<TeamGameStats> gameStatsList = new();
            int wins = 0, losses = 0;

            foreach (JsonElement game in matchUpStats.EnumerateArray())
            {
                var homeStats = game.GetProperty("homeStats");
                var visStats = game.GetProperty("visStats");

                int homeTeamId = homeStats.GetProperty("teamCode").GetInt32();
                int visTeamId = visStats.GetProperty("teamCode").GetInt32();

                bool isHomeTeam = homeTeamId == teamId;
                JsonElement teamStats = isHomeTeam ? homeStats : visStats;

                var stats = new TeamGameStats
                {
                    Date = game.GetProperty("date").GetString() ?? "Unknown",
                    Opponent = teamNames.ContainsKey(isHomeTeam ? visTeamId : homeTeamId)
                        ? teamNames[isHomeTeam ? visTeamId : homeTeamId]
                        : "Unknown",
                    Score = $"{homeStats.GetProperty("score").GetInt32()} - {visStats.GetProperty("score").GetInt32()}",
                    PassYards = teamStats.TryGetProperty("passYds", out var passYds) ? passYds.GetInt32() : 0,
                    PassComp = teamStats.TryGetProperty("passComp", out var passComp) ? passComp.GetInt32() : 0,
                    PassAtt = teamStats.TryGetProperty("passAtt", out var passAtt) ? passAtt.GetInt32() : 0,
                    RushYards = teamStats.TryGetProperty("rushYds", out var rushYds) ? rushYds.GetInt32() : 0,
                    RushAtt = teamStats.TryGetProperty("rushAtt", out var rushAtt) ? rushAtt.GetInt32() : 0,
                    FirstDowns = teamStats.TryGetProperty("firstDowns", out var firstDowns) ? firstDowns.GetInt32() : 0,
                    TimePossession = teamStats.TryGetProperty("timePoss", out var timePoss) ? timePoss.GetInt32() / 60 : 0,
                    Penalties = teamStats.TryGetProperty("penalties", out var penalties) ? penalties.GetInt32() : 0,
                    penaltYds = teamStats.TryGetProperty("penaltYds", out var penaltYds) ? penaltYds.GetInt32() : 0,
                    FumblesLost = teamStats.TryGetProperty("fumblesLost", out var fumblesLost) ? fumblesLost.GetInt32() : 0,
                    InterceptionsThrown = teamStats.TryGetProperty("interceptionsThrown", out var interceptionsThrown) ? interceptionsThrown.GetInt32() : 0,
                    thridDownAtt = teamStats.TryGetProperty("thridDownAtt", out JsonElement thridDownAtt) ? thridDownAtt.GetInt32() : 0,
                    ThirdDownConver = teamStats.TryGetProperty("thirdDownConver", out var thirdDownConver) ? thirdDownConver.GetInt32() : 0,
                    FourthDownAtt = teamStats.TryGetProperty("fourthDownAtt", out var fourthDownAtt) ? fourthDownAtt.GetInt32() : 0,
                    FourthDownConver = teamStats.TryGetProperty("fourthDownConver", out var fourthDownConver) ? fourthDownConver.GetInt32() : 0,
                };
                gameStatsList.Add(stats);

                int homeScore = homeStats.GetProperty("score").GetInt32();
                int visitorScore = visStats.GetProperty("score").GetInt32();

                if ((isHomeTeam && homeScore > visitorScore) || (!isHomeTeam && visitorScore > homeScore))
                {
                    wins++;
                    stats.Result = "W"; // Mark as Win
                }
                else
                {
                    losses++;
                    stats.Result = "L"; // Mark as Loss
                }
            }

            string teamRecord = $"{wins} - {losses}";
            return (teamName, gameStatsList, teamRecord);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching JSON for team {teamId}: {ex.Message}");
            return (teamName, null, null);
        }
    }

    // Model to structure team statistics
    public class TeamGameStats
    {
        internal int thridDownAtt;

        public string Date { get; set; }
        public string Opponent { get; set; }
        public string Score { get; set; }
        public int PassYards { get; set; }
        public int PassComp { get; set; }
        public int PassAtt { get; set; }
        public int RushYards { get; set; }
        public int RushAtt { get; set; }
        public int FirstDowns { get; set; }
        public int TimePossession { get; set; }  // Time in minutes
        public int Penalties { get; set; }
        public int penaltYds { get; set; }
        public int FumblesLost { get; set; }
        public int InterceptionsThrown { get; set; }
        public int ThirdDownAtt { get; set; }
        public int ThirdDownConver { get; set; }
        public int FourthDownAtt { get; set; }
        public int FourthDownConver { get; set; }

        // Add Result property to indicate Win, Loss, or Tie
        public string Result { get; set; }
    }

    // Main method to handle multiple team data fetches concurrently
    public async Task ProcessMultipleTeamsAsync()
    {
        List<int> teamIds = new() { 1, 2, 3, 4, 5 }; // Example team IDs to process

        List<Task<(string, List<TeamGameStats>, string)>> tasks = new();

        foreach (int teamId in teamIds)
        {
            tasks.Add(GetTeamDataAsync(teamId));
        }

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            Console.WriteLine($"Team: {result.Item1}, Record: {result.Item3}");
        }
    }
}

