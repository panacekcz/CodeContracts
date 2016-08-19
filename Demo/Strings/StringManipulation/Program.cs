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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;


class Program
{
  public static void Main()
  {
    string text = Console.In.ReadLine();
    int M = int.Parse(Console.In.ReadLine());

    Contract.Assume(Regex.IsMatch(text, "^[a-z]*\\z"));

    string buffer = "";

    for (int i = 0; i < M; ++i)
    {
      int[] parts = Console.In.ReadLine().Split(new char[] { ' ' }).Select(s => int.Parse(s)).ToArray();

      int l = parts[0];
      int r = parts[1];
      int k = parts[2];

      int length = r - l + 1;
      --r;
      --l;

      k %= length;

      buffer = "";
      for (int j = 0; j < length; ++j)
      {
        buffer = buffer + text.Substring(l + j, 1);
      }
      for (int j = 0; j < length; ++j)
      {
        int index = l + (j + k) % length;
        text = text.Substring(0, index) + buffer.Substring(j, 1) + text.Substring(index + 1);
      }
    }

    Contract.Assert(Regex.IsMatch(text, "^[a-z]*\\z"));
    Console.Out.WriteLine(text);
  }
}
