using DotaDb.ViewModels;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotaDb.Controllers
{
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

        private static IReadOnlyCollection<SourceSchemaParser.Dota2.DotaHeroSchemaItem> GetHeroes()
        {
            string appDataPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string heroesVdfPath = Path.Combine(appDataPath, "npc_heroes.vdf");
            string vdf = System.IO.File.ReadAllText(heroesVdfPath);
            var heroes = SourceSchemaParser.SchemaFactory.GetDotaHeroes(vdf, String.Empty);
            return heroes;
        }

        public ActionResult Hero(int id)
        {
            IReadOnlyCollection<DotaHeroSchemaItem> heroes = GetHeroes();

            var hero = heroes.FirstOrDefault(x => x.HeroId == id);

            HeroViewModel viewModel = new HeroViewModel()
            {
                Name = hero.Url.Replace("_", " "),
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

            if(!String.IsNullOrEmpty(hero.Ability1))
            {

            }

            return View(viewModel);
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
            if(team == DotaTeam.DOTA_TEAM_GOODGUYS.ToString())
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