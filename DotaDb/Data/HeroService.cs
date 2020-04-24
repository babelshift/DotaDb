using DotaDb.Utilities;
using Microsoft.Extensions.Configuration;
using SourceSchemaParser;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class HeroService
    {
        private readonly IConfiguration configuration;
        private readonly ISchemaParser schemaParser;
        private readonly CacheService cacheService;
        private readonly BlobStorageService blobStorageService;
        private readonly LocalizationService localizationService;
        private readonly string minimapIconsBaseUrl;

        // I have seen some of these keys having different casing such as "DOTA_Tooltip_ability" and "DOTA_Tooltip_Ability". Watch out.
        private const string tooltipLocalizationPrefix = "DOTA_Tooltip_ability";

        public HeroService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            CacheService cacheService,
            BlobStorageService blobStorageService,
            LocalizationService localizationService)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.cacheService = cacheService;
            this.blobStorageService = blobStorageService;
            this.localizationService = localizationService;
            minimapIconsBaseUrl = configuration["MinimapIconsBaseUrl"];
        }

        public async Task<IReadOnlyDictionary<uint, HeroSchemaModel>> GetHeroesAsync()
        {
            string fileName = "heroes.vdf";
            string cacheKey = $"parsed_{fileName}";
            return await cacheService.GetOrSetAsync(fileName, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", fileName);
                var heroes = schemaParser.GetDotaHeroes(vdf);
                return heroes.ToDictionary(hero => hero.HeroId, hero => hero);
            }, TimeSpan.FromDays(1));
        }

        public async Task<HeroDetailModel> GetHeroDetailsAsync(uint heroId)
        {
            var heroes = await GetHeroesAsync();
            heroes.TryGetValue(heroId, out HeroSchemaModel heroModel);
            return heroModel != null ? (await GetHeroDetailModelAsync(heroModel)).Value : null;
        }

        public async Task<IReadOnlyDictionary<uint, HeroDetailModel>> GetHeroDetailsAsync()
        {
            var heroes = await GetHeroesAsync();

            List<Task<KeyValuePair<uint, HeroDetailModel>>> tasks = new List<Task<KeyValuePair<uint, HeroDetailModel>>>();
            foreach (var hero in heroes)
            {
                // skip over the "base" hero since we don't care about it
                if (hero.Key == 0) { continue; }

                tasks.Add(GetHeroDetailModelAsync(hero.Value));
            }
            var completedTasks = await Task.WhenAll(tasks);

            var heroDetails = new Dictionary<uint, HeroDetailModel>(completedTasks);
            return new ReadOnlyDictionary<uint, HeroDetailModel>(heroDetails);
        }

        private async Task<KeyValuePair<uint, HeroDetailModel>> GetHeroDetailModelAsync(HeroSchemaModel hero)
        {
            var heroDetail = new HeroDetailModel()
            {
                Id = hero.HeroId,
                Url = !string.IsNullOrWhiteSpace(hero.Url) ? hero.Url.ToLower() : string.Empty,
                Name = await localizationService.GetLocalizationTextAsync(hero.Name),
                Description = await localizationService.GetLocalizationTextAsync($"{hero.Name}_hype"),
                AvatarImagePath = $"http://cdn.dota2.com/apps/dota2/images/heroes/{hero.Name.Replace("npc_dota_hero_", string.Empty)}_full.png",
                BaseAgility = hero.AttributeBaseAgility,
                BaseArmor = hero.ArmorPhysical,
                BaseDamageMax = hero.AttackDamageMax,
                BaseDamageMin = hero.AttackDamageMin,
                BaseMoveSpeed = hero.MovementSpeed,
                BaseStrength = hero.AttributeBaseStrength,
                BaseIntelligence = hero.AttributeBaseIntelligence,
                AttackRate = hero.AttackRate,
                AttackRange = hero.AttackRange,
                Team = GetTeamTypeKeyValue(hero.Team)?.ToString() ?? string.Empty,
                TurnRate = hero.MovementTurnRate,
                AttackType = GetAttackTypeKeyValue(hero.AttackCapabilities)?.ToString() ?? string.Empty,
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
            return new KeyValuePair<uint, HeroDetailModel>(heroDetail.Id, heroDetail);
        }

        public async Task<IReadOnlyCollection<AbilitySchemaItemModel>> GetHeroAbilitiesAsync()
        {
            string fileName = "hero_abilities.vdf";
            string cacheKey = $"parsed_{fileName}";
            return await cacheService.GetOrSetAsync(fileName, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", fileName);
                return schemaParser.GetDotaHeroAbilities(vdf);
            }, TimeSpan.FromDays(1));
        }

        private async Task AddAbilityToViewModelIfNotNull(string abilityName, IReadOnlyCollection<AbilitySchemaItemModel> abilities, List<HeroAbilityDetailModel> abilityDetailModels)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return;
            }

            // Don't return "generic" and "hidden" abilities (I don't know what they are)
            if(abilityName.ToLower().Contains("generic"))
            {
                return;
            }

            abilityDetailModels.Add(await GetHeroAbilityDetailModel(abilityName, abilities));
        }

        /// <summary>
        /// Returns a model containing a single ability's details.
        /// </summary>
        /// <param name="abilityName"></param>
        /// <param name="abilities"></param>
        /// <returns></returns>
        private async Task<HeroAbilityDetailModel> GetHeroAbilityDetailModel(string abilityName, IReadOnlyCollection<AbilitySchemaItemModel> abilities)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return null;
            }

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);
            var abilityDetailModel = new HeroAbilityDetailModel();

            // If the ability type is "ATTRIBUTE", treat it differently since there's less information about them. Also, ATTRIBUTES are now known as TALENTS as of the latest DOTA 2 7.00 patch.
            var abilityType = GetHeroAbilityTypeKeyValue(ability.AbilityType);
            if (abilityType == DotaHeroAbilityType.TALENTS)
            {
                var completedTooltip = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}");

                // Talent tooltips are in a form like this: "+{s:value} Tree Dance Vision AoE"
                // We need to replace the "{s:value}" token with a value found in the "ability special" list
                var replaceableTokens = Regex.Matches(completedTooltip, @"{s:(\w+)}");
                foreach(Match token in replaceableTokens)
                {
                    var abilitySpecialValue = ability.AbilitySpecials.FirstOrDefault(x => x.Name == token.Groups[1].Value);
                    completedTooltip = completedTooltip.Replace(token.Value, abilitySpecialValue.Value);
                }

                abilityDetailModel = new HeroAbilityDetailModel()
                {
                    Id = ability.Id,
                    Name = completedTooltip,
                    AbilityType = abilityType,
                };
            }
            else
            {
                var abilityNameTask = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}");
                var descriptionTask = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Description");
                var abilitySpecialTask = GetHeroAbilitySpecialDetailModels(abilityName, tooltipLocalizationPrefix, ability);
                var note0Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note0");
                var note1Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note1");
                var note2Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note2");
                var note3Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note3");
                var note4Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note4");
                var note5Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note5");
                var note6Task = localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_Note6");

                abilityDetailModel = new HeroAbilityDetailModel()
                {
                    Id = ability.Id,
                    Name = await abilityNameTask,
                    AvatarImagePath = $"http://cdn.dota2.com/apps/dota2/images/abilities/{ability.Name}_lg.png",
                    Description = await descriptionTask,
                    CastPoint = ability.AbilityCastPoint.ToSlashSeparatedString(),
                    CastRange = ability.AbilityCastRange.ToSlashSeparatedString(),
                    Cooldown = ability.AbilityCooldown.ToSlashSeparatedString(),
                    Damage = ability.AbilityDamage.ToSlashSeparatedString(),
                    DamageType = GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                    Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                    ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                    SpellImmunityType = GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                    Attributes = await abilitySpecialTask,
                    Behaviors = GetJoinedBehaviors(ability.AbilityBehavior),
                    AbilityType = abilityType,
                    TargetFlags = GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags),
                    TargetTypes = GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType),
                    TeamTargets = GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam),
                    Note0 = await note0Task,
                    Note1 = await note1Task,
                    Note2 = await note2Task,
                    Note3 = await note3Task,
                    Note4 = await note4Task,
                    Note5 = await note5Task,
                    Note6 = await note6Task
                };
            }

            return abilityDetailModel;
        }

        private async Task<List<HeroAbilitySpecialDetailModel>> GetHeroAbilitySpecialDetailModels(string abilityName, string tooltipLocalizationPrefix, AbilitySchemaItemModel ability)
        {
            var abilitySpecialDetailModels = ability.AbilitySpecials
                .Select(async abilitySpecial => new HeroAbilitySpecialDetailModel()
                {
                    Name = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_{abilitySpecial.Name}"),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value.ToSlashSeparatedString(),
                    LinkedSpecialBonus = abilitySpecial.LinkedSpecialBonus
                });
            return (await Task.WhenAll(abilitySpecialDetailModels)).ToList();
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
            if (string.IsNullOrWhiteSpace(startingValue))
            {
                return string.Empty;
            }

            string[] raw = startingValue.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
            List<T> individual = raw.Select(x => GetKeyValue(x, lookup)).ToList();
            return string.Join(", ", individual);
        }

        private T GetKeyValue<T, K>(K key, IReadOnlyDictionary<K, T> dict)
        {
            if (key == null || string.IsNullOrWhiteSpace(key.ToString()))
            {
                return default(T);
            }

            if (dict != null && dict.TryGetValue(key, out T value))
            {
                return value;
            }
            else
            {
                return default(T);
            }
        }
    }
}