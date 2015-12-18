using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class InStoreItemViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string PriceBucket { get; set; }
        public string PriceClass { get; set; }
        public string PriceCategoryTags { get; set; }
        public DateTime? PriceDate { get; set; }
        public decimal? Price { get; set; }
        public string StorePath { get; internal set; }
        public string Rarity { get; set; }
        public string RarityColor { get; set; }
        public string Quality { get; set; }
        public string QualityColor { get; set; }
        public IReadOnlyCollection<InStoreItemUsedByHeroViewModel> UsedBy { get; set; }
        public IReadOnlyCollection<string> BundledItems { get; set; }
    }
}