using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public class Union : Element
    {
        private readonly List<Element> patterns = new List<Element>();

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
    }
}
