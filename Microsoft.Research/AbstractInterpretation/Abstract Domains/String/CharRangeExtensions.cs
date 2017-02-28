using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Helper methods for <see cref="CharRange"/> and <see cref="CharRanges"/>.
    /// </summary>
    static class CharRangeExtensions
    {
        /// <summary>
        /// Converts a character range (from regex) to a character interval.
        /// </summary>
        /// <param name="range">Character range from regex.</param>
        /// <returns>Character interval with the same values as <paramref name="range"/>. </returns>
        public static CharInterval ToInterval(this CharRange range)
        {
            return CharInterval.For(range.Low, range.High);
        }

        /// <summary>
        /// Converts multiple character range (from regex) to multiple character interval.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <returns>Character intervals with the same values as <paramref name="ranges"/>. </returns>
        public static IEnumerable<CharInterval> ToIntervals(this CharRanges ranges)
        {
            return ranges.Ranges.Select(ToInterval);
        }

        /// <summary>
        /// Tries to extract the first character from character ranges.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <param name="first">Set to the first char from <paramref name="ranges"/>.</param>
        /// <returns>True, if <paramref name="ranges"/> contains at least one character.</returns>
        public static bool TryGetFirst(this CharRanges ranges, out char first)
        {
            foreach (var range in ranges.Ranges)
            {
                first = range.Low;
                return true;
            }
            first = default(char);
            return false;
        }

        /// <summary>
        /// Tries to extract a single character from character ranges.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <param name="singleton">Set to the single char from <paramref name="ranges"/>.</param>
        /// <returns>True, if <paramref name="ranges"/> contains exactly one character.</returns>
        public static bool TryGetSingleton(this CharRanges ranges, out char singleton)
        {
            bool first = true;
            singleton = default(char);
            foreach(var range in ranges.Ranges)
            {
                if (first)
                {
                    if (range.Low != range.High)
                        return false;

                    first = false;
                    singleton = range.Low;
                    
                }
                else
                {
                    return false;
                }
            }
            return !first;
        }
    }
}
