using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public abstract class Anchor : Element
    {
        public static readonly Begin Begin = new Begin();
        public static readonly End End = new End();
    }

    public class Begin : Anchor {
        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("begin()");
        }
    }
    public class End : Anchor {
        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("end()");
        }
    }
}
