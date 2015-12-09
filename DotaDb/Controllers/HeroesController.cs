using DotaDb.Models;
using DotaDb.ViewModels;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public class HeroesController : Controller
    {
        private InMemoryDb db = InMemoryDb.Instance;

        #region Hero Index

        public ActionResult Index()
        {
            var heroes = db.GetHeroes();

            var str = heroes.Where(x =>
                x.Value.AttributePrimary == DotaHeroPrimaryAttributeType.STRENGTH.Key
                && x.Value.Name != "npc_dota_hero_base"
                && x.Value.Enabled);

            var agi = heroes.Where(x =>
                x.Value.AttributePrimary == DotaHeroPrimaryAttributeType.AGILITY.Key
                && x.Value.Name != "npc_dota_hero_base"
                && x.Value.Enabled);

            var intel = heroes.Where(x =>
                x.Value.AttributePrimary == DotaHeroPrimaryAttributeType.INTELLECT.Key
                && x.Value.Name != "npc_dota_hero_base"
                && x.Value.Enabled);

            List<HeroSelectItemViewModel> strHeroes = new List<HeroSelectItemViewModel>();
            foreach (var strHero in str)
            {
                strHeroes.Add(new HeroSelectItemViewModel()
                {
                    Url = strHero.Value.Url,
                    Name = strHero.Value.Name,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", strHero.Value.Name.Replace("npc_dota_hero_", ""))
                });
            }

            List<HeroSelectItemViewModel> agiHeroes = new List<HeroSelectItemViewModel>();
            foreach (var agiHero in agi)
            {
                agiHeroes.Add(new HeroSelectItemViewModel()
                {
                    Url = agiHero.Value.Url,
                    Name = agiHero.Value.Name,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", agiHero.Value.Name.Replace("npc_dota_hero_", ""))
                });
            }

            List<HeroSelectItemViewModel> intHeroes = new List<HeroSelectItemViewModel>();
            foreach (var intHero in intel)
            {
                intHeroes.Add(new HeroSelectItemViewModel()
                {
                    Url = intHero.Value.Url,
                    Name = intHero.Value.Name,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", intHero.Value.Name.Replace("npc_dota_hero_", ""))
                });
            }

            HeroSelectViewModel viewModel = new HeroSelectViewModel();
            viewModel.StrengthHeroes = strHeroes.AsReadOnly();
            viewModel.AgilityHeroes = agiHeroes.AsReadOnly();
            viewModel.IntelligenceHeroes = intHeroes.AsReadOnly();

            return View(viewModel);
        }

        #endregion Hero Index

        #region Hero Specifics

        public ActionResult Build(int id)
        {
            HeroItemBuildViewModel viewModel = new HeroItemBuildViewModel();

            var hero = db.GetHeroKeyValue(id);
            SetupHeroViewModel(hero, viewModel);

            viewModel.ActiveTab = "ItemBuilds";

            try
            {
                string vdfPath = Path.Combine(db.AppDataPath, String.Format("default_{0}.txt", hero.Name.Replace("npc_dota_hero_", "")));
                string vdf = System.IO.File.ReadAllText(vdfPath);
                var itemBuild = SourceSchemaParser.SchemaFactory.GetDotaItemBuild(vdf);

                viewModel.Title = itemBuild.Title;
                viewModel.Author = itemBuild.Author;
                viewModel.ItemGroups = GetItemGroups(itemBuild);
            }
            catch (FileNotFoundException)
            {
                ViewBag.ErrorMessage = "This hero doesn't have any item builds in the Dota 2 files yet.";
            }

            return PartialView("_ItemBuildsPartial", viewModel);
        }

        private List<HeroItemBuildGroupViewModel> GetItemGroups(DotaItemBuildSchemaItem itemBuild)
        {
            List<HeroItemBuildGroupViewModel> itemGroupViewModels = new List<HeroItemBuildGroupViewModel>();
            foreach (var itemGroup in itemBuild.Items)
            {
                HeroItemBuildGroupViewModel itemGroupViewModel = new HeroItemBuildGroupViewModel();
                itemGroupViewModel.Title = db.GetLocalizationText(itemGroup.Name.Remove(0, 1));

                List<HeroItemBuildItemViewModel> itemViewModels = new List<HeroItemBuildItemViewModel>();

                itemGroupViewModel.Items = itemGroup.Items
                    .Select(x => new HeroItemBuildItemViewModel()
                    {
                        IconPath = String.Format("http://cdn.dota2.com/apps/dota2/images/items/{0}_lg.png", x.Replace("item_", "")),
                        Name = db.GetLocalizationText(String.Format("DOTA_Tooltip_Ability_{0}", x))
                    })
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

        public ActionResult Hero(int id)
        {
            HeroViewModel viewModel = new HeroViewModel();

            var hero = db.GetHeroKeyValue(id);
            SetupHeroViewModel(hero, viewModel);
            SetupAbilities(hero, viewModel);

            viewModel.ActiveTab = "Overview";

            return PartialView("_HeroOverviewPartial", viewModel);
        }

        private BaseHeroViewModel SetupHeroViewModel<T>(DotaHeroSchemaItem hero, T viewModel)
            where T : BaseHeroViewModel
        {
            viewModel.Name = db.GetLocalizationText(hero.Name);
            viewModel.Description = "<from localization -> npc_dota_hero_<heroname>_hype>";
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
            viewModel.Roles = GetRoles(hero.Role, hero.RoleLevels).AsReadOnly();
            viewModel.AgilityGain = hero.AttributeAgilityGain;
            viewModel.IntelligenceGain = hero.AttributeIntelligenceGain;
            viewModel.StrengthGain = hero.AttributeStrengthGain;
            viewModel.PrimaryAttribute = db.GetHeroPrimaryAttributeTypeKeyValue(hero.AttributePrimary);
            return viewModel;
        }
        
        private void SetupAbilities(DotaHeroSchemaItem hero, HeroViewModel viewModel)
        {
            var abilities = db.GetHeroAbilities();

            List<HeroAbilityViewModel> abilityViewModels = new List<HeroAbilityViewModel>();

            AddAbilityToViewModel(hero.Ability1, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability2, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability3, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability4, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability5, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability6, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability7, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability8, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability9, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability10, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability11, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability12, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability13, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability14, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability15, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability16, abilities, abilityViewModels);

            viewModel.Abilities = abilityViewModels.AsReadOnly();
        }

        private void AddAbilityToViewModel(string abilityName, IReadOnlyCollection<DotaAbilitySchemaItem> abilities, List<HeroAbilityViewModel> abilityViewModels)
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
                    Name = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString()
                });
            }

            var abilityViewModel = new HeroAbilityViewModel()
            {
                Name = db.GetLocalizationText(String.Format("{0}_{1}", "DOTA_Tooltip_ability", abilityName)),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                Description = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Description")),
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
                Note0 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note0")),
                Note1 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note1")),
                Note2 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note2")),
                Note3 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note3")),
                Note4 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note4")),
                Note5 = db.GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Note5")),
            };

            abilityViewModels.Add(abilityViewModel);
        }

        private static List<HeroRoleViewModel> GetRoles(string roles, string roleLevels)
        {
            string[] rolesSplit = roles.Split(',');
            string[] roleLevelsSplit = roleLevels.Split(',');

            List<HeroRoleViewModel> roleViewModels = new List<HeroRoleViewModel>();
            for (int i = 0; i < rolesSplit.Length; i++)
            {
                roleViewModels.Add(new HeroRoleViewModel()
                {
                    Name = rolesSplit[i],
                    Level = roleLevelsSplit[i]
                });
            }

            return roleViewModels;
        }

        #endregion Hero Specifics
    }
}