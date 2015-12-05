using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotaDb.ViewModels
{
    public class GameItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public int Cost { get; set; }
        public bool SecretShop { get; set; }
        public bool SideShop { get; set; }
        public bool IsRecipe { get; set; }
    }
}