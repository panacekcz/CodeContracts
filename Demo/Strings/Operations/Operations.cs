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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Example usage of string operations, that could be statically 
/// analyzed.
/// (Not all of them are implemented.)
/// </summary>
class Operations
{
  static void String()
  {
    string a = "constA";
    string b = "constB";

    string c;

    // Assignment
    c = a;
    // Copy
    c = (string)a.Clone();
    c = string.Copy(a);
    c = string.Intern(a);
    c = a.ToString();
    // Concat
    c = string.Concat(a, b);
    c = string.Concat(a, b, a);
    c = string.Concat(a, b, a, b);

    // Insert
    c = a.Insert(3, b);

    // Replace
    c = a.Replace('n', 'm');
    c = a.Replace("on", "e");

    // Substring
    c = a.Substring(3);
    c = a.Substring(3, 2);
    // Remove
    c = a.Remove(3);
    c = a.Remove(3, 2);
    // Pad
    c = a.PadLeft(10);
    c = a.PadLeft(10, 'x');
    c = a.PadRight(10);
    c = a.PadRight(10, 'y');
    // Trim
    c = a.Trim('c', 'o', 'd');
    c = a.TrimStart('c', 'o', 'd');
    c = a.TrimEnd('c', 'o', 'd');

    c = string.Empty;

    char h = a[3];

    int l = a.Length;

    l = string.CompareOrdinal(a, b);
    l = string.Compare(a, b, StringComparison.Ordinal);
    l = a.IndexOf("on", StringComparison.Ordinal);
    l = a.LastIndexOf("on", StringComparison.Ordinal);

    l = a.IndexOfAny(new[] { 'c', 'o', 'd' });
    l = a.LastIndexOfAny(new[] { 'c', 'o', 'd' });

  }

  static void Array()
  {
    char[] a = new char[10];

    a[0] = 'N';
    char c = a[1];

    string s = new string(a);
    s = a.ToString();
    a = s.ToCharArray();
  }

  static void StringBuilder()
  {
    StringBuilder sb = new StringBuilder();
    sb = new StringBuilder("const");

    string s = sb.ToString();

    sb.Append(s);
    sb.Insert(3, s);

    sb.Replace('c', 'd');
    sb.Remove(3, 2);

    sb.Clear();

    int l = sb.Length;
    sb.Length = 10;

  }

  static void Enumerable()
  {
    IEnumerable<char> constant = "const";
    IEnumerable<char> upper = constant.Where(c => c >= 'a' && c <= 'z').Select(c => (char)(c - 'a' + 'A'));
  }

  static void RegularExpression()
  {
    Regex regex = new Regex("abc");
    regex.IsMatch("abcdef");
    regex.Replace("abcdef", "ghi");
    Regex.IsMatch("abcdef", "abc");
  }
}