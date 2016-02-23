using DotaDb.Data;
using DotaDb.ViewModels;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class HeroesController : BaseController
    {
        private InMemoryDb db = InMemoryDb.Instance;

        #region Hero Index

        [OutputCache(CacheProfile = "Default", VaryByParam = "tab")]
        public async Task<ActionResult> Index(string tab = "")
        {
            var heroes = await db.GetHeroDetailsAsync();

            var str = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.STRENGTH.Key);
            var agi = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.AGILITY.Key);
            var intel = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.INTELLECT.Key);

            HeroSelectViewModel viewModel = new HeroSelectViewModel();

            viewModel.StrengthHeroes = TranslateToViewModel(str);
            viewModel.AgilityHeroes = TranslateToViewModel(agi);
            viewModel.IntelligenceHeroes = TranslateToViewModel(intel);

            if (tab == "grid")
            {
                return View(viewModel);
            }
            else if (tab == "table")
            {
                return View("IndexTable", viewModel);
            }
            else
            {
                return View(viewModel);
            }
        }

        private IReadOnlyCollection<HeroViewModel> TranslateToViewModel(IEnumerable<KeyValuePair<int, HeroDetailModel>> heroes)
        {
            List<HeroViewModel> heroViewModels = new List<HeroViewModel>();

            foreach (var hero in heroes)
            {
                HeroViewModel heroViewModel = AutoMapperConfiguration.Mapper.Map<HeroDetailModel, HeroViewModel>(hero.Value);
                heroViewModels.Add(heroViewModel);
            }

            return heroViewModels;
        }

        private static IEnumerable<KeyValuePair<int, HeroDetailModel>> GetHeroesByPrimaryAttribute(IReadOnlyDictionary<int, HeroDetailModel> heroes, string attributeKey)
        {
            return heroes.Where(x =>
                x.Value.PrimaryAttribute.Key == attributeKey
                && x.Value.NameInSchema != "npc_dota_hero_base"
                && x.Value.IsEnabled);
        }

        #endregion Hero Index

        #region Hero Specifics

        [OutputCache(CacheProfile = "Default", VaryByParam = "id")]
        public async Task<ActionResult> Build(int id, string heroName = null)
        {
            var hero = await db.GetHeroDetailsAsync(id);

            if (heroName != hero.Url.ToLower())
            {
                RedirectToAction("hero", new { id = id, heroName = hero.Url.ToLower() });
            }

            var viewModel = AutoMapperConfiguration.Mapper.Map<HeroDetailModel, HeroItemBuildViewModel>(hero);

            viewModel.ActiveTab = "ItemBuilds";

            try
            {
                var itemBuild = await db.GetItemBuildAsync(hero.Name);
                viewModel.Title = itemBuild.Title;
                viewModel.Author = itemBuild.Author;
                viewModel.ItemGroups = await GetItemGroupsAsync(itemBuild);
            }
            catch (FileNotFoundException)
            {
                ViewBag.ErrorMessage = "This hero doesn't have any item builds in the Dota 2 files yet.";
            }

            return PartialView("_ItemBuildsPartial", viewModel);
        }

        private async Task<List<HeroItemBuildGroupViewModel>> GetItemGroupsAsync(ItemBuildSchemaItemModel itemBuild)
        {
            List<HeroItemBuildGroupViewModel> itemGroupViewModels = new List<HeroItemBuildGroupViewModel>();
            foreach (var itemGroup in itemBuild.Items)
            {
                HeroItemBuildGroupViewModel itemGroupViewModel = new HeroItemBuildGroupViewModel();
                itemGroupViewModel.Title = await db.GetLocalizationTextAsync(itemGroup.Name.Remove(0, 1));

                List<HeroItemBuildItemViewModel> itemViewModels = new List<HeroItemBuildItemViewModel>();

                var tasks = itemGroup.Items.Select(async (x) => new HeroItemBuildItemViewModel()
                {
                    IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", x.Replace("item_", "")),
                    Name = await db.GetLocalizationTextAsync(String.Format("DOTA_Tooltip_Ability_{0}", x))
                });
                var selectedItems = await Task.WhenAll(tasks);

                itemGroupViewModel.Items = selectedItems
                    .GroupBy(x => new { x.Name, x.IconPath })
                    .Select(x => new HeroItemBuildItemViewModel()
                    {
                        IconPath = x.Key.IconPath,
                        Name = x.Key.Name,
                        Quantity = x.Count()
                    })
                    .ToList();

                itemGroupViewModels.Add(itemGroupViewModel);
            }

            return itemGroupViewModels;
        }

        [OutputCache(CacheProfile = "Default", VaryByParam = "id")]
        public async Task<ActionResult> Hero(int id, string heroName = null)
        {
            var hero = await db.GetHeroDetailsAsync(id);

            if (heroName != hero.Url.ToLower())
            {
                RedirectToAction("hero", new { id = id, heroName = hero.Url.ToLower() });
            }

            var viewModel = AutoMapperConfiguration.Mapper.Map<HeroDetailModel, HeroViewModel>(hero);

            viewModel.ActiveTab = "Overview";

            return PartialView("_HeroOverviewPartial", viewModel);
        }

        #endregion Hero Specifics
    }
}