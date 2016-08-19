// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

/// <summary>
/// Contains test contracts/assertions 
/// that can be proven using the prefix domain.
/// </summary>
public class Proven
{
  /// <summary>
  /// Tests constant prefixes of constant.
  /// </summary>
  public void PrefixConstant()
  {
    string constant = "prefixString";
    Contract.Assert(constant.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Assert(constant.StartsWith("prefixString", StringComparison.Ordinal));
    Contract.Assert(constant.StartsWith("", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining prefixes.
  /// </summary>
  public void PrefixJoin(bool x)
  {
    string value;
    if (x)
      value = "prefixA";
    else
      value = "prefixB";

    Contract.Assert(value.StartsWith("prefix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining prefixes in a loop with concatenation.
  /// </summary>
  public void PrefixLoopRight(int counter)
  {
    string value = "prefix";

    for (int i = 0; i < counter; ++i)
    {
      value = value + "Suffix";
    }

    Contract.Assert(value.StartsWith("prefix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining prefixes in a loop with concatenation.
  /// </summary>
  public void PrefixLoopLeft(int counter)
  {
    string value = "prefix";

    for (int i = 0; i < counter; ++i)
    {
      value = "prefixAnd" + value;
    }

    Contract.Assert(value.StartsWith("prefix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests replacing constant in a prefix.
  /// </summary>
  public void ReplaceString(string any)
  {
    string value = "PrefixWithPre" + any;
    string replaced = value.Replace("Prefix", "Other");
    Contract.Assert(replaced.StartsWith("OtherWith", StringComparison.Ordinal));
  }

  public void Insert(string any)
  {
    string value1 = "prefix" + any;
    string value2 = "other" + any;

    Contract.Assert(value1.Insert(3, "const").StartsWith("preconstfix", StringComparison.Ordinal)); // Const into Prefix
    Contract.Assert(value1.Insert(3, value2).StartsWith("preother", StringComparison.Ordinal)); // Prefix into Prefix
    Contract.Assert(value1.Insert(10, "const").StartsWith("prefix", StringComparison.Ordinal)); // Anything after prefix
    Contract.Assert("const".Insert(3, value1).StartsWith("conprefix", StringComparison.Ordinal)); // Prefix into const
  }

  public void PadLeft(string any)
  {
    string value = "    prefix" + any;
    Contract.Assert(value.PadLeft(10, '.').StartsWith("    prefix", StringComparison.Ordinal));
    Contract.Assert(value.PadLeft(20).StartsWith("    ", StringComparison.Ordinal));
  }

  public void PadRight(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value.PadRight(20, '.').StartsWith("prefix", StringComparison.Ordinal));
  }

  public void IsNullOrEmpty(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(!string.IsNullOrEmpty(value));
  }

  public void Contains(string any)
  {
    string value1 = "prefix" + any;

    Contract.Assert(!"const".Contains(value1));
    Contract.Assert(value1.Contains("pre"));
  }

  /// <summary>
  /// Tests impossible prefix.
  /// </summary>
  public void OtherPrefix(string s)
  {
    string value = "not" + s;
    Contract.Assert(!value.StartsWith("prefix", StringComparison.Ordinal));
  }
  /// <summary>
  /// Tests substring operations.
  /// </summary>
  public void Substring(string s)
  {
    string value = "prefix" + s;
    Contract.Assert(value.Substring(3).StartsWith("fix", StringComparison.Ordinal));
    Contract.Assert(value.Substring(3, 10).StartsWith("fix", StringComparison.Ordinal));
    Contract.Assert(value.Substring(2, 2).StartsWith("ef", StringComparison.Ordinal)); //Constant
  }
  /// <summary>
  /// Tests remove operations.
  /// </summary>
  public void Remove(string s)
  {
    string value = "prefix" + s;
    Contract.Assert(value.Remove(3).StartsWith("pre", StringComparison.Ordinal)); //Constant
    Contract.Assert(value.Remove(3, 10).StartsWith("pre", StringComparison.Ordinal));
    Contract.Assert(value.Remove(2, 2).StartsWith("prix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests that the same prefix as required is ensured.
  /// </summary>
  public string ContractsEq(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().StartsWith("prefix", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Tests that a different prefix than required is ensured to be false.
  /// </summary>
  public string ContractsNeg(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(!Contract.Result<string>().StartsWith("other", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Test that a shorter prefix than required is ensured.
  /// </summary>
  public string ContractsPre(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().StartsWith("pre", StringComparison.Ordinal));

    return s;
  }

  /// <summary>
  /// Tests that a longer prefix is ensured after concatenation.
  /// </summary>
  public string ContractsCat(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().StartsWith("otherpre", StringComparison.Ordinal));

    return "other" + s;
  }


  public void Assume(string s)
  {
    Contract.Assume(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
  }

  public void AssumeEqual(string s)
  {
    Contract.Assume(s == "constant");
    Contract.Assert(s.StartsWith("const", StringComparison.Ordinal));
  }

  public void BranchOr(string s)
  {
    if (s.StartsWith("prefix", StringComparison.Ordinal) || s.StartsWith("precise", StringComparison.Ordinal))
    {
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
  }

  public void BranchAnd(string s, string t)
  {
    if (s.StartsWith("pre", StringComparison.Ordinal) && t.StartsWith("prefix", StringComparison.Ordinal))
    {
      Contract.Assert(t.StartsWith("prefix", StringComparison.Ordinal));
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
  }

  public void BranchTrue(string s)
  {
    if (s.StartsWith("prefix", StringComparison.Ordinal))
    {
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
    else
    {
      //Contract.Assert(s.StartsWith("pre")); should not be proven
    }
  }

  public void BranchFalse(string s)
  {
    if (!s.StartsWith("prefix", StringComparison.Ordinal))
    {
      //Contract.Assert(s.StartsWith("pre")); should not be proven
    }
    else
    {
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
  }

  public void BranchMeet(string s)
  {
    Contract.Assume(s.StartsWith("prefix", StringComparison.Ordinal));
    if (s.StartsWith("p", StringComparison.Ordinal))
    {
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
    else
    {
      // Unreachable
      Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
    }
  }

  public void RegexAssume(string s)
  {
    Contract.Requires(Regex.IsMatch(s, "^prefix"));
    Contract.Assert(Regex.IsMatch(s, "^prefix"));
    Contract.Assert(s.StartsWith("prefix", StringComparison.Ordinal));

  }

  public void RegexMatch(string s)
  {
    string p = "prefix" + s;
    Contract.Assert(Regex.IsMatch(p, "^prefix"));
    Contract.Assert(Regex.IsMatch(p, "^p"));
  }

  public void NullJoin(bool b)
  {
    string s;

    if (b)
    {
      s = "const";
    }
    else
    {
      s = null;
    }

    s = "pre" + s;

    Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
  }

  public void NullConcat(string s)
  {
    s = string.Concat("pre", s);
    s = string.Concat(null, s);
    Contract.Assert(s.StartsWith("pre", StringComparison.Ordinal));
  }

}
