
using DotaDb.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class PlayerCountService
    {
        private readonly IConfiguration configuration;
        private readonly CacheService cacheService;

        private readonly string playerCountsUrl;

        public PlayerCountService(
            IConfiguration configuration,
            CacheService cacheService)
        {
            this.configuration = configuration;
            this.cacheService = cacheService;
            playerCountsUrl = configuration["PlayerCountsUrl"];
        }

        public async Task<PlayerCountModel> GetPlayerCountsFromScrapingAsync()
        {
            return await cacheService.GetOrSetAsync(MemoryCacheKey.PlayerCounts, async () =>
            {
                HttpClient client = new HttpClient();
                var steamChartsHtml = await client.GetStringAsync(playerCountsUrl);
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
            }, TimeSpan.FromMinutes(15));
        }
    }
}