using System.Collections.Generic;

namespace DotaDb.Models
{
    public class InStoreViewModel
    {
        public InStoreViewModel()
        {
            Rarities = new List<InStoreItemRarityViewModel>();
            Qualities = new List<InStoreItemQualityViewModel>();
            Prefabs = new List<InStoreItemPrefabViewModel>();
            Items = new List<InStoreItemViewModel>();
        }

        public IReadOnlyCollection<InStoreItemRarityViewModel> Rarities { get; set; }
        public IReadOnlyCollection<InStoreItemQualityViewModel> Qualities { get; set; }
        public IReadOnlyCollection<InStoreItemPrefabViewModel> Prefabs { get; set; }
        public IEnumerable<InStoreItemViewModel> Items { get; set; }
    }
}