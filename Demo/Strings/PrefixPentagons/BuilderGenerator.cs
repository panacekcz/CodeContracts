using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PrefixPentagons
{

  class BuilderGenerator
  {
   
    public void GenerateSth(StringBuilder pre, int abc, int def)
    {
      Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
      pre.Append(abc.ToString());
      pre.Append(" ");
      pre.Append(def.ToString());
    }
   
    public void CompositionBranches(StringBuilder pre, int x, int y)
    {
      Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));

      if (x < y)
      {
        CompositionLoops(pre, x);
      }
      else
      {
        GenerateSth(pre, x, y);
      }
    }

    public void CompositionLoops(StringBuilder pre, int a)
    {
      Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));

      for (int i = 0; i < a; ++i)
      {
        GenerateSth(pre, i, a - i);
      }
    }
  }

}
