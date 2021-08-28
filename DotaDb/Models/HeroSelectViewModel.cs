using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroSelectViewModel
    {
        public HeroSelectViewModel()
        {
            StrengthHeroes = new List<HeroViewModel>();
            AgilityHeroes = new List<HeroViewModel>();
            IntelligenceHeroes = new List<HeroViewModel>();
        }

        public IReadOnlyCollection<HeroViewModel> StrengthHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> AgilityHeroes { get; set; }
        public IReadOnlyCollection<HeroViewModel> IntelligenceHeroes { get; set; }
    }
}