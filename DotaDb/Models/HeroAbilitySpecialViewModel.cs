﻿using System.Collections.Generic;

namespace DotaDb.Models
{
    public class HeroAbilitySpecialViewModel
    {
        public string Name { get; set; }
        public IReadOnlyList<string> Value { get; set; }
        public string RawName { get; set; }
    }
}