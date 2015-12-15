using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class HeroViewModel : BaseHeroViewModel
    {
        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }
    }
}