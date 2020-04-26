using System.Collections.Generic;

namespace DotaDb.Models
{
    public class InStoreViewModel
    {
        public InStoreViewModel()
        {
            Prefabs = new List<InStoreItemPrefabViewModel>();
            Items = new List<InStoreItemViewModel>();
        }

        public IReadOnlyCollection<InStoreItemPrefabViewModel> Prefabs { get; set; }
        public IEnumerable<InStoreItemViewModel> Items { get; set; }
        public string SelectedPrefab { get; set; }
    }
}