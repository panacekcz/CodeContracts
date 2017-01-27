using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public class Loop : Element
    {
        int Min { get; set; }
        int Max { get; set; }

        Element Pattern { get; set; }
    }
}
