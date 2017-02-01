using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
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

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("char(");
            bool first = true;
            foreach(var range in CanMatch.Ranges)
            {
                if (first)
                    first = false;
                else
                    builder.Append(",");

                if(range.Low != range.High)
                    builder.AppendFormat("{0:X}-{1:X}", (int)range.Low, (int)range.High);
                else
                    builder.AppendFormat("{0:X}", (int)range.Low);
            }

            builder.Append(")");
        }
    }

}
