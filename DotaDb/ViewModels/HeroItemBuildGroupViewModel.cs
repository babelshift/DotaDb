using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HeroItemBuildGroupViewModel
    {
        public string Title { get; set; }
        public List<HeroItemBuildItemViewModel> Items { get; set; }
    }
}