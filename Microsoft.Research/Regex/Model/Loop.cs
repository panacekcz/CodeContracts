using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public class Loop : Element
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public Element Pattern { get; set; }

        public Loop(Element pattern, int min, int max)
        {
            Pattern = pattern;
            Min = min;
            Max = max;
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("loop(");
            Pattern.GenerateString(builder);
            builder.Append(",");
            builder.Append(Min);
            builder.Append(",");
            builder.Append(Max);
            builder.Append(")");
        }
    }
}
