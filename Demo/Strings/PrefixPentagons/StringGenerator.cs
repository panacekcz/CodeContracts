using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PrefixPentagons
{
  class StringGenerator
  {
    public string GenerateSth(string pre, int abc, int def)
    {
      Contract.Ensures(Contract.Result<string>().StartsWith(pre, StringComparison.Ordinal));
      string a = pre + abc.ToString();
      a = a + " ";
      a = a + def.ToString();

      return a;
    }
    public string CompositionBranches(string pre, int x, int y)
    {
      Contract.Ensures(Contract.Result<string>().StartsWith(pre, StringComparison.Ordinal));

      if (x < y)
      {
        return CompositionLoops(pre, x);
      }
      else
      {
        return GenerateSth(pre, x, y);
      }
    }
    
    public string CompositionLoops(string pre, int a)
    {
      Contract.Ensures(Contract.Result<string>().StartsWith(pre, StringComparison.Ordinal));

      for (int i = 0; i < a; ++i)
      {
        pre = GenerateSth(pre, i, a - i);
      }

      return pre;
    }


  }
}
