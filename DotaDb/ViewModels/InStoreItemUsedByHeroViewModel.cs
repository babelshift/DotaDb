using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class InStoreItemUsedByHeroViewModel
    {
        public int HeroId { get; set; }
        public string HeroName { get; set; }
        public string MinimapIconPath { get; set; }
    }
}