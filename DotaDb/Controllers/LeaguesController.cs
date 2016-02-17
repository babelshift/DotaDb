using DotaDb.Data;
using DotaDb.Utilities;
using DotaDb.ViewModels;
using PagedList;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class LeaguesController : BaseController
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