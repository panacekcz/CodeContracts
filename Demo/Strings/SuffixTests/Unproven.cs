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

public class Unproven
{
  /// <summary>
  /// Tests that using current culture leads to Top.
  /// </summary>
  public void Culture()
  {
    string constant = "suffix";
    Contract.Assert(constant.EndsWith("suffix", StringComparison.CurrentCulture));
  }

  /// <summary>
  /// Tests that predicate on top string is Top.
  /// </summary>
  public void Top(string top)
  {
    Contract.Assert(top.EndsWith("suffix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests that endnding with an approximated string is top.
  /// </summary>
  public void StartsWithPrefix(bool x, bool y)
  {
    string main = x ? "withSuffix" : "suffix";
    string pre = y ? "suffix" : "fix";
    Contract.Assert(main.EndsWith(pre, StringComparison.Ordinal));
  }

  /// <summary>
  /// Tests that a longer suffix than required is not ensured.
  /// </summary>
  public string ContractsEq(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().EndsWith("stringsuffix", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Tests that a longer suffix than required is not ensured to be false.
  /// </summary>
  public string ContractsNeg(string s)
  {
    Contract.Requires(s.EndsWith("suffix", StringComparison.Ordinal));
    Contract.Ensures(!Contract.Result<string>().EndsWith("stringsuffix", StringComparison.Ordinal));

    return s;
  }

  public void Substring(string s)
  {
    string value = s + "suffix";

    Contract.Assert(value.Substring(3, 10).EndsWith("fix", StringComparison.Ordinal));
    Contract.Assert(value.Substring(2, 2).EndsWith("ff", StringComparison.Ordinal)); //Constant
  }

  public void RegexMatch(string s)
  {
    string p = s + "prefix";
    Contract.Assert(Regex.IsMatch(p, "^suffix$"));
    Contract.Assert(Regex.IsMatch(p, "^s$"));
    Contract.Assert(Regex.IsMatch(p, "other"));
  }
}
