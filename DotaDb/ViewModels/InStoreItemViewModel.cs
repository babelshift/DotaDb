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
        public string PriceDate { get; set; }
        public double? Price { get; set; }
        public string StorePath { get; internal set; }
    }
}