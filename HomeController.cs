using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly JsonProcessor _jsonProcessor = new();

    // Dictionary to map team IDs to image file names
    private readonly Dictionary<int, string> _teamImages = new Dictionary<int, string>
    {
        {26, "49ers.png"},
        {6, "bears.png"},
        {31, "bengals.png"},
        {4, "bills.png"},
        {9, "broncos.png"},
        {7, "browns.png"},
        {28, "buccaneers.png"},
        {1, "cardinals.png"},
        {24, "chargers.png"},
        {15, "chiefs.png"},
        {13, "colts.png"},
        {8, "cowboys.png"},
        {16, "dolphins.png"},
        {22, "eagles.png"},
        {2, "falcons.png"},
        {12, "giants.png"},
        {14, "jaguars.png"},
        {20, "jets.png"},
        {10, "lions.png"},
        {11, "packers.png"},
        {5, "panthers.png"},
        {18, "patriots.png"},
        {21, "raiders.png"},
        {27, "rams.jpg"},
        {3, "ravens.png"},
        {19, "saints.png"},
        {25, "seahawks.png"},
        {23, "steelers.png"},
        {32, "texans.png"},
        {29, "titans.png"},
        {17, "vikings.png"},
        {30, "commanders.png"}
    };

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> FetchTeamData(int teamId)
    {
        var (teamName, statsList, teamRecord) = await _jsonProcessor.GetTeamDataAsync(teamId);

        if (statsList == null || statsList.Count == 0)
        {
            ViewBag.ErrorMessage = "No data found for the selected team.";
            return View("Index");
        }

        // Set team name, stats, and team record to ViewBag
        ViewBag.TeamName = teamName;
        ViewBag.TeamRecord = teamRecord;

        // Set the team image or use a default image
        ViewBag.TeamImage = _teamImages.ContainsKey(teamId) ? _teamImages[teamId] : "default.png";

        return View("Index", statsList);
    }
}
