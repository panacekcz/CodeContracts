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
  /// Test joining prefixes
  /// </summary>
  public void Join(bool x)
  {
    string value;
    if (x)
      value = "something";
    else
      value = "other";

    Contract.Assert(value.Contains("r"));
    Contract.Assert(value.Contains("g"));
    Contract.Assert(value.Contains("s"));
    Contract.Assert(!value.Contains("i"));
  }

  public void StartsWith(string s)
  {
    Contract.Assume(s.Contains("a"));
    Contract.Assert(s.StartsWith("a", StringComparison.Ordinal));
  }

  public void RegexMatch(string s)
  {
    Contract.Assume(s.Contains("a"));
    Contract.Assume(s.Contains("b"));

    Contract.Assume(!s.Contains("x"));
    Contract.Assume(!s.Contains("y"));

    Contract.Assert(Regex.IsMatch(s, "u"));
    Contract.Assert(Regex.IsMatch(s, "ab"));
    Contract.Assert(Regex.IsMatch(s, "^a"));
  }

  public void RegexMatch2(string s)
  {
    Contract.Assume(s.Contains("a"));
    Contract.Assume(s.Contains("b"));

    Contract.Assume(!s.Contains("x"));
    Contract.Assume(!s.Contains("y"));

    Contract.Assert(Regex.IsMatch(s, "u|x"));
  }
}