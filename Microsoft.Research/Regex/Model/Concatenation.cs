using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public class Concatenation : Element
    {
        private readonly List<Element> parts = new List<Element>();

        public List<Element> Parts
        {
            get
            {
                return parts;
            }
        }
    }
}
