using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public struct CharRange
    {
        private char low, high;

        public char High
        {
            get
            {
                return high;
            }
        }

        public char Low
        {
            get
            {
                return low;
            }
        }

        public CharRange(char low, char high)
        {
            this.low = low;
            this.high = high;
        }
    }
    public struct CharRanges
    {
        private IEnumerable<CharRange> ranges;

        public IEnumerable<CharRange> Ranges
        {
            get
            {
                return ranges;
            }
        }

        public CharRanges(IEnumerable<CharRange> ranges)
        {
            this.ranges = ranges;
        }
    }

    public class Character : Element
    {
        private CharRanges mustMatch, canMatch;

        public CharRanges MustMatch { get { return mustMatch; } }
        public CharRanges CanMatch { get { return mustMatch; } }

        public Character(CharRanges mustMatch, CharRanges canMatch)
        {
            this.mustMatch = mustMatch;
            this.canMatch = canMatch;
        }

    }

}
