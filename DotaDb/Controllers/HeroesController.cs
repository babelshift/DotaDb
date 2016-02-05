using DotaDb.Models;
using DotaDb.ViewModels;
using EasyAzureStorage;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DotaDb.Utilities;

namespace DotaDb.Controllers
{
    public class HeroesController : BaseController
    {
        private InMemoryDb db = InMemoryDb.Instance;

        #region Hero Index

        public async Task<ActionResult> Index(string tab = "")
        {
            var heroes = await db.GetHeroesAsync();
            var str = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.STRENGTH.Key);
            var agi = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.AGILITY.Key);
            var intel = GetHeroesByPrimaryAttribute(heroes, DotaHeroPrimaryAttributeType.INTELLECT.Key);

            HeroSelectViewModel viewModel = new HeroSelectViewModel();

            viewModel.StrengthHeroes = await TranslateToViewModelAsync(str);
            viewModel.AgilityHeroes = await TranslateToViewModelAsync(agi);
            viewModel.IntelligenceHeroes = await TranslateToViewModelAsync(intel);

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

        private async Task<IReadOnlyCollection<HeroViewModel>> TranslateToViewModelAsync(IEnumerable<KeyValuePair<int, DotaHeroSchemaItem>> heroes)
        {
            List<HeroViewModel> heroViewModels = new List<HeroViewModel>();

            foreach (var hero in heroes)
            {
                HeroViewModel viewModel = new HeroViewModel();
                await SetupHeroViewModelAsync(hero.Value, viewModel);
                heroViewModels.Add(viewModel);
            }

            return heroViewModels;
        }

        private static IEnumerable<KeyValuePair<int, DotaHeroSchemaItem>> GetHeroesByPrimaryAttribute(IReadOnlyDictionary<int, DotaHeroSchemaItem> heroes, string attributeKey)
        {
            return heroes.Where(x =>
                x.Value.AttributePrimary == attributeKey
                && x.Value.Name != "npc_dota_hero_base"
                && x.Value.Enabled);
        }

        #endregion Hero Index

        #region Hero Specifics

        public async Task<ActionResult> Build(int id, string heroName = null)
        {
            HeroItemBuildViewModel viewModel = new HeroItemBuildViewModel();

            var hero = await db.GetHeroKeyValueAsync(id);

            if (heroName != hero.Url.ToLower())
            {
                RedirectToAction("hero", new { id = id, heroName = hero.Url.ToLower() });
            }

            await SetupHeroViewModelAsync(hero, viewModel);

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

        private async Task<List<HeroItemBuildGroupViewModel>> GetItemGroupsAsync(DotaItemBuildSchemaItem itemBuild)
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

        public async Task<ActionResult> Hero(int id, string heroName = null)
        {
            HeroViewModel viewModel = new HeroViewModel();

            var hero = await db.GetHeroKeyValueAsync(id);

            if (heroName != hero.Url.ToLower())
            {
                RedirectToAction("hero", new { id = id, heroName = hero.Url.ToLower() });
            }

            await SetupHeroViewModelAsync(hero, viewModel);
            await SetupAbilitiesAsync(hero, viewModel);

            viewModel.ActiveTab = "Overview";

            return PartialView("_HeroOverviewPartial", viewModel);
        }

        private async Task<BaseHeroViewModel> SetupHeroViewModelAsync<T>(DotaHeroSchemaItem hero, T viewModel)
            where T : BaseHeroViewModel
        {
            viewModel.Id = hero.HeroId;
            viewModel.Url = hero.Url.ToLower();
            viewModel.Name = await db.GetLocalizationTextAsync(hero.Name);
            viewModel.Description = await db.GetPanoramaLocalizationTextAsync(String.Format("npc_dota_hero_{0}_hype", hero.Url.ToLower()));
            viewModel.AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_full.png", hero.Name.Replace("npc_dota_hero_", ""));
            viewModel.BaseAgility = hero.AttributeBaseAgility;
            viewModel.BaseArmor = hero.ArmorPhysical;
            viewModel.BaseDamageMax = hero.AttackDamageMax;
            viewModel.BaseDamageMin = hero.AttackDamageMin;
            viewModel.BaseMoveSpeed = hero.MovementSpeed;
            viewModel.BaseStrength = hero.AttributeBaseStrength;
            viewModel.BaseIntelligence = hero.AttributeBaseIntelligence;
            viewModel.AttackRate = hero.AttackRate;
            viewModel.AttackRange = hero.AttackRange;
            viewModel.Team = db.GetTeamTypeKeyValue(hero.Team).ToString();
            viewModel.TurnRate = hero.MovementTurnRate;
            viewModel.AttackType = db.GetAttackTypeKeyValue(hero.AttackCapabilities).ToString();
            viewModel.Roles = hero.GetRoles();
            viewModel.AgilityGain = hero.AttributeAgilityGain;
            viewModel.IntelligenceGain = hero.AttributeIntelligenceGain;
            viewModel.StrengthGain = hero.AttributeStrengthGain;
            viewModel.PrimaryAttribute = db.GetHeroPrimaryAttributeTypeKeyValue(hero.AttributePrimary);
            viewModel.MinimapIconPath = hero.GetMinimapIconFilePath();

            return viewModel;
        }

        private async Task SetupAbilitiesAsync(DotaHeroSchemaItem hero, HeroViewModel viewModel)
        {
            var abilities = await db.GetHeroAbilitiesAsync();

            List<HeroAbilityViewModel> abilityViewModels = new List<HeroAbilityViewModel>();

            await AddAbilityToViewModelAsync(hero.Ability1, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability2, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability3, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability4, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability5, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability6, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability7, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability8, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability9, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability10, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability11, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability12, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability13, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability14, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability15, abilities, abilityViewModels);
            await AddAbilityToViewModelAsync(hero.Ability16, abilities, abilityViewModels);

            viewModel.Abilities = abilityViewModels.AsReadOnly();
        }

        private async Task AddAbilityToViewModelAsync(string abilityName, IReadOnlyCollection<DotaAbilitySchemaItem> abilities, List<HeroAbilityViewModel> abilityViewModels)
        {
            if (String.IsNullOrEmpty(abilityName))
            {
                return;
            }

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);

            string joinedBehaviors = db.GetJoinedBehaviors(ability.AbilityBehavior);
            string joinedUnitTargetTeamTypes = db.GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam);
            string joinedUnitTargetTypes = db.GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType);
            string joinedUnitTargetFlags = db.GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags);

            List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
            foreach (var abilitySpecial in ability.AbilitySpecials)
            {
                abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                {
                    Name = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString()
                });
            }

            var abilityViewModel = new HeroAbilityViewModel()
            {
                Id = ability.Id,
                Name = await db.GetLocalizationTextAsync(String.Format("{0}_{1}", "DOTA_Tooltip_ability", abilityName)),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                Description = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Description")),
                CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString(),
                CastRange = ability.AbilityCastRange.ToSlashSeparatedString(),
                Cooldown = ability.AbilityCooldown.ToSlashSeparatedString(),
                Damage = ability.AbilityDamage.ToSlashSeparatedString(),
                DamageType = db.GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                SpellImmunityType = db.GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                Attributes = abilitySpecialViewModels,
                Behaviors = joinedBehaviors,
                AbilityType = db.GetHeroAbilityTypeKeyValue(ability.AbilityType),
                TargetFlags = joinedUnitTargetFlags,
                TargetTypes = joinedUnitTargetTypes,
                TeamTargets = joinedUnitTargetTeamTypes,
                Note0 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note0")),
                Note1 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note1")),
                Note2 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note2")),
                Note3 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note3")),
                Note4 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note4")),
                Note5 = await db.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note5")),
            };

            abilityViewModels.Add(abilityViewModel);
        }

        #endregion Hero Specifics
    }
}