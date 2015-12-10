using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroSelectViewModel
    {
        public IReadOnlyCollection<HeroViewModel> StrengthHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> AgilityHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> IntelligenceHeroes { get; set; }
    }
}