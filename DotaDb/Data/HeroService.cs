using DotaDb.Utilities;
using Microsoft.Extensions.Configuration;
using SourceSchemaParser;
using Steam.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private readonly SharedService sharedService;

        private readonly string heroesFileName;
        private readonly string heroAbilitiesFileName;
        private readonly string minimapIconsBaseUrl;
        private readonly string heroAvatarsBaseUrl;
        private readonly string heroAbilityIconsBaseUrl;

        private const string tooltipLocalizationPrefix = "DOTA_Tooltip_ability";
        private const string heroNamePrefix = "npc_dota_hero_";

        public HeroService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            CacheService cacheService,
            BlobStorageService blobStorageService,
            LocalizationService localizationService,
            SharedService sharedService)
        {
            this.configuration = configuration;
            this.schemaParser = schemaParser;
            this.cacheService = cacheService;
            this.blobStorageService = blobStorageService;
            this.localizationService = localizationService;
            this.sharedService = sharedService;

            minimapIconsBaseUrl = configuration["ImageUrls:MinimapIconsBaseUrl"];
            heroesFileName = configuration["FileNames:Heroes"];
            heroAbilitiesFileName = configuration["FileNames:HeroAbilities"];
            heroAvatarsBaseUrl = configuration["ImageUrls:HeroAvatarsBaseUrl"];
            heroAbilityIconsBaseUrl = configuration["ImageUrls:HeroAbilityIconsBaseUrl"];
        }

        public async Task<IReadOnlyDictionary<uint, HeroSchema>> GetHeroesAsync()
        {
            string cacheKey = $"parsed_{heroesFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", heroesFileName);
                var heroes = schemaParser.GetDotaHeroes(vdf);
                return new ReadOnlyDictionary<uint, HeroSchema>(heroes.ToDictionary(hero => hero.HeroId, hero => hero));
            }, TimeSpan.FromDays(1));
        }

        public async Task<HeroDetail> GetHeroDetailsAsync(uint heroId)
        {
            var heroes = await GetHeroesAsync();
            heroes.TryGetValue(heroId, out HeroSchema heroModel);
            return heroModel != null ? (await GetHeroDetailAsync(heroModel)).Value : null;
        }

        public async Task<IReadOnlyDictionary<uint, HeroDetail>> GetHeroDetailsAsync()
        {
            var heroes = await GetHeroesAsync();

            List<Task<KeyValuePair<uint, HeroDetail>>> tasks = new List<Task<KeyValuePair<uint, HeroDetail>>>();
            foreach (var hero in heroes)
            {
                // skip over the "base" hero since we don't care about it
                if (hero.Key == 0) { continue; }

                tasks.Add(GetHeroDetailAsync(hero.Value));
            }
            var completedTasks = await Task.WhenAll(tasks);

            var heroDetails = new Dictionary<uint, HeroDetail>(completedTasks);
            return new ReadOnlyDictionary<uint, HeroDetail>(heroDetails);
        }

        public async Task<string> GetHeroNameAsync(HeroSchema hero)
        {
            // For some reason, the latest 2 heroes added in patch 7.23 don't have localization names, so we do our best to figure it out
            if (hero.Name.EndsWith("snapfire") || hero.Name.EndsWith("void_spirit"))
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string heroName = hero.Name.Replace(heroNamePrefix, string.Empty);
                heroName = heroName.ReplaceUnderscoresWithSpaces();
                return textInfo.ToTitleCase(heroName);
            }

            return await localizationService.GetLocalizationTextAsync(hero.Name);
        }

        private async Task<KeyValuePair<uint, HeroDetail>> GetHeroDetailAsync(HeroSchema hero)
        {
            var heroDetail = new HeroDetail()
            {
                Id = hero.HeroId,
                Url = !string.IsNullOrWhiteSpace(hero.Url) ? hero.Url.ToLower() : string.Empty,
                Name = await GetHeroNameAsync(hero),
                Description = await localizationService.GetLocalizationTextAsync($"{hero.Name}_hype"), // TODO: what happened to "bio"?
                AvatarImagePath = $"{heroAvatarsBaseUrl}{hero.Name.Replace(heroNamePrefix, string.Empty)}_full.png",
                BaseAgility = hero.AttributeBaseAgility,
                BaseArmor = hero.ArmorPhysical,
                BaseDamageMax = hero.AttackDamageMax,
                BaseDamageMin = hero.AttackDamageMin,
                BaseMoveSpeed = hero.MovementSpeed,
                BaseStrength = hero.AttributeBaseStrength,
                BaseIntelligence = hero.AttributeBaseIntelligence,
                AttackRate = hero.AttackRate,
                AttackRange = hero.AttackRange,
                Team = sharedService.GetTeamTypeKeyValue(hero.Team)?.ToString() ?? string.Empty,
                TurnRate = hero.MovementTurnRate,
                AttackType = sharedService.GetAttackTypeKeyValue(hero.AttackCapabilities)?.ToString() ?? string.Empty,
                Roles = hero.GetRoles(),
                AgilityGain = hero.AttributeAgilityGain,
                IntelligenceGain = hero.AttributeIntelligenceGain,
                StrengthGain = hero.AttributeStrengthGain,
                PrimaryAttribute = sharedService.GetAttributeTypeKeyValue(hero.AttributePrimary),
                MinimapIconPath = hero.GetMinimapIconFilePath(minimapIconsBaseUrl),
                IsEnabled = hero.Enabled,
                NameInSchema = hero.Name
            };

            var abilities = await GetHeroAbilitiesAsync();

            List<HeroAbilityDetail> abilityDetailModels = new List<HeroAbilityDetail>();

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
            return new KeyValuePair<uint, HeroDetail>(heroDetail.Id, heroDetail);
        }

        public async Task<IReadOnlyCollection<HeroAbility>> GetHeroAbilitiesAsync()
        {
            string cacheKey = $"parsed_{heroAbilitiesFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", heroAbilitiesFileName);
                return schemaParser.GetDotaHeroAbilities(vdf);
            }, TimeSpan.FromDays(1));
        }

        private async Task AddAbilityToViewModelIfNotNull(
            string abilityName,
            IReadOnlyCollection<HeroAbility> abilities,
            IList<HeroAbilityDetail> abilityDetailModels)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return;
            }

            // Don't return "generic" and "hidden" abilities (I don't know what they are)
            if (abilityName.ToLower().Contains("generic"))
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
        private async Task<HeroAbilityDetail> GetHeroAbilityDetailModel(string abilityName, IReadOnlyCollection<HeroAbility> abilities)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return null;
            }

            var ability = abilities.FirstOrDefault(x => x.Name == abilityName);
            var abilityDetailModel = new HeroAbilityDetail();

            // If the ability type is "ATTRIBUTE", treat it differently since there's less information about them. Also, ATTRIBUTES are now known as TALENTS as of the latest DOTA 2 7.00 patch.
            var abilityType = sharedService.GetHeroAbilityTypeKeyValue(ability.AbilityType);
            if (abilityType == DotaHeroAbilityType.TALENTS)
            {
                var completedTooltip = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}");

                // Talent tooltips are in a form like this: "+{s:value} Tree Dance Vision AoE"
                // We need to replace the "{s:value}" token with a value found in the "ability special" list
                var replaceableTokens = Regex.Matches(completedTooltip, @"{s:(\w+)}");
                foreach (Match token in replaceableTokens)
                {
                    var abilitySpecialValue = ability.AbilitySpecials.FirstOrDefault(x => x.Name == token.Groups[1].Value);
                    completedTooltip = completedTooltip.Replace(token.Value, abilitySpecialValue.Value);
                }

                abilityDetailModel = new HeroAbilityDetail()
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

                abilityDetailModel = new HeroAbilityDetail()
                {
                    Id = ability.Id,
                    Name = await abilityNameTask,
                    AvatarImagePath = $"{heroAbilityIconsBaseUrl}{ability.Name}_lg.png",
                    Description = await descriptionTask,
                    CastPoint = ability.AbilityCastPoint,
                    CastRange = ability.AbilityCastRange,
                    Cooldown = ability.AbilityCooldown,
                    Damage = ability.AbilityDamage,
                    DamageType = sharedService.GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                    Duration = ability.AbilityDuration,
                    ManaCost = ability.AbilityManaCost,
                    SpellImmunityType = sharedService.GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                    AbilitySpecials = await abilitySpecialTask,
                    Behaviors = ability.AbilityBehavior,
                    AbilityType = abilityType,
                    TargetFlags = ability.AbilityUnitTargetFlags,
                    TargetTypes = ability.AbilityUnitTargetType,
                    TeamTargets = ability.AbilityUnitTargetTeam,
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

        private async Task<IReadOnlyCollection<HeroAbilitySpecialDetail>> GetHeroAbilitySpecialDetailModels(
            string abilityName,
            string tooltipLocalizationPrefix,
            HeroAbility ability)
        {
            var abilitySpecialDetailModels = ability.AbilitySpecials
                .Select(async abilitySpecial => new HeroAbilitySpecialDetail()
                {
                    Name = await localizationService.GetAbilityLocalizationTextAsync($"{tooltipLocalizationPrefix}_{abilityName}_{abilitySpecial.Name}"),
                    RawName = abilitySpecial.Name,
                    Value = abilitySpecial.Value,
                    LinkedSpecialBonus = abilitySpecial.LinkedSpecialBonus
                });
            return (await Task.WhenAll(abilitySpecialDetailModels))
                .ToList()
                .AsReadOnly();
        }
    }
}