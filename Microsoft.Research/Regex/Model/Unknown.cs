using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public class Unknown : Element
    {
        public Unknown(Element inner)
        {
            Pattern = inner;
        }

        public Element Pattern { get; set; }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("unknown(");
            Pattern.GenerateString(builder);
            builder.Append(")");
        }
    }
}
