using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    /// <summary>
    /// Represents a union (or alternation) of elements in a regex.
    /// </summary>
    public class Union : Element
    {
        private readonly List<Element> patterns;

        /// <summary>
        /// Gets a list of patterns which can match.
        /// </summary>
        public List<Element> Patterns
        {
            get
            {
                return patterns;
            }
        }

        public Union()
        {
            patterns = new List<Element>();
        }
        public Union(params Element[] elements)
        {
             patterns = new List<Element>(elements);
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("union(");
            foreach (var part in patterns)
                part.GenerateString(builder);
            builder.Append(")");
        }
    }
}
