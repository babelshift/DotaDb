using DotaDb.Models;
using DotaDb.ViewModels;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class HomeController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            HomeViewModel viewModel = new HomeViewModel();

            await ScrapeSteamChart(viewModel);

            viewModel.HeroCount = db.GetHeroes().Count;
            viewModel.HeroAbilityCount = db.GetHeroAbilities().Count;
            viewModel.ShopItemCount = db.GetSchema().Items.Count;
            viewModel.LeagueCount = db.GetLeagues().Count;
            viewModel.InGameItemCount = db.GetGameItems().Count;
            viewModel.LiveLeagueGameCount = (await db.GetLiveLeagueGamesAsync()).Count;

            return View(viewModel);
        }

        private static async Task ScrapeSteamChart(HomeViewModel viewModel)
        {
            HttpClient client = new HttpClient();
            var steamChartsHtml = await client.GetStringAsync("http://steamcharts.com/app/570");
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(steamChartsHtml);
            var appStats = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.GetAttributeValue("class", null) == "app-stat");

            for (int i = 0; i < appStats.Count(); i++)
            {
                var num = appStats.ElementAt(i)
                    .Descendants("span")
                    .First(x => x.GetAttributeValue("class", null) == "num");
                int value = 0;
                bool success = int.TryParse(num.InnerText, out value);

                if (i == 0)
                {
                    viewModel.InGamePlayerCount = value;
                }
                else if (i == 1)
                {
                    viewModel.DailyPeakPlayerCount = value;
                }
                else if (i == 2)
                {
                    viewModel.AllTimePeakPlayerCount = value;
                }
            }
        }
    }
}