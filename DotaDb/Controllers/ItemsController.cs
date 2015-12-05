using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Newtonsoft.Json;
using DotaDb.Models;
using DotaDb.ViewModels;

namespace DotaDb.Controllers
{
    public class ItemsController : Controller
    {
        private string AppDataPath { get { return AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); } }

        public ActionResult Index()
        {
            // file would come from SteamWebAPI2 call
            string itemsJsonPath = Path.Combine(AppDataPath, "game_items.json");
            string itemsJson = System.IO.File.ReadAllText(itemsJsonPath);
            JObject parsedItems = JObject.Parse(itemsJson);
            var itemsArray = parsedItems["result"]["items"];

            var items = itemsArray.ToObject<List<GameItem>>();
            var itemsViewModel = items.Select(x => new GameItemViewModel()
            {
                Cost = x.Cost,
                Name = x.Name,
                Id = x.Id,
                IsRecipe = x.IsRecipe,
                SecretShop = x.SecretShop,
                SideShop = x.SideShop,
                IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", x.IsRecipe ? "recipe" : x.Name.Replace("item_", "")),
            })
            .OrderBy(x => x.IsRecipe);

            return View(itemsViewModel.ToList());
        }
    }
}