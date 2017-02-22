using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    /// <summary>
    /// Represents a concatenation of elements in a regex.
    /// </summary>
    public class Concatenation : Element
    {
        private readonly List<Element> parts = new List<Element>();

        /// <summary>
        /// Gets the list of elements that are concatenated.
        /// </summary>
        public List<Element> Parts
        {
            get
            {
                return parts;
            }
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("concat(");
            foreach (var part in parts)
                part.GenerateString(builder);
            builder.Append(")");
        }
    }
}
