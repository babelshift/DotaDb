using DotaDb.Models;
using System;
using System.Collections.Generic;

namespace DotaDb.Utilities
{
    public class InStoreItemViewModelEqualityComparer : IEqualityComparer<InStoreItemViewModel>
    {
        public bool Equals(InStoreItemViewModel x, InStoreItemViewModel y)
        {
            return x.DefIndex == y.DefIndex;
        }

        public int GetHashCode(InStoreItemViewModel obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return obj.DefIndex.GetHashCode();
        }
    }
}