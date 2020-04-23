using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Caching.Memory;
using SourceSchemaParser;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class LocalizationService
    {
        private readonly ISchemaParser schemaParser;
        private readonly BlobStorageService blobStorageService;
        private readonly CacheService cacheService;

        public LocalizationService(
            ISchemaParser schemaParser,
            BlobStorageService blobStorageService,
            CacheService cacheService)
        {
            this.schemaParser = schemaParser;
            this.blobStorageService = blobStorageService;
            this.cacheService = cacheService;
        }

        public async Task<string> GetAbilityLocalizationTextAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var localizationKeys = await GetAbilityLocalizationAsync();
            if (localizationKeys != null && localizationKeys.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<IReadOnlyDictionary<string, string>> GetAbilityLocalizationAsync()
        {
            string fileName = "abilities_english.vdf";
            string cacheKey = $"parsed_{fileName}";
            return await cacheService.GetOrSetAsync(fileName, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", fileName);
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
            if (localizationKeys != null && localizationKeys.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task<IReadOnlyDictionary<string, string>> GetPublicLocalizationAsync()
        {
            string fileName = "dota_english.vdf";
            string cacheKey = $"parsed_{fileName}";

            return await cacheService.GetOrSetAsync(fileName, async () =>
            {
                var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", fileName);
                return schemaParser.GetDotaPublicLocalizationKeys(vdf);
            }, TimeSpan.FromDays(1));
        }
    }
}