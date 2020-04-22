using SourceSchemaParser;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotaDb.Data
{
    public class LocalizationService
    {
        private readonly ISchemaParser schemaParser;
        private readonly BlobStorageService blobStorageService;

        public LocalizationService(
            ISchemaParser schemaParser,
            BlobStorageService blobStorageService)
        {
            this.schemaParser = schemaParser;
            this.blobStorageService = blobStorageService;
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
            var vdf = await blobStorageService.GetFileFromStorageAsync("schemas", "dota_english.vdf");
            var result = schemaParser.GetDotaPublicLocalizationKeys(vdf);
            return result;
        }
    }
}