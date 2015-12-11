using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using DotaDb.Utilities;
using System.Threading.Tasks;

namespace DotaDb.Controllers
{
    public class LeaguesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index(int? page)
        {
            var leagues = await db.GetLeagueTicketsAsync();

            List<LeagueViewModel> viewModel = new List<LeagueViewModel>();
            
            foreach (var league in leagues.Values)
            {
                LeagueViewModel leagueViewModel = new LeagueViewModel()
                {
                    LogoFilePath = league.GetLogoFilePath(),
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