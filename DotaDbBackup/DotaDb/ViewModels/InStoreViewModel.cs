using PagedList;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class InStoreViewModel
    {
        public IReadOnlyCollection<InStoreItemPrefabViewModel> Prefabs { get; set; }
        public IPagedList<InStoreItemViewModel> Items { get; set; }
        public string SelectedPrefab { get; set; }
    }
}