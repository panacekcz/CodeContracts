using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    /// <summary>
    /// Represents a loop element of a regex.
    /// </summary>
    /// <remarks>
    /// Used for ?, +, *, {}.
    /// </remarks>
    public class Loop : Element
    {
        /// <summary>
        /// The constant used to specify unbounded number of occurences.
        /// </summary>
        public const int Unbounded = -1;
        
        /// <summary>
        /// Gets or sets the minimum number of occurences.
        /// </summary>
        public int Min { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of occurences. Might be <see cref="Unbounded"/>.
        /// </summary>
        public int Max { get; set; }
        /// <summary>
        /// Gets or sets the pattern that is repeated.
        /// </summary>
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
            if (Min == Unbounded)
                builder.Append("inf");
            else
                builder.Append(Min);
            builder.Append(",");
            if (Max == Unbounded)
                builder.Append("inf");
            else
                builder.Append(Max);
            builder.Append(")");
        }
    }
}
