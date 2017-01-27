using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public enum AnchorKind
    {
        Start, End
    }

    public class Anchor : Element
    {
        public AnchorKind Kind { get; set; }

    }
}
