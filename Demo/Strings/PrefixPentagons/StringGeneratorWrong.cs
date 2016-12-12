using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace PrefixPentagons
{
  class StringGeneratorWrong
  {
    public string GenerateSth(string pre, int abc, int def)
    {
      Contract.Ensures(Contract.Result<string>().StartsWith(pre, StringComparison.Ordinal));
      string a = pre + "abc";
      a = a + " ";
      a = def + "a";

      return a;
    }

    public string GenerateSthElse(string pre, int abc, int def)
    {
      string a = pre + "abc";
      a = a + " ";
      a = def + "a";

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
        return "OK" + GenerateSth(pre, x, y);
      }
    }

    public string CompositionLoops(string pre, int a)
    {
      Contract.Ensures(Contract.Result<string>().StartsWith(pre, StringComparison.Ordinal));

      for (int i = 0; i < a; ++i)
      {
        pre = GenerateSthElse(pre, i, a - i);
      }

      return pre;
    }


  }
}
