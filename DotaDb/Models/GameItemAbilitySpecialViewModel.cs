using System.Collections.Generic;

namespace DotaDb.Models
{
    public class GameItemAbilitySpecialViewModel
    {
        public IReadOnlyList<string> Value { get; set; }
        public string RawName { get; set; }
    }
}