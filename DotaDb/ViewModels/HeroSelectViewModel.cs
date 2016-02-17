using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HeroSelectViewModel
    {
        public IReadOnlyCollection<HeroViewModel> StrengthHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> AgilityHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> IntelligenceHeroes { get; set; }
    }
}