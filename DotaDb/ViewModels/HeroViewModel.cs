using SourceSchemaParser.Dota2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class HeroViewModel : BaseHeroViewModel
    {
        public IReadOnlyCollection<HeroAbilityViewModel> Abilities { get; set; }
    }
}