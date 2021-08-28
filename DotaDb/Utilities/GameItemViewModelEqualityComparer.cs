using DotaDb.Models;
using System;
using System.Collections.Generic;

namespace DotaDb.Utilities
{
    public class GameItemViewModelEqualityComparer : IEqualityComparer<GameItemViewModel>
    {
        public bool Equals(GameItemViewModel x, GameItemViewModel y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(GameItemViewModel obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.Id.GetHashCode();
        }
    }
}