using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class ItemSetViewModel
    {
        public string Name { get; set; }
        public IReadOnlyCollection<string> Items { get; set; }
    }
}