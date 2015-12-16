using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class InStoreViewModel
    {
        public IReadOnlyCollection<InStoreItemPrefabViewModel> Prefabs { get; set; }
        public IPagedList<InStoreItemViewModel> Items { get; set; }
    }
}