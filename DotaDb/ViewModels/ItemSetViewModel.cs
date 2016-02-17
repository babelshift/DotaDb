using System.Collections.Generic;

namespace DotaDb.ViewModels
{
    public class ItemSetViewModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<string> Items { get; set; }
    }
}