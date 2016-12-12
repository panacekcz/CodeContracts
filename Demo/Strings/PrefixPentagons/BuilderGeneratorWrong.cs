using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PrefixPentagons
{
#if false
  [ContractVerification(false)]
  class BuilderGeneratorWrong
  {
    public void GenerateSth(StringBuilder pre, int abc, int def)
    {
      Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
      pre.Append(abc);
      pre.Append(" ");
      pre.Append(def);
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
        pre.Insert(0, "Something");
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
#endif
}
