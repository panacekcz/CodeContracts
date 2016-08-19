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
  /// Test that using current culture leads to Top.
  /// </summary>
  public void Culture()
  {
    string constant = "prefix";
    Contract.Assert(constant.StartsWith("prefix", StringComparison.CurrentCulture));
  }

  /// <summary>
  /// Test that predicate on top string is top
  /// </summary>
  public void Top(string top)
  {
    Contract.Assert(top.StartsWith("prefix", StringComparison.Ordinal));
  }

  /// <summary>
  /// Test that starting with an approximated string is top
  /// </summary>
  public void StartsWithPrefix(bool x, bool y)
  {
    string main = x ? "prefixedString" : "prefixed";
    string pre = y ? "prefix" : "pre";
    Contract.Assert(main.StartsWith(pre, StringComparison.Ordinal));
  }

  /// <summary>
  /// Test that a longer prefix than required is not ensured
  /// </summary>
  public string ContractsEq(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(Contract.Result<string>().StartsWith("prefixString", StringComparison.Ordinal));

    return s;
  }
  /// <summary>
  /// Test that a longer prefix than required is not ensured to be false
  /// </summary>
  public string ContractsNeg(string s)
  {
    Contract.Requires(s.StartsWith("prefix", StringComparison.Ordinal));
    Contract.Ensures(!Contract.Result<string>().StartsWith("prefixString", StringComparison.Ordinal));

    return s;
  }

  public void RegexMatch(string s)
  {
    string p = "prefix" + s;
    Contract.Assert(Regex.IsMatch(p, "^prefix$"));
    Contract.Assert(Regex.IsMatch(p, "^p$"));
    Contract.Assert(Regex.IsMatch(p, "other"));
  }
}
