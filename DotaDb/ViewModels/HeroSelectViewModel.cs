using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroSelectViewModel
    {
        public IReadOnlyCollection<HeroSelectItemViewModel> StrengthHeroes { get; set; }
        public IReadOnlyCollection<HeroSelectItemViewModel> AgilityHeroes { get; set; }
        public IReadOnlyCollection<HeroSelectItemViewModel> IntelligenceHeroes { get; set; }
    }
}