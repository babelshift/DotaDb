using DotaDb.Models;
using DotaDb.ViewModels;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class FilesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        public async Task<ActionResult> Index()
        {
            FilesIndexViewModel viewModel = new FilesIndexViewModel();

            viewModel.MainSchemaLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.MainSchema);
            viewModel.HeroAbilitiesLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.HeroAbilities);
            viewModel.HeroesLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.Heroes);
            viewModel.InGameItemsLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.InGameItems);
            viewModel.ItemAbilitiesLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.ItemAbilities);
            viewModel.PanoramaLocalizationLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.PanoramaLocalization);
            viewModel.PublicLocalizationLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.PublicLocalization);
            viewModel.MainSchemaLocalizationLastUpdated = await db.GetSourceFileLastModifiedDateAsync(SourceFile.MainSchemaLocalization);
            
            return View(viewModel);
        }
    }
}