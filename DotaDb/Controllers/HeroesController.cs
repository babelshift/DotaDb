using DotaDb.ViewModels;
using SourceSchemaParser;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
    public enum DotaHeroAbilityType
    {
        DOTA_ABILITY_TYPE_BASIC,
        DOTA_ABILITY_TYPE_ULTIMATE,
        DOTA_ABILITY_TYPE_ATTRIBUTES
    }

    public enum DotaHeroPrimaryAttribute
    {
        DOTA_ATTRIBUTE_STRENGTH,
        DOTA_ATTRIBUTE_AGILITY,
        DOTA_ATTRIBUTE_INTELLECT
    }

    public enum DotaAttackType
    {
        DOTA_UNIT_CAP_RANGED_ATTACK,
        DOTA_UNIT_CAP_MELEE_ATTACK
    }

    public enum DotaTeam
    {
        DOTA_TEAM_GOODGUYS,
        DOTA_TEAM_BADGUYS
    }

    public class HeroesController : Controller
    {
        private string AppDataPath { get { return AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); } }

        private IReadOnlyDictionary<string, string> localizationKeys;
        private IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType> abilityBehaviorTypes;

        public ActionResult Index()
        {
            IReadOnlyCollection<DotaHeroSchemaItem> heroes = GetHeroes();

            var str = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttribute.DOTA_ATTRIBUTE_STRENGTH.ToString()
                && x.Name != "npc_dota_hero_base"
                && x.Enabled);

            var agi = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttribute.DOTA_ATTRIBUTE_AGILITY.ToString()
                && x.Name != "npc_dota_hero_base"
                && x.Enabled);

            var intel = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttribute.DOTA_ATTRIBUTE_INTELLECT.ToString()
                && x.Name != "npc_dota_hero_base"
                && x.Enabled);

            List<HeroSelectItemViewModel> strHeroes = new List<HeroSelectItemViewModel>();
            foreach (var strHero in str)
            {
                strHeroes.Add(new HeroSelectItemViewModel()
                {
                    Id = strHero.HeroId,
                    Name = strHero.Url,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", strHero.Name.Replace("npc_dota_hero_", ""))
                });
            }

            List<HeroSelectItemViewModel> agiHeroes = new List<HeroSelectItemViewModel>();
            foreach (var agiHero in agi)
            {
                agiHeroes.Add(new HeroSelectItemViewModel()
                {
                    Id = agiHero.HeroId,
                    Name = agiHero.Url,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", agiHero.Name.Replace("npc_dota_hero_", ""))
                });
            }

            List<HeroSelectItemViewModel> intHeroes = new List<HeroSelectItemViewModel>();
            foreach (var intHero in intel)
            {
                intHeroes.Add(new HeroSelectItemViewModel()
                {
                    Id = intHero.HeroId,
                    Name = intHero.Url,
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_lg.png", intHero.Name.Replace("npc_dota_hero_", ""))
                });
            }

            HeroSelectViewModel viewModel = new HeroSelectViewModel();
            viewModel.StrengthHeroes = strHeroes.AsReadOnly();
            viewModel.AgilityHeroes = agiHeroes.AsReadOnly();
            viewModel.IntelligenceHeroes = intHeroes.AsReadOnly();

            return View(viewModel);
        }

        private IReadOnlyCollection<DotaHeroSchemaItem> GetHeroes()
        {
            string heroesVdfPath = Path.Combine(AppDataPath, "npc_heroes.vdf");
            string vdf = System.IO.File.ReadAllText(heroesVdfPath);
            var heroes = SourceSchemaParser.SchemaFactory.GetDotaHeroes(vdf);
            return heroes;
        }

        private IReadOnlyCollection<DotaAbilitySchemaItem> GetHeroAbilities()
        {
            string heroesVdfPath = Path.Combine(AppDataPath, "npc_abilities.vdf");
            string vdf = System.IO.File.ReadAllText(heroesVdfPath);
            var abilities = SourceSchemaParser.SchemaFactory.GetDotaHeroAbilities(vdf);
            return abilities;
        }

        private IReadOnlyDictionary<string, string> GetPublicLocalization()
        {
            string vdfPath = Path.Combine(AppDataPath, "public_dota_english.vdf");
            string vdf = System.IO.File.ReadAllText(vdfPath);
            var result = SourceSchemaParser.SchemaFactory.GetDotaPublicLocalizationKeys(vdf);
            return result;
        }

        private string GetLocalizationText(string key)
        {
            string value = String.Empty;
            if (localizationKeys != null && localizationKeys.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return String.Empty;
            }
        }

        private IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType> GetAbilityBehaviorTypes()
        {
            Dictionary<string, DotaHeroAbilityBehaviorType> temp = new Dictionary<string, DotaHeroAbilityBehaviorType>();

            temp.Add(DotaHeroAbilityBehaviorType.HIDDEN.Key, DotaHeroAbilityBehaviorType.HIDDEN);
            temp.Add(DotaHeroAbilityBehaviorType.AOE.Key, DotaHeroAbilityBehaviorType.AOE);
            temp.Add(DotaHeroAbilityBehaviorType.CHANNELLED.Key, DotaHeroAbilityBehaviorType.CHANNELLED);
            temp.Add(DotaHeroAbilityBehaviorType.ITEM.Key, DotaHeroAbilityBehaviorType.ITEM);
            temp.Add(DotaHeroAbilityBehaviorType.NOT_LEARNABLE.Key, DotaHeroAbilityBehaviorType.NOT_LEARNABLE);
            temp.Add(DotaHeroAbilityBehaviorType.NO_TARGET.Key, DotaHeroAbilityBehaviorType.NO_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.PASSIVE.Key, DotaHeroAbilityBehaviorType.PASSIVE);
            temp.Add(DotaHeroAbilityBehaviorType.POINT.Key, DotaHeroAbilityBehaviorType.POINT);
            temp.Add(DotaHeroAbilityBehaviorType.TOGGLE.Key, DotaHeroAbilityBehaviorType.TOGGLE);
            temp.Add(DotaHeroAbilityBehaviorType.UNIT_TARGET.Key, DotaHeroAbilityBehaviorType.UNIT_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.IMMEDIATE.Key, DotaHeroAbilityBehaviorType.IMMEDIATE);
            temp.Add(DotaHeroAbilityBehaviorType.ROOT_DISABLES.Key, DotaHeroAbilityBehaviorType.ROOT_DISABLES);
            temp.Add(DotaHeroAbilityBehaviorType.DONT_RESUME_MOVEMENT.Key, DotaHeroAbilityBehaviorType.DONT_RESUME_MOVEMENT);
            temp.Add(DotaHeroAbilityBehaviorType.IGNORE_BACKSWING.Key, DotaHeroAbilityBehaviorType.IGNORE_BACKSWING);
            temp.Add(DotaHeroAbilityBehaviorType.DONT_RESUME_ATTACK.Key, DotaHeroAbilityBehaviorType.DONT_RESUME_ATTACK);
            temp.Add(DotaHeroAbilityBehaviorType.IGNORE_PSEUDO_QUEUE.Key, DotaHeroAbilityBehaviorType.IGNORE_PSEUDO_QUEUE);
            temp.Add(DotaHeroAbilityBehaviorType.AUTOCAST.Key, DotaHeroAbilityBehaviorType.AUTOCAST);
            temp.Add(DotaHeroAbilityBehaviorType.IGNORE_CHANNEL.Key, DotaHeroAbilityBehaviorType.IGNORE_CHANNEL);
            temp.Add(DotaHeroAbilityBehaviorType.DIRECTIONAL.Key, DotaHeroAbilityBehaviorType.DIRECTIONAL);
            temp.Add(DotaHeroAbilityBehaviorType.AURA.Key, DotaHeroAbilityBehaviorType.AURA);

            return new ReadOnlyDictionary<string, DotaHeroAbilityBehaviorType>(temp);
        }

        public ActionResult Hero(int id)
        {
            localizationKeys = GetPublicLocalization();
            abilityBehaviorTypes = GetAbilityBehaviorTypes();

            IReadOnlyCollection<DotaHeroSchemaItem> heroes = GetHeroes();

            var hero = heroes.FirstOrDefault(x => x.HeroId == id);

            HeroViewModel viewModel = new HeroViewModel()
            {
                Name = GetLocalizationText(hero.Name),
                Description = "<from localization -> npc_dota_hero_<heroname>_hype>",
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_full.png", hero.Name.Replace("npc_dota_hero_", "")),
                BaseAgility = hero.AttributeBaseAgility,
                BaseArmor = hero.ArmorPhysical,
                BaseDamageMax = hero.AttackDamageMax,
                BaseDamageMin = hero.AttackDamageMin,
                BaseMoveSpeed = hero.MovementSpeed,
                BaseStrength = hero.AttributeBaseStrength,
                BaseIntelligence = hero.AttributeBaseIntelligence,
                AttackRate = hero.AttackRate,
                Team = GetTeam(hero.TeamName),
                TurnRate = hero.MovementTurnRate,
                AttackType = GetAttackType(hero.AttackCapabilities),
                Roles = GetRoles(hero.Role, hero.RoleLevels).AsReadOnly(),
                AgilityGain = hero.AttributeAgilityGain,
                IntelligenceGain = hero.AttributeIntelligenceGain,
                StrengthGain = hero.AttributeStrengthGain
            };

            SetupAbilities(hero, viewModel);

            return View(viewModel);
        }

        private void SetupAbilities(DotaHeroSchemaItem hero, HeroViewModel viewModel)
        {
            var abilities = GetHeroAbilities();

            List<HeroAbilityViewModel> abilityViewModels = new List<HeroAbilityViewModel>();

            AddAbilityToViewModel(hero.Ability1, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability2, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability3, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability4, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability5, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability6, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability7, abilities, abilityViewModels);
            AddAbilityToViewModel(hero.Ability8, abilities, abilityViewModels);

            viewModel.Abilities = abilityViewModels.AsReadOnly();
        }

        private void AddAbilityToViewModel(string abilityName, IReadOnlyCollection<DotaAbilitySchemaItem> abilities, List<HeroAbilityViewModel> abilityViewModels)
        {
            if (String.IsNullOrEmpty(abilityName))
            {
                return;
            }

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);

            string[] rawBehaviors = ability.AbilityBehavior.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            List<DotaHeroAbilityBehaviorType> behaviors = rawBehaviors.Select(x => abilityBehaviorTypes[x]).ToList();
            string joinedBehaviors = String.Join(", ", behaviors);

            List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
            foreach (var abilitySpecial in ability.AbilitySpecials)
            {
                abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                {
                    Name = GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value
                });
            }

            abilityViewModels.Add(new HeroAbilityViewModel()
            {
                Name = GetLocalizationText(String.Format("{0}{1}", "DOTA_Tooltip_ability_", abilityName)),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                Description = GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Description")),
                CastPoint = ability.AbilityCastPoint,
                CastRange = ability.AbilityCastRange.Replace(" ", " / "),
                Cooldown = ability.AbilityCooldown.Replace(" ", " / "),
                Damage = ability.AbilityDamage.Replace(" ", " / "),
                DamageType = ability.AbilityUnitDamageType,
                Duration = ability.AbilityDuration.Replace(" ", " / "),
                ManaCost = ability.AbilityManaCost.Replace(" ", " / "),
                SpellImmunityType = ability.SpellImmunityType,
                Attributes = abilitySpecialViewModels,
                Behaviors = joinedBehaviors
            });
        }

        private static string GetAttackType(string attackType)
        {
            if (attackType == DotaAttackType.DOTA_UNIT_CAP_MELEE_ATTACK.ToString())
            {
                return "Melee";
            }
            else if (attackType == DotaAttackType.DOTA_UNIT_CAP_RANGED_ATTACK.ToString())
            {
                return "Ranged";
            }
            else
            {
                return "Undefined";
            }
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

        private static string GetTeam(string team)
        {
            if (team == DotaTeam.DOTA_TEAM_GOODGUYS.ToString())
            {
                return "Radiant";
            }
            else if (team == DotaTeam.DOTA_TEAM_BADGUYS.ToString())
            {
                return "Dire";
            }
            else
            {
                return "Unknown";
            }
        }
    }
}