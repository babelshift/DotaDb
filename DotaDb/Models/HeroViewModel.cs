using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroViewModel : BaseHeroViewModel
    {
        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }
    }
}