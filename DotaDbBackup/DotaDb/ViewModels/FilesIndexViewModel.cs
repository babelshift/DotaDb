using System;

namespace DotaDb.ViewModels
{
    public class FilesIndexViewModel
    {
        public DateTimeOffset? MainSchemaLastUpdated { get; set; }
        public DateTimeOffset? HeroesLastUpdated { get; set; }
        public DateTimeOffset? HeroAbilitiesLastUpdated { get; set; }
        public DateTimeOffset? ItemAbilitiesLastUpdated { get; set; }
        public DateTimeOffset? PanoramaLocalizationLastUpdated { get; set; }
        public DateTimeOffset? PublicLocalizationLastUpdated { get; set; }
        public DateTimeOffset? MainSchemaLocalizationLastUpdated { get; set; }
        public DateTimeOffset? InGameItemsLastUpdated { get; set; }
    }
}