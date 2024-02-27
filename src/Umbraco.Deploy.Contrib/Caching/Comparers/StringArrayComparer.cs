using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Deploy.Contrib.Connectors.Caching.Comparers
{
    /// <summary>
    /// Compares an array of strings.
    /// </summary>
    /// <remarks>
    /// Based on: https://github.com/rhythmagency/rhythm.caching.core/blob/master/src/Rhythm.Caching.Core/Comparers/StringArrayComparer.cs
    /// </remarks>
    public class StringArrayComparer : IEqualityComparer<string[]>
    {
        /// <summary>
        /// Check if the arrays are equal.
        /// </summary>
        /// <param name="x">
        /// The first array.
        /// </param>
        /// <param name="y">
        /// The second array.
        /// </param>
        /// <returns>
        /// True, if the arrays are both null, are both empty, or both have
        /// the same strings in the same order; otherwise, false.
        /// </returns>
        public bool Equals(string[] x, string[] y)
        {
            if (x == null || y == null)
            {
                return x == null && y == null;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generates a hash code by combining all of the hash codes for the strings in the array.
        /// </summary>
        /// <param name="items">
        /// The array of strings.
        /// </param>
        /// <returns>
        /// The combined hash code.
        /// </returns>
        public int GetHashCode(string[] items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }
            else
            {
                var hashCode = default(int);
                foreach (var item in items)
                {
                    hashCode ^= (item ?? string.Empty).GetHashCode();
                }

                return hashCode;
            }
        }
    }
}
