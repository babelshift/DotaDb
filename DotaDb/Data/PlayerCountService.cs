using DotaDb.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class PlayerCountService
    {
        public async Task<PlayerCountModel> GetPlayerCountsFromScrapingAsync()
        {
            HttpClient client = new HttpClient();
            var steamChartsHtml = await client.GetStringAsync("http://steamcharts.com/app/570");
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(steamChartsHtml);
            var appStats = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.GetAttributeValue("class", null) == "app-stat");

            PlayerCountModel model = new PlayerCountModel();

            for (int i = 0; i < appStats.Count(); i++)
            {
                var num = appStats.ElementAt(i)
                    .Descendants("span")
                    .First(x => x.GetAttributeValue("class", null) == "num");
                int value = 0;
                bool success = int.TryParse(num.InnerText, out value);

                if (i == 0)
                {
                    model.InGamePlayerCount = value;
                }
                else if (i == 1)
                {
                    model.DailyPeakPlayerCount = value;
                }
                else if (i == 2)
                {
                    model.AllTimePeakPlayerCount = value;
                }
            }

            return model;
        }
    }
}
