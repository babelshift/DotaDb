using DotaDb.Models;
using System;
using System.Collections.Generic;

namespace DotaDb.Utilities
{
    public class HeroViewModelEqualityComparer : IEqualityComparer<HeroViewModel>
    {
        public bool Equals(HeroViewModel x, HeroViewModel y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(HeroViewModel obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.Id.GetHashCode();
        }
    }
}