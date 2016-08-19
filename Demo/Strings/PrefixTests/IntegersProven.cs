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


public class IntegersProven
{
  public void IndexOf(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value.IndexOf("", StringComparison.Ordinal) == 0);
    Contract.Assert(value.IndexOf("ref", StringComparison.Ordinal) == 1);
    Contract.Assert(value.IndexOf("zzzz", StringComparison.Ordinal) >= -1);//also proven by contracts
  }

  public void LastIndexOf(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value.LastIndexOf("", StringComparison.Ordinal) >= 5);
    Contract.Assert(value.LastIndexOf("ref", StringComparison.Ordinal) >= 1);
    Contract.Assert(value.LastIndexOf("zzzz", StringComparison.Ordinal) >= -1);
  }

  public void IndexOfChar(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value.IndexOf('e') == 2);
    Contract.Assert(value.IndexOf('z') >= -1);
  }

  public void LastIndexOfChar(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value.LastIndexOf('e') >= 2);
    Contract.Assert(value.LastIndexOf('z') >= -1);
  }

  public void CharAt(string any)
  {
    string value = "prefix" + any;
    Contract.Assert(value[4] == 'i');
  }
}
