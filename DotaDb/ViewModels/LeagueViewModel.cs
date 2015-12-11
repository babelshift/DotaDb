using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class LeagueViewModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Tier { get; set; }
        public string Location { get; set; }
        public string LogoFilePath { get; set; }
    }
}