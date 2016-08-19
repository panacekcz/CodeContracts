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
/// that can be proven using the suffix domain.
/// </summary>
public class Proven
{
  /// <summary>
  /// Tests constant suffixes of constant.
  /// </summary>
  public void SuffixConstant()
  {
    string constant = "suffixString";
    Contract.Assert(constant.EndsWith("String", StringComparison.Ordinal));
    Contract.Assert(constant.EndsWith("suffixString", StringComparison.Ordinal));
    Contract.Assert(constant.EndsWith("", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining suffixes.
  /// </summary>
  public void SuffixJoin(bool x)
  {
    string value;
    if (x)
      value = "asuffix";
    else
      value = "Bsuffix";

    Contract.Assert(value.EndsWith("suffix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining suffixes in a loop with concatenation.
  /// </summary>
  public void SuffixLoopRight(int counter)
  {
    string value = "Suffix";

    for (int i = 0; i < counter; ++i)
    {
      value = value + "andSuffix";
    }

    Contract.Assert(value.EndsWith("Suffix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests joining suffixes in a loop with concatenation.
  /// </summary>
  public void SuffixLoopLeft(int counter)
  {
    string value = "suffix";

    for (int i = 0; i < counter; ++i)
    {
      value = "suffixAnd" + value;
    }

    Contract.Assert(value.EndsWith("suffix", StringComparison.Ordinal));
  }

  public void PadLeft(string any)
  {
    string value = any + "suffix";
    Contract.Assert(value.PadLeft(10, '.').EndsWith("suffix", StringComparison.Ordinal));
  }

  public void PadRight(string any)
  {
    string value = any + "suffix     ";
    Contract.Assert(value.PadRight(10, '.').EndsWith("suffix     ", StringComparison.Ordinal));
    Contract.Assert(value.PadRight(20).EndsWith("     ", StringComparison.Ordinal));
  }

  public void IsNullOrEmpty(string any)
  {
    string value = any + "suffix";
    Contract.Assert(!string.IsNullOrEmpty(value));
  }

  public void Contains(string any)
  {
    string value1 = any + "suffix";

    Contract.Assert(!"const".Contains(value1));
    Contract.Assert(value1.Contains("fix"));
  }

  /// <summary>
  /// Tests impossible suffix.
  /// </summary>
  public void OtherSuffix(string s)
  {
    string value = s + "not";
    Contract.Assert(!value.EndsWith("suffix", StringComparison.Ordinal));
  }
  /// <summary>
  /// Tests substring operations.
  /// </summary>
  public void Substring(string s)
  {
    string value = s + "suffix";
    Contract.Assert(value.Substring(3).EndsWith("fix", StringComparison.Ordinal));

  }
  /// <summary>
  /// Tests remove operations.
  /// </summary>
  public void Remove(string s)
  {
    string value = s + "suffix";
    Contract.Assert(value.Remove(1, 2).EndsWith("fix", StringComparison.Ordinal)); //Constant    
  }

  /// <summary>
  /// Tests that the same suffix as required is ensured.
  /// </summary>
  public string ContractsEq(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().EndsWith("suffix", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Tests that a different suffix than required is ensured to be false.
  /// </summary>
  public string ContractsNeg(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(!Contract.Result<string>().EndsWith("other", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Tests that a shorter suffix than required is ensured.
  /// </summary>
  public string ContractsSuffix(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().EndsWith("fix", StringComparison.Ordinal));

    return s;
  }

  /// <summary>
  /// Tests that a longer suffix is ensured after concatenation.
  /// </summary>
  public string ContractsCat(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().EndsWith("fixother", StringComparison.Ordinal));

    return s + "other";
  }


  public void Assume(string s)
  {
    Contract.Assume(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
  }

  public void AssumeEqual(string s)
  {
    Contract.Assume(s == "constant");
    Contract.Assert(s.EndsWith("ant", StringComparison.Ordinal));
  }

  public void BranchOr(string s)
  {
    if (s.EndsWith("suffix", StringComparison.Ordinal) || s.EndsWith("hotfix", StringComparison.Ordinal))
    {
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
  }

  public void BranchAnd(string s, string t)
  {
    if (s.EndsWith("fix", StringComparison.Ordinal) && t.EndsWith("suffix", StringComparison.Ordinal))
    {
      Contract.Assert(t.EndsWith("suffix", StringComparison.Ordinal));
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
  }

  public void BranchTrue(string s)
  {
    if (s.EndsWith("suffix", StringComparison.Ordinal))
    {
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
    else
    {
      //Contract.Assert(s.EndsWith("fix")); should not be proven
    }
  }

  public void BranchFalse(string s)
  {
    if (!s.EndsWith("suffix", StringComparison.Ordinal))
    {
      //Contract.Assert(s.EndsWith("fix")); should not be proven
    }
    else
    {
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
  }

  public void BranchMeet(string s)
  {
    Contract.Assume(s.EndsWith("suffix", StringComparison.Ordinal));
    if (s.EndsWith("x", StringComparison.Ordinal))
    {
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
    else
    {
      // Unreachable
      Contract.Assert(s.EndsWith("fix", StringComparison.Ordinal));
    }
  }

  public void RegexAssume(string s)
  {
    Contract.Requires(Regex.IsMatch(s, "suffix\\z"));
    Contract.Assert(Regex.IsMatch(s, "suffix\\z"));
    Contract.Assert(s.EndsWith("suffix", StringComparison.Ordinal));

  }

  public void RegexMatch(string s)
  {
    string p = s + "suffix";
    Contract.Assert(Regex.IsMatch(p, "suffix\\z"));
    Contract.Assert(Regex.IsMatch(p, "x\\z"));
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

    s = s + "suffix";

    Contract.Assert(s.EndsWith("suffix", StringComparison.Ordinal));
  }

  public void NullConcat(string s)
  {
    s = string.Concat(s, "suffix");
    s = string.Concat(s, null);
    Contract.Assert(s.EndsWith("suffix", StringComparison.Ordinal));
  }
}
