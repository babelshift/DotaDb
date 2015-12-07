using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace DotaDb.Controllers
{
    public class LeaguesController : Controller
    {
        private InMemoryDb db = new InMemoryDb();

        public ActionResult Index(int? page)
        {
            var leagues = db.GetLeagues();

            List<LeagueViewModel> viewModel = new List<LeagueViewModel>();
            
            foreach (var league in leagues)
            {
                string itemIconFileName = String.Empty;

                if (String.IsNullOrEmpty(league.ImageBannerPath) || league.ImageBannerPath.EndsWith("ingame"))
                {
                    itemIconFileName = league.ImageInventoryPath.Replace("econ/leagues/", "") + ".png";
                }
                else
                {
                    itemIconFileName = league.ImageBannerPath.Replace("econ/leagues/", "") + ".png";
                }

                LeagueViewModel leagueViewModel = new LeagueViewModel()
                {

                    Name = league.NameLocalized,
                    Description = league.DescriptionLocalized,
                    Location = league.Location,
                    Tier = league.Tier,
                    Url = league.TournamentUrl,
                    LogoFileName = itemIconFileName
                };

                viewModel.Add(leagueViewModel);
            }

            return View(viewModel.ToPagedList(page ?? 1, 25));
        }
    }
}