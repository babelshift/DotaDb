using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroItemBuildViewModel : BaseHeroViewModel
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public List<HeroItemBuildGroupViewModel> ItemGroups { get; set; }
    }
}