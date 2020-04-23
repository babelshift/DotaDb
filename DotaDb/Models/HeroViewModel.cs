using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroViewModel : BaseHeroViewModel
    {
        public HeroViewModel() : base()
        {
            Abilities = new List<HeroAbilityViewModel>();
        }

        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }
    }
}