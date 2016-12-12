using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
#if false
namespace PrefixPentagons
{
  class StringGeneratorInfer
  {
    public string GenerateSth(string pre, int abc, int def)
    {
      string a = pre + abc.ToString();
      a = a + " ";
      a = a + def.ToString();

      return a;
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
#endif