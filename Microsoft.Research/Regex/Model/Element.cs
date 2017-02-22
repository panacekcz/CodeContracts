using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    /// <summary>
    /// Represents an element of a regex model.
    /// </summary>
    public abstract class Element
    {
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            GenerateString(builder);
            return builder.ToString();
        }

        internal abstract void GenerateString(StringBuilder builder);
    }
}
