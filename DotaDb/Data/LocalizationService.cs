using Microsoft.Extensions.Configuration;
using SourceSchemaParser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class LocalizationService
    {
        private readonly IConfiguration configuration;
        private readonly ISchemaParser schemaParser;
        private readonly BlobStorageService blobStorageService;
        private readonly CacheService cacheService;

        private readonly string abilityLocalizationEnglishFileName;
        private readonly string gameLocalizationEnglishFileName;
        private readonly string cosmeticItemLocalizationEnglishFileName;


        public LocalizationService(
            IConfiguration configuration,
            ISchemaParser schemaParser,
            BlobStorageService blobStorageService,
            CacheService cacheService)
        {
            this.schemaParser = schemaParser;
            this.blobStorageService = blobStorageService;
            this.cacheService = cacheService;

            abilityLocalizationEnglishFileName = configuration["FileNames:AbilityLocalizationEnglish"];
            gameLocalizationEnglishFileName = configuration["FileNames:GameLocalizationEnglish"];
            cosmeticItemLocalizationEnglishFileName = configuration["FileNames:CosmeticItemLocalizationEnglish"];
        }

        public async Task<string> GetAbilityLocalizationTextAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var localizationKeys = await GetAbilityLocalizationAsync();
            return localizationKeys != null && localizationKeys.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private async Task<IReadOnlyDictionary<string, string>> GetAbilityLocalizationAsync()
        {
            string cacheKey = $"parsed_{abilityLocalizationEnglishFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", abilityLocalizationEnglishFileName);
                return schemaParser.GetDotaPublicLocalizationKeys(vdf);
            }, TimeSpan.FromDays(1));
        }

        public async Task<string> GetLocalizationTextAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var localizationKeys = await GetPublicLocalizationAsync();
            return localizationKeys != null && localizationKeys.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private async Task<IReadOnlyDictionary<string, string>> GetPublicLocalizationAsync()
        {
            string cacheKey = $"parsed_{gameLocalizationEnglishFileName}";
            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", gameLocalizationEnglishFileName);
                return schemaParser.GetDotaPublicLocalizationKeys(vdf);
            }, TimeSpan.FromDays(1));
        }

        public async Task<string> GetCosmeticItemLocalizationTextAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var localizationKeys = await GetCosmeticItemLocalizationAsync();
            return localizationKeys != null && localizationKeys.TryGetValue(key, out string value) ? value : string.Empty;
        }

        private async Task<IReadOnlyDictionary<string, string>> GetCosmeticItemLocalizationAsync()
        {
            string cacheKey = $"parsed_{cosmeticItemLocalizationEnglishFileName}";

            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", cosmeticItemLocalizationEnglishFileName);
                return schemaParser.GetDotaPublicLocalizationKeys(vdf);
            }, TimeSpan.FromDays(1));
        }
    }
}