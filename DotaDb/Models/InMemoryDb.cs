using DotaDb.Utilities;
using EasyAzureStorage;
using HtmlAgilityPack;
using SourceSchemaParser.Dota2;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Models.DOTA2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace DotaDb.Models
{
    public sealed class InMemoryDb
    {
        private const string storageContainerName = "schemas";

        private static volatile InMemoryDb instance;
        private static object sync = new object();
        private MemoryCache cache = MemoryCache.Default;
        private CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
        private AzureStorage storage;

        #region Data Source File Names

        /// <summary>
        /// Contains information about all hero abilities and descriptions
        /// </summary>
        private const string heroAbilitiesFileName = "hero_abilities.vdf";  // comes from npc_abilities.txt in dota 2 pak

        /// <summary>
        /// Contains information about all item abilities and descriptions
        /// </summary>
        private const string itemAbilitiesFileName = "item_abilities.vdf";  // comes from items.txt in dota 2 pak

        /// <summary>
        /// Contains a list of all heroes (active and inactive) with some details about them
        /// </summary>
        private const string heroesFileName = "heroes.vdf";                 // comes from npc_heroes.txt in dota 2 pak

        /// <summary>
        /// Contains a list of all in game items that can be purchased by heroes in a regular game (blink dagger, boots of speed, etc)
        /// </summary>
        private const string itemsInGameFileName = "items_ingame.json";     // comes from web API GetGameItems

        /// <summary>
        /// Contains a list and details of all items that can be purchased from the game shop (wearables, cosmetics, announcers, etc)
        /// </summary>
        private const string mainSchemaFileName = "items_wearable_schema.vdf";     // comes from web API GetSchemaURL >> download from result URL

        /// <summary>
        /// Contains localization text for wearable items
        /// </summary>
        private const string mainSchemaEnglishFileName = "items_wearable_english.vdf";   // comes from public dota 2 resource folder

        /// <summary>
        /// Contains localization text for in game tooltips
        /// </summary>
        private const string tooltipsEnglishFileName = "public_dota_english.vdf";            // comes from public dota 2 resource folder

        /// <summary>
        /// Contains localization text for in game UI
        /// </summary>
        private const string panoramaDotaEnglishFileName = "panorama_dota_english.vdf";     // comes from dota 2 pak

        #endregion Data Source File Names

        #region Enum Type Lookup Members

        public IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType> abilityBehaviorTypes;
        public IReadOnlyDictionary<string, DotaHeroAbilityType> abilityTypes;
        public IReadOnlyDictionary<string, DotaAttackType> attackTypes;
        public IReadOnlyDictionary<string, DotaTeamType> teamTypes;
        public IReadOnlyDictionary<string, DotaDamageType> damageTypes;
        public IReadOnlyDictionary<string, DotaSpellImmunityType> spellImmunityTypes;
        public IReadOnlyDictionary<string, DotaUnitTargetFlag> unitTargetFlags;
        public IReadOnlyDictionary<string, DotaUnitTargetTeamType> unitTargetTeamTypes;
        public IReadOnlyDictionary<string, DotaUnitTargetType> unitTargetTypes;
        public IReadOnlyDictionary<string, DotaHeroPrimaryAttributeType> attributeTypes;
        public IReadOnlyDictionary<string, DotaItemDeclarationType> itemDeclarationTypes;
        public IReadOnlyDictionary<string, DotaItemShareabilityType> itemShareabilityTypes;
        public IReadOnlyDictionary<string, DotaItemDisassembleType> itemDisassembleTypes;

        #endregion Enum Type Lookup Members

        public static InMemoryDb Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (sync)
                    {
                        if (instance == null)
                        {
                            instance = new InMemoryDb();
                        }
                    }
                }

                return instance;
            }
        }

        private InMemoryDb()
        {
            cache.Remove(MemoryCacheKey.LocalizationKeys.ToString());
            cache.Remove(MemoryCacheKey.Heroes.ToString());
            cache.Remove(MemoryCacheKey.HeroAbilities.ToString());
            cache.Remove(MemoryCacheKey.LeagueTickets.ToString());
            cache.Remove(MemoryCacheKey.ItemAbilities.ToString());

            abilityBehaviorTypes = GetAbilityBehaviorTypes();
            attackTypes = GetAttackTypes();
            teamTypes = GetTeamTypes();
            abilityTypes = GetHeroAbilityTypes();
            spellImmunityTypes = GetSpellImmunityTypes();
            damageTypes = GetDamageTypes();
            unitTargetFlags = GetUnitTargetFlags();
            unitTargetTeamTypes = GetUnitTargetTeamTypes();
            unitTargetTypes = GetUnitTargetTypes();
            attributeTypes = GetAttributeTypes();
            itemDeclarationTypes = GetItemDeclarationTypes();
            itemShareabilityTypes = GetItemShareabilityTypes();
            itemDisassembleTypes = GetItemDisassembleTypes();

            cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

            string schemaStorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"].ToString();
            storage = new AzureStorage(schemaStorageConnectionString);
        }

        #region Enum Type Lookup Methods

        public IReadOnlyDictionary<string, DotaItemDisassembleType> GetItemDisassembleTypes()
        {
            Dictionary<string, DotaItemDisassembleType> temp = new Dictionary<string, DotaItemDisassembleType>();

            temp.Add(DotaItemDisassembleType.ALWAYS.Key, DotaItemDisassembleType.ALWAYS);
            temp.Add(DotaItemDisassembleType.NEVER.Key, DotaItemDisassembleType.NEVER);

            return new ReadOnlyDictionary<string, DotaItemDisassembleType>(temp);
        }

        public IReadOnlyDictionary<string, DotaItemShareabilityType> GetItemShareabilityTypes()
        {
            Dictionary<string, DotaItemShareabilityType> temp = new Dictionary<string, DotaItemShareabilityType>();

            temp.Add(DotaItemShareabilityType.FULLY_SHAREABLE.Key, DotaItemShareabilityType.FULLY_SHAREABLE);
            temp.Add(DotaItemShareabilityType.FULLY_SHAREABLE_STACKING.Key, DotaItemShareabilityType.FULLY_SHAREABLE_STACKING);
            temp.Add(DotaItemShareabilityType.PARTIALLY_SHAREABLE.Key, DotaItemShareabilityType.PARTIALLY_SHAREABLE);

            return new ReadOnlyDictionary<string, DotaItemShareabilityType>(temp);
        }

        public IReadOnlyDictionary<string, DotaItemDeclarationType> GetItemDeclarationTypes()
        {
            Dictionary<string, DotaItemDeclarationType> temp = new Dictionary<string, DotaItemDeclarationType>();

            temp.Add(DotaItemDeclarationType.PURCHASES_IN_SPEECH.Key, DotaItemDeclarationType.PURCHASES_IN_SPEECH);
            temp.Add(DotaItemDeclarationType.PURCHASES_TO_SPECTATORS.Key, DotaItemDeclarationType.PURCHASES_TO_SPECTATORS);
            temp.Add(DotaItemDeclarationType.PURCHASES_TO_TEAMMATES.Key, DotaItemDeclarationType.PURCHASES_TO_TEAMMATES);

            return new ReadOnlyDictionary<string, DotaItemDeclarationType>(temp);
        }

        public IReadOnlyDictionary<string, DotaHeroAbilityBehaviorType> GetAbilityBehaviorTypes()
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

        public IReadOnlyDictionary<string, DotaHeroAbilityType> GetHeroAbilityTypes()
        {
            Dictionary<string, DotaHeroAbilityType> temp = new Dictionary<string, DotaHeroAbilityType>();

            temp.Add(DotaHeroAbilityType.BASIC.Key, DotaHeroAbilityType.BASIC);
            temp.Add(DotaHeroAbilityType.ULTIMATE.Key, DotaHeroAbilityType.ULTIMATE);
            temp.Add(DotaHeroAbilityType.ATTRIBUTES.Key, DotaHeroAbilityType.ATTRIBUTES);

            return new ReadOnlyDictionary<string, DotaHeroAbilityType>(temp);
        }

        public IReadOnlyDictionary<string, DotaAttackType> GetAttackTypes()
        {
            Dictionary<string, DotaAttackType> temp = new Dictionary<string, DotaAttackType>();

            temp.Add(DotaAttackType.MELEE.Key, DotaAttackType.MELEE);
            temp.Add(DotaAttackType.RANGED.Key, DotaAttackType.RANGED);

            return new ReadOnlyDictionary<string, DotaAttackType>(temp);
        }

        public IReadOnlyDictionary<string, DotaHeroPrimaryAttributeType> GetPrimaryAttributeTypes()
        {
            Dictionary<string, DotaHeroPrimaryAttributeType> temp = new Dictionary<string, DotaHeroPrimaryAttributeType>();

            temp.Add(DotaHeroPrimaryAttributeType.AGILITY.Key, DotaHeroPrimaryAttributeType.AGILITY);
            temp.Add(DotaHeroPrimaryAttributeType.INTELLECT.Key, DotaHeroPrimaryAttributeType.INTELLECT);
            temp.Add(DotaHeroPrimaryAttributeType.STRENGTH.Key, DotaHeroPrimaryAttributeType.STRENGTH);

            return new ReadOnlyDictionary<string, DotaHeroPrimaryAttributeType>(temp);
        }

        public IReadOnlyDictionary<string, DotaTeamType> GetTeamTypes()
        {
            Dictionary<string, DotaTeamType> temp = new Dictionary<string, DotaTeamType>();

            temp.Add(DotaTeamType.BAD.Key, DotaTeamType.BAD);
            temp.Add(DotaTeamType.GOOD.Key, DotaTeamType.GOOD);

            return new ReadOnlyDictionary<string, DotaTeamType>(temp);
        }

        public IReadOnlyDictionary<string, DotaDamageType> GetDamageTypes()
        {
            Dictionary<string, DotaDamageType> temp = new Dictionary<string, DotaDamageType>();

            temp.Add(DotaDamageType.MAGICAL.Key, DotaDamageType.MAGICAL);
            temp.Add(DotaDamageType.PHYSICAL.Key, DotaDamageType.PHYSICAL);
            temp.Add(DotaDamageType.PURE.Key, DotaDamageType.PURE);

            return new ReadOnlyDictionary<string, DotaDamageType>(temp);
        }

        public IReadOnlyDictionary<string, DotaSpellImmunityType> GetSpellImmunityTypes()
        {
            Dictionary<string, DotaSpellImmunityType> temp = new Dictionary<string, DotaSpellImmunityType>();

            temp.Add(DotaSpellImmunityType.ALLIES_NO.Key, DotaSpellImmunityType.ALLIES_NO);
            temp.Add(DotaSpellImmunityType.ALLIES_YES.Key, DotaSpellImmunityType.ALLIES_YES);
            temp.Add(DotaSpellImmunityType.ENEMIES_NO.Key, DotaSpellImmunityType.ENEMIES_NO);
            temp.Add(DotaSpellImmunityType.ENEMIES_YES.Key, DotaSpellImmunityType.ENEMIES_YES);

            return new ReadOnlyDictionary<string, DotaSpellImmunityType>(temp);
        }

        public IReadOnlyDictionary<string, DotaUnitTargetFlag> GetUnitTargetFlags()
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

        public IReadOnlyDictionary<string, DotaUnitTargetTeamType> GetUnitTargetTeamTypes()
        {
            Dictionary<string, DotaUnitTargetTeamType> temp = new Dictionary<string, DotaUnitTargetTeamType>();

            temp.Add(DotaUnitTargetTeamType.BOTH.Key, DotaUnitTargetTeamType.BOTH);
            temp.Add(DotaUnitTargetTeamType.CUSTOM.Key, DotaUnitTargetTeamType.CUSTOM);
            temp.Add(DotaUnitTargetTeamType.ENEMY.Key, DotaUnitTargetTeamType.ENEMY);
            temp.Add(DotaUnitTargetTeamType.FRIENDLY.Key, DotaUnitTargetTeamType.FRIENDLY);

            return new ReadOnlyDictionary<string, DotaUnitTargetTeamType>(temp);
        }

        public IReadOnlyDictionary<string, DotaUnitTargetType> GetUnitTargetTypes()
        {
            Dictionary<string, DotaUnitTargetType> temp = new Dictionary<string, DotaUnitTargetType>();

            temp.Add(DotaUnitTargetType.BASIC.Key, DotaUnitTargetType.BASIC);
            temp.Add(DotaUnitTargetType.BUILDING.Key, DotaUnitTargetType.BUILDING);
            temp.Add(DotaUnitTargetType.CUSTOM.Key, DotaUnitTargetType.CUSTOM);
            temp.Add(DotaUnitTargetType.HERO.Key, DotaUnitTargetType.HERO);

            return new ReadOnlyDictionary<string, DotaUnitTargetType>(temp);
        }

        public IReadOnlyDictionary<string, DotaHeroPrimaryAttributeType> GetAttributeTypes()
        {
            Dictionary<string, DotaHeroPrimaryAttributeType> temp = new Dictionary<string, DotaHeroPrimaryAttributeType>();

            temp.Add(DotaHeroPrimaryAttributeType.AGILITY.Key, DotaHeroPrimaryAttributeType.AGILITY);
            temp.Add(DotaHeroPrimaryAttributeType.INTELLECT.Key, DotaHeroPrimaryAttributeType.INTELLECT);
            temp.Add(DotaHeroPrimaryAttributeType.STRENGTH.Key, DotaHeroPrimaryAttributeType.STRENGTH);

            return new ReadOnlyDictionary<string, DotaHeroPrimaryAttributeType>(temp);
        }

        public DotaHeroPrimaryAttributeType GetHeroPrimaryAttributeTypeKeyValue(string key)
        {
            return GetKeyValue(key, attributeTypes);
        }

        public DotaAttackType GetAttackTypeKeyValue(string key)
        {
            return GetKeyValue(key, attackTypes);
        }

        public DotaTeamType GetTeamTypeKeyValue(string key)
        {
            return GetKeyValue(key, teamTypes);
        }

        public DotaHeroAbilityType GetHeroAbilityTypeKeyValue(string key)
        {
            return GetKeyValue(key, abilityTypes);
        }

        public DotaSpellImmunityType GetSpellImmunityTypeKeyValue(string key)
        {
            return GetKeyValue(key, spellImmunityTypes);
        }

        public DotaDamageType GetDamageTypeKeyValue(string key)
        {
            return GetKeyValue(key, damageTypes);
        }

        public async Task<DotaHeroSchemaItem> GetHeroKeyValueAsync(int key)
        {
            var heroes = await GetHeroesAsync();
            return GetKeyValue(key, heroes);
        }

        #endregion Enum Type Lookup Methods

        public async Task<string> GetTeamLogoUrlAsync(long ugcId)
        {
            string steamWebApiKey = ConfigurationManager.AppSettings["steamWebApiKey"].ToString();
            var steamRemoteStorage = new SteamRemoteStorage(steamWebApiKey);
            var ugcFileDetails = await steamRemoteStorage.GetUGCFileDetailsAsync(ugcId, 570);
            if (ugcFileDetails != null)
            {
                return ugcFileDetails.URL;
            }
            else
            {
                return String.Empty;
            }
        }

        public async Task<string> GetLocalizationTextAsync(string key)
        {
            string value = String.Empty;
            var localizationKeys = await GetPublicLocalizationAsync();
            if (localizationKeys != null && localizationKeys.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return String.Empty;
            }
        }

        public async Task<DotaItemBuildSchemaItem> GetItemBuildAsync(string heroName)
        {
            string fileName = String.Format("default_{0}.txt", heroName.Replace("npc_dota_hero_", ""));
            var fileLines = await GetFileLinesFromStorageAsync(fileName);
            string[] vdf = fileLines.ToArray();
            var itemBuild = SourceSchemaParser.SchemaFactory.GetDotaItemBuild(vdf);
            return itemBuild;
        }

        private async Task<IList<string>> GetFileLinesFromStorageAsync(string fileName)
        {
            var blob = await storage.DownloadBlobAsync(storageContainerName, fileName);

            List<string> contents = new List<string>();
            using (var ms = new MemoryStream(blob))
            {
                using (var sr = new StreamReader(ms))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = await sr.ReadLineAsync();
                        contents.Add(line);
                    }
                }
            }

            return contents;
        }

        #region Cached Data

        public async Task<PlayerCountModel> GetPlayerCountsAsync()
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(15)
            };
            return await AddOrGetCachedValue(MemoryCacheKey.PlayerCounts, GetPlayerCountsFromScrapingAsync, cacheItemPolicy);
        }

        private async Task<PlayerCountModel> GetPlayerCountsFromScrapingAsync()
        {
            HttpClient client = new HttpClient();
            var steamChartsHtml = await client.GetStringAsync("http://steamcharts.com/app/570");
            HtmlDocument doc = new HtmlDocument();

            doc.LoadHtml(steamChartsHtml);
            var appStats = doc.DocumentNode
                .Descendants("div")
                .Where(x => x.GetAttributeValue("class", null) == "app-stat");

            PlayerCountModel model = new PlayerCountModel();

            for (int i = 0; i < appStats.Count(); i++)
            {
                var num = appStats.ElementAt(i)
                    .Descendants("span")
                    .First(x => x.GetAttributeValue("class", null) == "num");
                int value = 0;
                bool success = int.TryParse(num.InnerText, out value);

                if (i == 0)
                {
                    model.InGamePlayerCount = value;
                }
                else if (i == 1)
                {
                    model.DailyPeakPlayerCount = value;
                }
                else if (i == 2)
                {
                    model.AllTimePeakPlayerCount = value;
                }
            }

            return model;
        }

        public async Task<DotaSchema> GetSchemaAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.Schema, GetSchemaFromFileAsync);
        }

        private async Task<DotaSchema> GetSchemaFromFileAsync()
        {
            string[] vdf = (await GetFileLinesFromStorageAsync(mainSchemaFileName)).ToArray();
            var schema = SourceSchemaParser.SchemaFactory.GetDotaSchema(vdf);
            return schema;
        }

        public async Task<int> GetLiveLeagueGameCountAsync()
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5)
            };
            var liveLeagueGames = await AddOrGetCachedValue(MemoryCacheKey.LiveLeagueGames, GetLiveLeagueGamesFromWebAPIAsync, cacheItemPolicy);
            return liveLeagueGames.Count;
        }

        public async Task<IReadOnlyCollection<LiveLeagueGameModel>> GetLiveLeagueGamesAsync(int takeAmount)
        {
            #region Get/Add From/To Cache

            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(20)
            };
            var liveLeagueGames = await AddOrGetCachedValue(MemoryCacheKey.LiveLeagueGames, GetLiveLeagueGamesFromWebAPIAsync, cacheItemPolicy);

            #endregion Get/Add From/To Cache

            var filteredLiveLeagueGames = liveLeagueGames
                .OrderByDescending(x => x.Spectators)
                .Take(takeAmount);

            List<LiveLeagueGameModel> liveLeagueGameModels = new List<LiveLeagueGameModel>();

            foreach (var liveLeagueGame in filteredLiveLeagueGames)
            {
                LiveLeagueGameModel liveLeagueGameModel = new LiveLeagueGameModel()
                {
                    BestOf = GetBestOfCountFromSeriesType(liveLeagueGame.SeriesType),
                    DireKillCount = (liveLeagueGame.Scoreboard != null && liveLeagueGame.Scoreboard.Dire != null) ? liveLeagueGame.Scoreboard.Dire.Score : 0,
                    RadiantKillCount = (liveLeagueGame.Scoreboard != null && liveLeagueGame.Scoreboard.Radiant != null) ? liveLeagueGame.Scoreboard.Radiant.Score : 0,
                    GameNumber = liveLeagueGame.RadiantSeriesWins + liveLeagueGame.DireSeriesWins + 1,
                    ElapsedTime = liveLeagueGame.Scoreboard != null ? GetElapsedTime(liveLeagueGame.Scoreboard.Duration) : "Unknown",
                    DireTeamName = liveLeagueGame.DireTeam != null ? liveLeagueGame.DireTeam.TeamName : "Dire",
                    RadiantTeamName = liveLeagueGame.RadiantTeam != null ? liveLeagueGame.RadiantTeam.TeamName : "Radiant",
                    SeriesStatus = String.Format("{0} - {1}", liveLeagueGame.RadiantSeriesWins, liveLeagueGame.DireSeriesWins),
                    SpectatorCount = liveLeagueGame.Spectators
                };

                #region Fill in Player Details

                liveLeagueGameModel.Players = liveLeagueGame.Players
                    .Select(x => new LiveLeagueGamePlayerModel()
                    {
                        AccountId = x.AccountId,
                        HeroId = x.HeroId,
                        Name = x.Name,
                        Team = x.Team
                    })
                    .ToList()
                    .AsReadOnly();

                Dictionary<int, LiveLeagueGamePlayerDetail> radiantPlayerDetail = null;
                Dictionary<int, LiveLeagueGamePlayerDetail> direPlayerDetail = null;

                // if the game hasn't started yet, the scoreboard won't exist
                if (liveLeagueGame.Scoreboard != null)
                {
                    radiantPlayerDetail = liveLeagueGame.Scoreboard.Radiant.Players.ToDictionary(x => x.AccountId, x => x);
                    direPlayerDetail = liveLeagueGame.Scoreboard.Dire.Players.ToDictionary(x => x.AccountId, x => x);
                }

                foreach (var player in liveLeagueGameModel.Players)
                {
                    // skip over spectators/observers/commentators
                    if (player.Team != 0 && player.Team != 1)
                    {
                        continue;
                    }

                    var hero = await GetHeroKeyValueAsync(player.HeroId);

                    player.HeroName = await GetLocalizationTextAsync(hero.Name);
                    player.HeroAvatarImageFileName = GetHeroAvatarFileName(hero.Name);

                    LiveLeagueGamePlayerDetail playerDetail = GetPlayerDetail(player.Team, player.AccountId, radiantPlayerDetail, direPlayerDetail);

                    // if the player hasn't picked a hero yet, details won't exist
                    if (playerDetail != null)
                    {
                        player.KillCount = playerDetail.Kills;
                        player.DeathCount = playerDetail.Deaths;
                        player.AssistCount = playerDetail.Assists;
                        player.PositionX = playerDetail.PositionX;
                        player.PositionY = playerDetail.PositionY;
                    }

                    player.HeroUrl = hero.Url;
                }

                #endregion Fill in Player Details

                #region Fill in League/Team Details

                // look up whatever league this game belongs to in the league listing to get more details about it
                var leagues = await GetLeaguesAsync();
                League league = null;
                bool success = leagues.TryGetValue(liveLeagueGame.LeagueId, out league);
                if (success)
                {
                    // look up this league's in game ticket asset for the logo and localized league name
                    var leagueTickets = await GetLeagueTicketsAsync();
                    DotaLeague leagueTicket = null;
                    success = leagueTickets.TryGetValue(league.ItemDef.ToString(), out leagueTicket);
                    if (success)
                    {
                        liveLeagueGameModel.LeagueLogoPath = leagueTicket.GetLogoFilePath();
                        liveLeagueGameModel.LeagueName = leagueTicket.NameLocalized;
                    }
                }

                // this property will only exist if the team is a tournament/league coordinated team
                if (liveLeagueGame.RadiantTeam != null)
                {
                    //liveLeagueGameModel.RadiantTeamLogo = await GetTeamLogoUrlAsync(liveLeagueGame.RadiantTeam.TeamLogo);
                }

                // this property will only exist if the team is a tournament/league coordinated team
                if (liveLeagueGame.DireTeam != null)
                {
                    //liveLeagueGameModel.DireTeamLogo = await GetTeamLogoUrlAsync(liveLeagueGame.DireTeam.TeamLogo);
                }

                #endregion Fill in League/Team Details

                liveLeagueGameModels.Add(liveLeagueGameModel);
            }

            return liveLeagueGameModels.AsReadOnly();
        }

        private static string GetHeroAvatarFileName(string heroName)
        {
            // if the player hasn't picked a hero yet, the 'base' hero will be shown, which basically is 'unknown'
            if (heroName == "npc_dota_hero_base")
            {
                return String.Empty;
            }
            else
            {
                return heroName.Replace("npc_dota_hero_", "");
            }
        }

        private static LiveLeagueGamePlayerDetail GetPlayerDetail(int playerTeam, int playerAccountId, Dictionary<int, LiveLeagueGamePlayerDetail> radiantPlayerDetail, Dictionary<int, LiveLeagueGamePlayerDetail> direPlayerDetail)
        {
            // team 0 is radiant, if the player hasn't picked a hero yet, details won't exist
            if (playerTeam == 0)
            {
                if (radiantPlayerDetail != null)
                {
                    return radiantPlayerDetail[playerAccountId];
                }
            }
            // team 1 is dire
            else if (playerTeam == 1)
            {
                if (direPlayerDetail != null)
                {
                    return direPlayerDetail[playerAccountId];
                }
            }

            return null;
        }

        private async Task<IReadOnlyCollection<LiveLeagueGame>> GetLiveLeagueGamesFromWebAPIAsync()
        {
            string steamWebApiKey = ConfigurationManager.AppSettings["steamWebApiKey"].ToString();
            var dota2Match = new DOTA2Match(steamWebApiKey);
            var liveLeagueGames = await dota2Match.GetLiveLeagueGamesAsync();
            return liveLeagueGames;
        }

        public async Task<IReadOnlyCollection<GameItem>> GetGameItemsAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.InGameItems, GetGameItemsFromWebAPIAsync);
        }

        private async Task<IReadOnlyCollection<GameItem>> GetGameItemsFromWebAPIAsync()
        {
            string steamWebApiKey = ConfigurationManager.AppSettings["steamWebApiKey"].ToString();
            var dota2Econ = new DOTA2Econ(steamWebApiKey);
            var gameItems = await dota2Econ.GetGameItemsAsync();
            return gameItems;
        }

        public async Task<IReadOnlyDictionary<int, DotaHeroSchemaItem>> GetHeroesAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.Heroes, GetHeroesFromSchemaAsync);
        }

        private async Task<IReadOnlyDictionary<int, DotaHeroSchemaItem>> GetHeroesFromSchemaAsync()
        {
            string[] vdf = (await GetFileLinesFromStorageAsync(heroesFileName)).ToArray();
            var heroes = SourceSchemaParser.SchemaFactory.GetDotaHeroes(vdf);
            return heroes.ToDictionary(x => x.HeroId, x => x);
        }

        public async Task<IReadOnlyCollection<DotaAbilitySchemaItem>> GetHeroAbilitiesAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.HeroAbilities, GetHeroAbilitiesFromSchemaAsync);
        }

        private async Task<IReadOnlyCollection<DotaAbilitySchemaItem>> GetHeroAbilitiesFromSchemaAsync()
        {
            string[] vdf = (await GetFileLinesFromStorageAsync(heroAbilitiesFileName)).ToArray();
            var abilities = SourceSchemaParser.SchemaFactory.GetDotaHeroAbilities(vdf);
            return abilities;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetPublicLocalizationAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.LocalizationKeys, GetPublicLocalizationFromSchemaAsync);
        }

        private async Task<IReadOnlyDictionary<string, string>> GetPublicLocalizationFromSchemaAsync()
        {
            string[] vdf = (await GetFileLinesFromStorageAsync(tooltipsEnglishFileName)).ToArray();
            var result = SourceSchemaParser.SchemaFactory.GetDotaPublicLocalizationKeys(vdf);
            return result;
        }

        public async Task<IReadOnlyDictionary<int, DotaItemAbilitySchemaItem>> GetItemAbilitiesAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.ItemAbilities, GetItemAbilitiesFromSchemaAsync);
        }

        private async Task<IReadOnlyDictionary<int, DotaItemAbilitySchemaItem>> GetItemAbilitiesFromSchemaAsync()
        {
            string[] vdf = (await GetFileLinesFromStorageAsync(itemAbilitiesFileName)).ToArray();
            var result = SourceSchemaParser.SchemaFactory.GetDotaItemAbilities(vdf);
            return new ReadOnlyDictionary<int, DotaItemAbilitySchemaItem>(result.ToDictionary(x => x.Id, x => x));
        }

        public async Task<IReadOnlyDictionary<int, League>> GetLeaguesAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.Leagues, GetLeaguesFromWebAPIAsync);
        }

        public async Task<IReadOnlyDictionary<string, DotaLeague>> GetLeagueTicketsAsync()
        {
            return await AddOrGetCachedValue(MemoryCacheKey.LeagueTickets, GetLeagueTicketsFromSchemaAsync);
        }

        private async Task<IReadOnlyDictionary<int, League>> GetLeaguesFromWebAPIAsync()
        {
            string steamWebApiKey = ConfigurationManager.AppSettings["steamWebApiKey"].ToString();
            var dota2Match = new DOTA2Match(steamWebApiKey);
            var leagueList = await dota2Match.GetLeagueListingAsync();
            var distinctLeagues = leagueList.Leagues
                .GroupBy(x => x.LeagueId)
                .Select(x => x.First());
            return new ReadOnlyDictionary<int, League>(distinctLeagues.ToDictionary(x => x.LeagueId, x => x));
        }

        private async Task<IReadOnlyDictionary<string, DotaLeague>> GetLeagueTicketsFromSchemaAsync()
        {
            string[] schemaVdf = (await GetFileLinesFromStorageAsync(mainSchemaFileName)).ToArray();
            string[] localizationVdf = (await GetFileLinesFromStorageAsync(mainSchemaEnglishFileName)).ToArray();
            var leagues = SourceSchemaParser.SchemaFactory.GetDotaLeaguesFromText(schemaVdf, localizationVdf);
            return new ReadOnlyDictionary<string, DotaLeague>(leagues.ToDictionary(x => x.ItemDef, x => x));
        }

        private async Task<T> AddOrGetCachedValue<T>(MemoryCacheKey key, Func<Task<T>> func, CacheItemPolicy overrideCacheItemPolicy = null)
        {
            object value = cache.Get(key.ToString());
            if (value != null)
            {
                return (T)value;
            }
            else
            {
                var newValue = await func();

                if (overrideCacheItemPolicy != null)
                {
                    cache.Add(key.ToString(), newValue, overrideCacheItemPolicy);
                }
                else
                {
                    cache.Add(key.ToString(), newValue, cacheItemPolicy);
                }

                return newValue;
            }
        }

        #endregion Cached Data

        #region Utility Methods

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

        public string GetJoinedItemDisassembleTypes(string value)
        {
            return GetJoinedValues(value, itemDisassembleTypes);
        }

        public string GetJoinedItemShareabilityTypes(string value)
        {
            return GetJoinedValues(value, itemShareabilityTypes);
        }

        public string GetJoinedItemDeclarationTypes(string value)
        {
            return GetJoinedValues(value, itemDeclarationTypes);
        }

        public string GetJoinedUnitTargetFlags(string value)
        {
            return GetJoinedValues(value, unitTargetFlags);
        }

        public string GetJoinedUnitTargetTypes(string value)
        {
            return GetJoinedValues(value, unitTargetTypes);
        }

        public string GetJoinedUnitTargetTeamTypes(string value)
        {
            return GetJoinedValues(value, unitTargetTeamTypes);
        }

        public string GetJoinedBehaviors(string value)
        {
            return GetJoinedValues(value, abilityBehaviorTypes);
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

        private static int GetBestOfCountFromSeriesType(int seriesType)
        {
            if (seriesType == 0)
            {
                return 1;
            }
            else if (seriesType == 1)
            {
                return 3;
            }
            else if (seriesType == 2)
            {
                return 5;
            }
            else
            {
                return 0;
            }
        }

        private static string GetElapsedTime(double seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return String.Format("{0}m {1}s", timeSpan.Minutes, timeSpan.Seconds);
        }

        #endregion Utility Methods
    }
}