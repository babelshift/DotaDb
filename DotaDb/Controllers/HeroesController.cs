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
        private IReadOnlyDictionary<string, string> localizationKeys;
        private IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType> abilityBehaviorTypes;
        private IReadOnlyDictionary<string, DotaHeroAbilityType> abilityTypes;
        private IReadOnlyDictionary<string, DotaAttackType> attackTypes;
        private IReadOnlyDictionary<string, DotaTeamType> teamTypes;
        private IReadOnlyDictionary<string, DotaDamageType> damageTypes;
        private IReadOnlyDictionary<string, DotaSpellImmunityType> spellImmunityTypes;
        private IReadOnlyDictionary<string, DotaUnitTargetFlag> unitTargetFlags;
        private IReadOnlyDictionary<string, DotaUnitTargetTeamType> unitTargetTeamTypes;
        private IReadOnlyDictionary<string, DotaUnitTargetType> unitTargetTypes;

        private string AppDataPath { get { return AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); } }

        #region In Memory "Database"

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
            temp.Add(DotaHeroAbilityBehaviorType.DONT_ALERT_TARGET.Key, DotaHeroAbilityBehaviorType.DONT_ALERT_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.DONT_CANCEL_MOVEMENT.Key, DotaHeroAbilityBehaviorType.DONT_CANCEL_MOVEMENT);
            temp.Add(DotaHeroAbilityBehaviorType.NORMAL_WHEN_STOLEN.Key, DotaHeroAbilityBehaviorType.NORMAL_WHEN_STOLEN);
            temp.Add(DotaHeroAbilityBehaviorType.RUNE_TARGET.Key, DotaHeroAbilityBehaviorType.RUNE_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.UNRESTRICTED.Key, DotaHeroAbilityBehaviorType.UNRESTRICTED);

            return new ReadOnlyDictionary<string, DotaHeroAbilityBehaviorType>(temp);
        }

        private IReadOnlyDictionary<string, DotaHeroAbilityType> GetHeroAbilityTypes()
        {
            Dictionary<string, DotaHeroAbilityType> temp = new Dictionary<string, DotaHeroAbilityType>();

            temp.Add(DotaHeroAbilityType.BASIC.Key, DotaHeroAbilityType.BASIC);
            temp.Add(DotaHeroAbilityType.ULTIMATE.Key, DotaHeroAbilityType.ULTIMATE);
            temp.Add(DotaHeroAbilityType.ATTRIBUTES.Key, DotaHeroAbilityType.ATTRIBUTES);

            return new ReadOnlyDictionary<string, DotaHeroAbilityType>(temp);
        }

        private IReadOnlyDictionary<string, DotaAttackType> GetAttackTypes()
        {
            Dictionary<string, DotaAttackType> temp = new Dictionary<string, DotaAttackType>();

            temp.Add(DotaAttackType.MELEE.Key, DotaAttackType.MELEE);
            temp.Add(DotaAttackType.RANGED.Key, DotaAttackType.RANGED);

            return new ReadOnlyDictionary<string, DotaAttackType>(temp);
        }

        private IReadOnlyDictionary<string, DotaHeroPrimaryAttributeType> GetPrimaryAttributeTypes()
        {
            Dictionary<string, DotaHeroPrimaryAttributeType> temp = new Dictionary<string, DotaHeroPrimaryAttributeType>();

            temp.Add(DotaHeroPrimaryAttributeType.AGILITY.Key, DotaHeroPrimaryAttributeType.AGILITY);
            temp.Add(DotaHeroPrimaryAttributeType.INTELLECT.Key, DotaHeroPrimaryAttributeType.INTELLECT);
            temp.Add(DotaHeroPrimaryAttributeType.STRENGTH.Key, DotaHeroPrimaryAttributeType.STRENGTH);

            return new ReadOnlyDictionary<string, DotaHeroPrimaryAttributeType>(temp);
        }

        private IReadOnlyDictionary<string, DotaTeamType> GetTeamTypes()
        {
            Dictionary<string, DotaTeamType> temp = new Dictionary<string, DotaTeamType>();

            temp.Add(DotaTeamType.BAD.Key, DotaTeamType.BAD);
            temp.Add(DotaTeamType.GOOD.Key, DotaTeamType.GOOD);

            return new ReadOnlyDictionary<string, DotaTeamType>(temp);
        }

        private IReadOnlyDictionary<string, DotaDamageType> GetDamageTypes()
        {
            Dictionary<string, DotaDamageType> temp = new Dictionary<string, DotaDamageType>();

            temp.Add(DotaDamageType.MAGICAL.Key, DotaDamageType.MAGICAL);
            temp.Add(DotaDamageType.PHYSICAL.Key, DotaDamageType.PHYSICAL);
            temp.Add(DotaDamageType.PURE.Key, DotaDamageType.PURE);

            return new ReadOnlyDictionary<string, DotaDamageType>(temp);
        }

        private IReadOnlyDictionary<string, DotaSpellImmunityType> GetSpellImmunityTypes()
        {
            Dictionary<string, DotaSpellImmunityType> temp = new Dictionary<string, DotaSpellImmunityType>();

            temp.Add(DotaSpellImmunityType.ALLIES_NO.Key, DotaSpellImmunityType.ALLIES_NO);
            temp.Add(DotaSpellImmunityType.ALLIES_YES.Key, DotaSpellImmunityType.ALLIES_YES);
            temp.Add(DotaSpellImmunityType.ENEMIES_NO.Key, DotaSpellImmunityType.ENEMIES_NO);
            temp.Add(DotaSpellImmunityType.ENEMIES_YES.Key, DotaSpellImmunityType.ENEMIES_YES);

            return new ReadOnlyDictionary<string, DotaSpellImmunityType>(temp);
        }

        private IReadOnlyDictionary<string, DotaUnitTargetFlag> GetUnitTargetFlags()
        {
            Dictionary<string, DotaUnitTargetFlag> temp = new Dictionary<string, DotaUnitTargetFlag>();

            temp.Add(DotaUnitTargetFlag.INVULNERABLE.Key, DotaUnitTargetFlag.INVULNERABLE);
            temp.Add(DotaUnitTargetFlag.MAGIC_IMMUNE_ENEMIES.Key, DotaUnitTargetFlag.MAGIC_IMMUNE_ENEMIES);
            temp.Add(DotaUnitTargetFlag.NOT_ANCIENTS.Key, DotaUnitTargetFlag.NOT_ANCIENTS);
            temp.Add(DotaUnitTargetFlag.NOT_CREEP_HERO.Key, DotaUnitTargetFlag.NOT_CREEP_HERO);
            temp.Add(DotaUnitTargetFlag.NOT_MAGIC_IMMUNE_ALLIES.Key, DotaUnitTargetFlag.NOT_MAGIC_IMMUNE_ALLIES);
            temp.Add(DotaUnitTargetFlag.NOT_SUMMONED.Key, DotaUnitTargetFlag.NOT_SUMMONED);

            return new ReadOnlyDictionary<string, DotaUnitTargetFlag>(temp);
        }

        private IReadOnlyDictionary<string, DotaUnitTargetTeamType> GetUnitTargetTeamTypes()
        {
            Dictionary<string, DotaUnitTargetTeamType> temp = new Dictionary<string, DotaUnitTargetTeamType>();

            temp.Add(DotaUnitTargetTeamType.BOTH.Key, DotaUnitTargetTeamType.BOTH);
            temp.Add(DotaUnitTargetTeamType.CUSTOM.Key, DotaUnitTargetTeamType.CUSTOM);
            temp.Add(DotaUnitTargetTeamType.ENEMY.Key, DotaUnitTargetTeamType.ENEMY);
            temp.Add(DotaUnitTargetTeamType.FRIENDLY.Key, DotaUnitTargetTeamType.FRIENDLY);

            return new ReadOnlyDictionary<string, DotaUnitTargetTeamType>(temp);
        }

        private IReadOnlyDictionary<string, DotaUnitTargetType> GetUnitTargetTypes()
        {
            Dictionary<string, DotaUnitTargetType> temp = new Dictionary<string, DotaUnitTargetType>();

            temp.Add(DotaUnitTargetType.BASIC.Key, DotaUnitTargetType.BASIC);
            temp.Add(DotaUnitTargetType.BUILDING.Key, DotaUnitTargetType.BUILDING);
            temp.Add(DotaUnitTargetType.CUSTOM.Key, DotaUnitTargetType.CUSTOM);
            temp.Add(DotaUnitTargetType.HERO.Key, DotaUnitTargetType.HERO);

            return new ReadOnlyDictionary<string, DotaUnitTargetType>(temp);
        }

        #endregion In Memory "Database"

        #region Hero Index

        public ActionResult Index()
        {
            IReadOnlyCollection<DotaHeroSchemaItem> heroes = GetHeroes();

            var str = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttributeType.STRENGTH.Key
                && x.Name != "npc_dota_hero_base"
                && x.Enabled);

            var agi = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttributeType.AGILITY.Key
                && x.Name != "npc_dota_hero_base"
                && x.Enabled);

            var intel = heroes.Where(x =>
                x.AttributePrimary == DotaHeroPrimaryAttributeType.INTELLECT.Key
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

        #endregion

        #region Hero Specifics

        public ActionResult Hero(int id)
        {
            localizationKeys = GetPublicLocalization();
            abilityBehaviorTypes = GetAbilityBehaviorTypes();
            attackTypes = GetAttackTypes();
            teamTypes = GetTeamTypes();
            abilityTypes = GetHeroAbilityTypes();
            spellImmunityTypes = GetSpellImmunityTypes();
            damageTypes = GetDamageTypes();
            unitTargetFlags = GetUnitTargetFlags();
            unitTargetTeamTypes = GetUnitTargetTeamTypes();
            unitTargetTypes = GetUnitTargetTypes();

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
                Team = GetKeyValue(hero.Team, teamTypes).ToString(),
                TurnRate = hero.MovementTurnRate,
                AttackType = GetKeyValue(hero.AttackCapabilities, attackTypes).ToString(),
                Roles = GetRoles(hero.Role, hero.RoleLevels).AsReadOnly(),
                AgilityGain = hero.AttributeAgilityGain,
                IntelligenceGain = hero.AttributeIntelligenceGain,
                StrengthGain = hero.AttributeStrengthGain
            };

            SetupAbilities(hero, viewModel);

            return View(viewModel);
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

            string joinedBehaviors = GetJoinedValues(ability.AbilityBehavior, abilityBehaviorTypes);
            string joinedUnitTargetTeamTypes = GetJoinedValues(ability.AbilityUnitTargetTeam, unitTargetTeamTypes);
            string joinedUnitTargetTypes = GetJoinedValues(ability.AbilityUnitTargetType, unitTargetTypes);
            string joinedUnitTargetFlags = GetJoinedValues(ability.AbilityUnitTargetFlags, unitTargetFlags);

            List<HeroAbilitySpecialViewModel> abilitySpecialViewModels = new List<HeroAbilitySpecialViewModel>();
            foreach (var abilitySpecial in ability.AbilitySpecials)
            {
                abilitySpecialViewModels.Add(new HeroAbilitySpecialViewModel()
                {
                    Name = GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString()
                });
            }

            var abilityViewModel = new HeroAbilityViewModel()
            {
                Name = GetLocalizationText(String.Format("{0}_{1}", "DOTA_Tooltip_ability", abilityName)),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                Description = GetLocalizationText(String.Format("{0}_{1}_{2}", "DOTA_Tooltip_ability", abilityName, "Description")),
                CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString(),
                CastRange = ability.AbilityCastRange.ToSlashSeparatedString(),
                Cooldown = ability.AbilityCooldown.ToSlashSeparatedString(),
                Damage = ability.AbilityDamage.ToSlashSeparatedString(),
                DamageType = GetKeyValue(ability.AbilityUnitDamageType, damageTypes),
                Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                SpellImmunityType = GetKeyValue(ability.SpellImmunityType, spellImmunityTypes),
                Attributes = abilitySpecialViewModels,
                Behaviors = joinedBehaviors,
                AbilityType = GetKeyValue(ability.AbilityType, abilityTypes),
                TargetFlags = joinedUnitTargetFlags,
                TargetTypes = joinedUnitTargetTypes,
                TeamTargets = joinedUnitTargetTeamTypes
            };

            abilityViewModels.Add(abilityViewModel);
        }

        private string GetJoinedValues<T>(string startingValue, IReadOnlyDictionary<string, T> lookup)
        {
            if (String.IsNullOrEmpty(startingValue))
            {
                return String.Empty;
            }

            string[] raw = startingValue.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            List<T> individual = raw.Select(x => GetKeyValue(x, lookup)).ToList();
            return String.Join(", ", individual);
        }

        private T GetKeyValue<T>(string key, IReadOnlyDictionary<string, T> dict)
        {
            if (String.IsNullOrEmpty(key))
            {
                return default(T);
            }

            T value;
            if (dict != null && dict.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return default(T);
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

        #endregion
    }
}