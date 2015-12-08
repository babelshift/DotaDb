using Newtonsoft.Json.Linq;
using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using SteamWebAPI2;
using SteamWebAPI2.Models.DOTA2;
using SteamWebAPI2.Interfaces;
using System.Configuration;
using System.Threading.Tasks;

namespace DotaDb.Models
{
    public sealed class InMemoryDb
    {
        private static volatile InMemoryDb instance;
        private static object sync = new object();

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
        private const string itemsWearableSchemaFileName = "items_wearable_schema.vdf";     // comes from web API GetSchemaURL >> download from result URL

        /// <summary>
        /// Contains localization text for wearable items
        /// </summary>
        private const string itemsWearableEnglishFileName = "items_wearable_english.vdf";   // comes from public dota 2 resource folder

        /// <summary>
        /// Contains localization text for in game tooltips
        /// </summary>
        private const string tooltipsEnglishFileName = "public_dota_english.vdf";            // comes from public dota 2 resource folder

        /// <summary>
        /// Contains localization text for in game UI
        /// </summary>
        private const string panoramaDotaEnglishFileName = "panorama_dota_english.vdf";     // comes from dota 2 pak

        private MemoryCache cache = MemoryCache.Default;
        private CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();

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

        public string AppDataPath { get { return AppDomain.CurrentDomain.GetData("DataDirectory").ToString(); } }

        private InMemoryDb()
        {
            cache.Remove(MemoryCacheKey.LocalizationKeys.ToString());
            cache.Remove(MemoryCacheKey.Heroes.ToString());
            cache.Remove(MemoryCacheKey.HeroAbilities.ToString());
            cache.Remove(MemoryCacheKey.Leagues.ToString());
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

            var localizationKeys = GetPublicLocalization();
            var heroes = GetHeroes();

            cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);

            cache.Add(MemoryCacheKey.LocalizationKeys.ToString(), localizationKeys, cacheItemPolicy);
            cache.Add(MemoryCacheKey.Heroes.ToString(), heroes, cacheItemPolicy);
        }

        public string GetLocalizationText(string key)
        {
            string value = String.Empty;
            var localizationKeys = GetPublicLocalization();
            if (localizationKeys != null && localizationKeys.TryGetValue(key, out value))
            {
                return value;
            }
            else
            {
                return String.Empty;
            }
        }

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

        public DotaHeroSchemaItem GetHeroKeyValue(string key)
        {
            var heroes = GetHeroes();
            return GetKeyValue(key, heroes);
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

        #region Cached Data
        
        public DotaSchema GetSchema()
        {
            return AddOrGetCachedValue(MemoryCacheKey.Schema, GetSchemaFromFile);
        }

        private DotaSchema GetSchemaFromFile()
        {
            string vdfPath = Path.Combine(AppDataPath, itemsWearableSchemaFileName);
            string vdf = System.IO.File.ReadAllText(vdfPath);
            var schema = SourceSchemaParser.SchemaFactory.GetDotaSchema(vdf);
            return schema;
        }

        public async Task<IReadOnlyCollection<LiveLeagueGame>> GetLiveLeagueGamesAsync()
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5)
            };
            return await AddOrGetCachedValue(MemoryCacheKey.LiveLeagueGames, GetLiveLeagueGamesFromWebAPI, cacheItemPolicy);
        }

        public async Task<IReadOnlyCollection<LiveLeagueGame>> GetLiveLeagueGamesFromWebAPI()
        {
            string steamWebApiKey = ConfigurationManager.AppSettings["steamWebApiKey"].ToString();
            var dota2Match = new DOTA2Match(steamWebApiKey);
            var liveLeagueGames = await dota2Match.GetLiveLeagueGamesAsync();
            return liveLeagueGames;
        }

        public IReadOnlyCollection<GameItem> GetGameItems()
        {
            return AddOrGetCachedValue(MemoryCacheKey.InGameItems, GetGameItemsFromSchema);
        }

        private IReadOnlyCollection<GameItem> GetGameItemsFromSchema()
        {
            string itemsJsonPath = Path.Combine(AppDataPath, itemsInGameFileName);
            string itemsJson = System.IO.File.ReadAllText(itemsJsonPath);
            JObject parsedItems = JObject.Parse(itemsJson);
            var itemsArray = parsedItems["result"]["items"];
            var items = itemsArray.ToObject<List<GameItem>>();
            return items.AsReadOnly();
        }

        public IReadOnlyDictionary<string, DotaHeroSchemaItem> GetHeroes()
        {
            return AddOrGetCachedValue(MemoryCacheKey.Heroes, GetHeroesFromSchema);
        }

        private IReadOnlyDictionary<string, DotaHeroSchemaItem> GetHeroesFromSchema()
        {
            string heroesVdfPath = Path.Combine(AppDataPath, heroesFileName);
            string vdf = System.IO.File.ReadAllText(heroesVdfPath);
            var heroes = SourceSchemaParser.SchemaFactory.GetDotaHeroes(vdf);
            return heroes
                .Where(x => !String.IsNullOrEmpty(x.Url))
                .ToDictionary(x => x.Url.ToLower(), x => x);
        }

        public IReadOnlyCollection<DotaAbilitySchemaItem> GetHeroAbilities()
        {
            return AddOrGetCachedValue(MemoryCacheKey.HeroAbilities, GetHeroAbilitiesFromSchema);
        }

        private IReadOnlyCollection<DotaAbilitySchemaItem> GetHeroAbilitiesFromSchema()
        {
            string heroesVdfPath = Path.Combine(AppDataPath, heroAbilitiesFileName);
            string vdf = System.IO.File.ReadAllText(heroesVdfPath);
            var abilities = SourceSchemaParser.SchemaFactory.GetDotaHeroAbilities(vdf);
            return abilities;
        }

        public IReadOnlyDictionary<string, string> GetPublicLocalization()
        {
            return AddOrGetCachedValue(MemoryCacheKey.LocalizationKeys, GetPublicLocalizationFromSchema);
        }

        private IReadOnlyDictionary<string, string> GetPublicLocalizationFromSchema()
        {
            string vdfPath = Path.Combine(AppDataPath, tooltipsEnglishFileName);
            string vdf = System.IO.File.ReadAllText(vdfPath);
            var result = SourceSchemaParser.SchemaFactory.GetDotaPublicLocalizationKeys(vdf);
            return result;
        }

        public IReadOnlyDictionary<int, DotaItemAbilitySchemaItem> GetItemAbilities()
        {
            return AddOrGetCachedValue(MemoryCacheKey.ItemAbilities, GetItemAbilitiesFromSchema);
        }

        private IReadOnlyDictionary<int, DotaItemAbilitySchemaItem> GetItemAbilitiesFromSchema()
        {
            string vdfPath = Path.Combine(AppDataPath, itemAbilitiesFileName);
            string vdf = System.IO.File.ReadAllText(vdfPath);
            var result = SourceSchemaParser.SchemaFactory.GetDotaItemAbilities(vdf);
            return new ReadOnlyDictionary<int, DotaItemAbilitySchemaItem>(result.ToDictionary(x => x.Id, x => x));
        }

        public IReadOnlyCollection<DotaLeague> GetLeagues()
        {
            return AddOrGetCachedValue(MemoryCacheKey.Leagues, GetLeaguesFromSchema);
        }

        private IReadOnlyCollection<DotaLeague> GetLeaguesFromSchema()
        {
            string schemaVdfPath = Path.Combine(AppDataPath, itemsWearableSchemaFileName);
            string localizationVdfPath = Path.Combine(AppDataPath, itemsWearableEnglishFileName);
            var leagues = SourceSchemaParser.SchemaFactory.GetDotaLeaguesFromFile(schemaVdfPath, localizationVdfPath);
            return leagues;
        }

        #endregion Cached Data

        private T AddOrGetCachedValue<T>(MemoryCacheKey key, Func<T> func, CacheItemPolicy overrideCacheItemPolicy = null)
        {
            object value = cache.Get(key.ToString());
            if (value != null)
            {
                return (T)value;
            }
            else
            {
                var newValue = func();

                if(overrideCacheItemPolicy != null)
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
    }
}