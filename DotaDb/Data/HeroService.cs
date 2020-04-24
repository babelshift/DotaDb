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
        private readonly SharedService sharedService;

        private readonly string minimapIconsBaseUrl;

        // I have seen some of these keys having different casing such as "DOTA_Tooltip_ability" and "DOTA_Tooltip_Ability". Watch out.
        private const string tooltipLocalizationPrefix = "DOTA_Tooltip_ability";

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
            var abilityType = sharedService.GetHeroAbilityTypeKeyValue(ability.AbilityType);
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
                    DamageType = sharedService.GetDamageTypeKeyValue(ability.AbilityUnitDamageType),
                    Duration = ability.AbilityDuration.ToSlashSeparatedString(),
                    ManaCost = ability.AbilityManaCost.ToSlashSeparatedString(),
                    SpellImmunityType = sharedService.GetSpellImmunityTypeKeyValue(ability.SpellImmunityType),
                    Attributes = await abilitySpecialTask,
                    Behaviors = sharedService.GetJoinedBehaviors(ability.AbilityBehavior),
                    AbilityType = abilityType,
                    TargetFlags = sharedService.GetJoinedUnitTargetFlags(ability.AbilityUnitTargetFlags),
                    TargetTypes = sharedService.GetJoinedUnitTargetTypes(ability.AbilityUnitTargetType),
                    TeamTargets = sharedService.GetJoinedUnitTargetTeamTypes(ability.AbilityUnitTargetTeam),
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
    }
}