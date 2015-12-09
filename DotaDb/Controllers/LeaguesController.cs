using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using DotaDb.Utilities;

namespace DotaDb.Controllers
{
    public class LeaguesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public ActionResult Index(int? page)
        {
            var leagues = db.GetLeagueTickets();

            List<LeagueViewModel> viewModel = new List<LeagueViewModel>();
            
            foreach (var league in leagues.Values)
            {
                LeagueViewModel leagueViewModel = new LeagueViewModel()
                {
                    LogoFileName = league.GetLogoFileName(),
                    Name = league.NameLocalized,
                    Description = league.DescriptionLocalized,
                    Location = league.Location,
                    Tier = league.Tier,
                    Url = league.TournamentUrl
                };

                viewModel.Add(leagueViewModel);
            }

            return View(viewModel.ToPagedList(page ?? 1, 25));
        }
    }
}