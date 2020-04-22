using SourceSchemaParser;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DotaDb.Utilities;
using DotaDb.Models;
using Microsoft.Extensions.Configuration;

namespace DotaDb.Data
{
    public class HeroService
    {
        private readonly IConfiguration configuration;
        private readonly ISchemaParser schemaParser;
        private readonly BlobStorageService blobStorageService;
        private readonly LocalizationService localizationService;
        private readonly string minimapIconsBaseUrl;

        public HeroService(
            IConfiguration configuration, 
            ISchemaParser schemaParser,
            BlobStorageService blobStorageService,
            LocalizationService localizationService)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.blobStorageService = blobStorageService;
            this.localizationService = localizationService;
            minimapIconsBaseUrl = configuration["MinimapIconsBaseUrl"];
        }

        public async Task<HeroDetailModel> GetHeroDetailsAsync(uint id)
        {
            var heroes = await GetHeroesAsync();

            heroes.TryGetValue(id, out HeroSchemaModel heroModel);

            if (heroModel != null)
            {
                return await GetHeroDetailModelAsync(heroModel);
            }

            return null;
        }

        public async Task<IReadOnlyDictionary<uint, HeroDetailModel>> GetHeroDetailsAsync()
        {
            Dictionary<uint, HeroDetailModel> heroDetails = new Dictionary<uint, HeroDetailModel>();

            var heroes = await GetHeroesAsync();

            foreach (var hero in heroes)
            {
                // skip over the "base" hero since we don't care about it
                if (hero.Key == 0)
                {
                    continue;
                }

                HeroDetailModel heroDetail = await GetHeroDetailModelAsync(hero.Value);

                heroDetails.Add(hero.Key, heroDetail);
            }

            return new ReadOnlyDictionary<uint, HeroDetailModel>(heroDetails);
        }

        private async Task<HeroDetailModel> GetHeroDetailModelAsync(HeroSchemaModel hero)
        {
            var heroDetail = new HeroDetailModel()
            {
                Id = hero.HeroId,
                Url = !String.IsNullOrEmpty(hero.Url) ? hero.Url.ToLower() : String.Empty,
                Name = await localizationService.GetLocalizationTextAsync(hero.Name),
                Description = await localizationService.GetLocalizationTextAsync($"{hero.Name}_bio"),
                AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/heroes/{0}_full.png", hero.Name.Replace("npc_dota_hero_", "")),
                BaseAgility = hero.AttributeBaseAgility,
                BaseArmor = hero.ArmorPhysical,
                BaseDamageMax = hero.AttackDamageMax,
                BaseDamageMin = hero.AttackDamageMin,
                BaseMoveSpeed = hero.MovementSpeed,
                BaseStrength = hero.AttributeBaseStrength,
                BaseIntelligence = hero.AttributeBaseIntelligence,
                AttackRate = hero.AttackRate,
                AttackRange = hero.AttackRange,
                Team = GetTeamTypeKeyValue(hero.Team) != null ? GetTeamTypeKeyValue(hero.Team).ToString() : String.Empty,
                TurnRate = hero.MovementTurnRate,
                AttackType = GetAttackTypeKeyValue(hero.AttackCapabilities) != null ? GetAttackTypeKeyValue(hero.AttackCapabilities).ToString() : String.Empty,
                Roles = hero.GetRoles(),
                AgilityGain = hero.AttributeAgilityGain,
                IntelligenceGain = hero.AttributeIntelligenceGain,
                StrengthGain = hero.AttributeStrengthGain,
                PrimaryAttribute = GetAttributeTypeKeyValue(hero.AttributePrimary),
                MinimapIconPath = hero.GetMinimapIconFilePath(minimapIconsBaseUrl),
                IsEnabled = hero.Enabled,
                NameInSchema = hero.Name
            };

            var abilities = await GetHeroAbilitiesAsync();

            List<HeroAbilityDetailModel> abilityDetailModels = new List<HeroAbilityDetailModel>();

            await AddAbilityToViewModelIfNotNull(hero.Ability1, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability2, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability3, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability4, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability6, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability7, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability8, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability9, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability10, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability11, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability12, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability13, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability14, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability15, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability16, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability17, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability18, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability19, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability20, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability21, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability22, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability23, abilities, abilityDetailModels);
            await AddAbilityToViewModelIfNotNull(hero.Ability24, abilities, abilityDetailModels);

            heroDetail.Abilities = abilityDetailModels;
            return heroDetail;
        }

        public async Task<IReadOnlyDictionary<uint, HeroSchemaModel>> GetHeroesAsync()
        {
            var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", "heroes.vdf");
            var heroes = schemaParser.GetDotaHeroes(vdf);
            return heroes.ToDictionary(x => x.HeroId, x => x);
        }

        public async Task<IReadOnlyCollection<AbilitySchemaItemModel>> GetHeroAbilitiesAsync()
        {
            var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", "hero_abilities.vdf");
            var abilities = schemaParser.GetDotaHeroAbilities(vdf);
            return abilities;
        }

        private async Task AddAbilityToViewModelIfNotNull(string abilityName, IReadOnlyCollection<AbilitySchemaItemModel> abilities, List<HeroAbilityDetailModel> abilityDetailModels)
        {
            if (!String.IsNullOrEmpty(abilityName))
            {
                abilityDetailModels.Add(await GetHeroAbilityDetailModel(abilityName, abilities));
            }
        }

        /// <summary>
        /// Returns a model containing a single ability's details.
        /// </summary>
        /// <param name="abilityName"></param>
        /// <param name="abilities"></param>
        /// <returns></returns>
        private async Task<HeroAbilityDetailModel> GetHeroAbilityDetailModel(string abilityName, IReadOnlyCollection<AbilitySchemaItemModel> abilities)
        {
            if (String.IsNullOrWhiteSpace(abilityName))
            {
                return null;
            }

            // I have seen some of these keys having different casing such as "DOTA_Tooltip_ability" and "DOTA_Tooltip_Ability". Watch out.
            string tooltipLocalizationPrefix = "DOTA_Tooltip_ability";

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);

            // Join together various flags and attributes on an ability to a single string for display purposes
            string joinedBehaviors = GetJoinedBehaviors(ability.AbilityBehavior);
            string joinedUnitTargetTeamTypes = GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam);
            string joinedUnitTargetTypes = GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType);
            string joinedUnitTargetFlags = GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags);

            List<HeroAbilitySpecialDetailModel> abilitySpecialDetailModels = new List<HeroAbilitySpecialDetailModel>();
            foreach (var abilitySpecial in ability.AbilitySpecials)
            {
                var abilitySpecialDetail = new HeroAbilitySpecialDetailModel()
                {
                    Name = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, abilitySpecial.Name)),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString(),
                    LinkedSpecialBonus = abilitySpecial.LinkedSpecialBonus
                };

                // Remove underscores from the displayed name for readability
                if (!String.IsNullOrWhiteSpace(abilitySpecialDetail.Name))
                {
                    abilitySpecialDetail.Name = abilitySpecialDetail.RawName.Replace("_", " ");
                }

                abilitySpecialDetailModels.Add(abilitySpecialDetail);
            }


            DotaHeroAbilityType abilityType = GetHeroAbilityTypeKeyValue(ability.AbilityType);

            HeroAbilityDetailModel abilityDetailModel = new HeroAbilityDetailModel();

            // If the ability type is "ATTRIBUTE", treat it differently since there's less information about them. Also, ATTRIBUTES are now known as TALENTS as of the latest DOTA 2 7.00 patch.
            if (abilityType == DotaHeroAbilityType.TALENTS)
            {
                abilityDetailModel = new HeroAbilityDetailModel()
                {
                    Id = ability.Id,
                    Name = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}", tooltipLocalizationPrefix, abilityName)),
                    AbilityType = abilityType,
                };
            }
            else
            {
                abilityDetailModel = new HeroAbilityDetailModel()
                {
                    Id = ability.Id,
                    Name = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}", tooltipLocalizationPrefix, abilityName)),
                    AvatarImagePath = String.Format("http://cdn.dota2.com/apps/dota2/images/abilities/{0}_lg.png", ability.Name),
                    Description = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Description")),
                    CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString(),
                    CastRange = ability.AbilityCastRange.ToSlashSeparatedString(),
                    Cooldown = ability.AbilityCooldown.ToSlashSeparatedString(),
                    Damage = ability.AbilityDamage.ToSlashSeparatedString(),
                    DamageType = GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                    Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                    ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                    SpellImmunityType = GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                    Attributes = abilitySpecialDetailModels,
                    Behaviors = joinedBehaviors,
                    AbilityType = abilityType,
                    TargetFlags = joinedUnitTargetFlags,
                    TargetTypes = joinedUnitTargetTypes,
                    TeamTargets = joinedUnitTargetTeamTypes,
                    Note0 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note0")),
                    Note1 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note1")),
                    Note2 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note2")),
                    Note3 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note3")),
                    Note4 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note4")),
                    Note5 = await localizationService.GetLocalizationTextAsync(String.Format("{0}_{1}_{2}", tooltipLocalizationPrefix, abilityName, "Note5"))
                };
            }

            return abilityDetailModel;
        }

        private DotaTeamType GetTeamTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetTeamTypes());
        }

        private IReadOnlyDictionary<string, DotaTeamType> GetTeamTypes()
        {
            Dictionary<string, DotaTeamType> temp = new Dictionary<string, DotaTeamType>();

            temp.Add(DotaTeamType.BAD.Key, DotaTeamType.BAD);
            temp.Add(DotaTeamType.GOOD.Key, DotaTeamType.GOOD);

            return new ReadOnlyDictionary<string, DotaTeamType>(temp);
        }

        private DotaAttackType GetAttackTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetAttackTypes());
        }

        private IReadOnlyDictionary<string, DotaAttackType> GetAttackTypes()
        {
            Dictionary<string, DotaAttackType> temp = new Dictionary<string, DotaAttackType>();

            temp.Add(DotaAttackType.MELEE.Key, DotaAttackType.MELEE);
            temp.Add(DotaAttackType.RANGED.Key, DotaAttackType.RANGED);

            return new ReadOnlyDictionary<string, DotaAttackType>(temp);
        }

        private DotaHeroPrimaryAttributeType GetAttributeTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetAttributeTypes());
        }

        private IReadOnlyDictionary<string, DotaHeroPrimaryAttributeType> GetAttributeTypes()
        {
            Dictionary<string, DotaHeroPrimaryAttributeType> temp = new Dictionary<string, DotaHeroPrimaryAttributeType>();

            temp.Add(DotaHeroPrimaryAttributeType.AGILITY.Key, DotaHeroPrimaryAttributeType.AGILITY);
            temp.Add(DotaHeroPrimaryAttributeType.INTELLECT.Key, DotaHeroPrimaryAttributeType.INTELLECT);
            temp.Add(DotaHeroPrimaryAttributeType.STRENGTH.Key, DotaHeroPrimaryAttributeType.STRENGTH);

            return new ReadOnlyDictionary<string, DotaHeroPrimaryAttributeType>(temp);
        }

        private DotaDamageType GetDamageTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetDamageTypes());
        }

        private IReadOnlyDictionary<string, DotaDamageType> GetDamageTypes()
        {
            Dictionary<string, DotaDamageType> temp = new Dictionary<string, DotaDamageType>();

            temp.Add(DotaDamageType.MAGICAL.Key, DotaDamageType.MAGICAL);
            temp.Add(DotaDamageType.PHYSICAL.Key, DotaDamageType.PHYSICAL);
            temp.Add(DotaDamageType.PURE.Key, DotaDamageType.PURE);

            return new ReadOnlyDictionary<string, DotaDamageType>(temp);
        }

        private DotaSpellImmunityType GetSpellImmunityTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetSpellImmunityTypes());
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

        private DotaHeroAbilityType GetHeroAbilityTypeKeyValue(string key)
        {
            return GetKeyValue(key, GetHeroAbilityTypes());
        }

        private IReadOnlyDictionary<string, DotaHeroAbilityType> GetHeroAbilityTypes()
        {
            Dictionary<string, DotaHeroAbilityType> temp = new Dictionary<string, DotaHeroAbilityType>();

            temp.Add(DotaHeroAbilityType.BASIC.Key, DotaHeroAbilityType.BASIC);
            temp.Add(DotaHeroAbilityType.ULTIMATE.Key, DotaHeroAbilityType.ULTIMATE);
            temp.Add(DotaHeroAbilityType.TALENTS.Key, DotaHeroAbilityType.TALENTS);

            return new ReadOnlyDictionary<string, DotaHeroAbilityType>(temp);
        }

        private T GetKeyValue<T, K>(K key, IReadOnlyDictionary<K, T> dict)
        {
            if (key == null || String.IsNullOrEmpty(key.ToString()))
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

        public string GetJoinedUnitTargetFlags(string value)
        {
            return GetJoinedValues(value, GetUnitTargetFlags());
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

        public string GetJoinedUnitTargetTypes(string value)
        {
            return GetJoinedValues(value, GetUnitTargetTypes());
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

        private string GetJoinedUnitTargetTeamTypes(string value)
        {
            return GetJoinedValues(value, GetUnitTargetTeamTypes());
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

        private string GetJoinedBehaviors(string value)
        {
            return GetJoinedValues(value, GetAbilityBehaviorTypes());
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
            temp.Add(DotaHeroAbilityBehaviorType.DONT_ALERT_TARGET.Key, DotaHeroAbilityBehaviorType.DONT_ALERT_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.DONT_CANCEL_MOVEMENT.Key, DotaHeroAbilityBehaviorType.DONT_CANCEL_MOVEMENT);
            temp.Add(DotaHeroAbilityBehaviorType.NORMAL_WHEN_STOLEN.Key, DotaHeroAbilityBehaviorType.NORMAL_WHEN_STOLEN);
            temp.Add(DotaHeroAbilityBehaviorType.RUNE_TARGET.Key, DotaHeroAbilityBehaviorType.RUNE_TARGET);
            temp.Add(DotaHeroAbilityBehaviorType.UNRESTRICTED.Key, DotaHeroAbilityBehaviorType.UNRESTRICTED);

            return new ReadOnlyDictionary<string, DotaHeroAbilityBehaviorType>(temp);
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
    }
}
