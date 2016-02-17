using DotaDb.Data;
using DotaDb.ViewModels;
using Steam.Models.DOTA2;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class FilesController : BaseController
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            FilesIndexViewModel viewModel = new FilesIndexViewModel();

            viewModel.MainSchemaLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.MainSchema);
            viewModel.HeroAbilitiesLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.HeroAbilities);
            viewModel.HeroesLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.Heroes);
            viewModel.InGameItemsLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.InGameItems);
            viewModel.ItemAbilitiesLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.ItemAbilities);
            viewModel.PanoramaLocalizationLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.PanoramaLocalization);
            viewModel.PublicLocalizationLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.PublicLocalization);
            viewModel.MainSchemaLocalizationLastUpdated = await db.GetGameFileLastModifiedDateAsync(GameFile.MainSchemaLocalization);

            return View(viewModel);
        }
    }
}