using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PrefixPentagons
{
  class Transitivity
  {
    public void Mehtod(string a, string b, string c)
    {
      Contract.Assume(a.StartsWith(b, StringComparison.Ordinal));
      Contract.Assume(b.StartsWith(c, StringComparison.Ordinal));

      Contract.Assert(a.StartsWith(c, StringComparison.Ordinal));
    }
  }
}
