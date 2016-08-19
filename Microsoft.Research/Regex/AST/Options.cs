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
using System.Text.RegularExpressions;

namespace Microsoft.Research.Regex.AST
{
  using RegexOptions = System.Text.RegularExpressions.RegexOptions;

  internal class RegexOptionsUtils
  {
    private static readonly RegexOptions[] namedOptions =
    {
      RegexOptions.IgnoreCase, RegexOptions.Multiline,
      RegexOptions.Singleline, RegexOptions.ExplicitCapture,
      RegexOptions.IgnorePatternWhitespace
    };
    private static readonly char[] names = { 'i', 'm', 's', 'n', 'x' };


    public static string OptionsToString(RegexOptions ro)
    {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < namedOptions.Length; ++i)
      {
        if ((ro & namedOptions[i]) == namedOptions[i])
        {
          sb.Append(names[i]);
        }
      }
      return sb.ToString();
    }
  }

  /// <summary>
  /// Represents a directive that modifies regex options.
  /// </summary>
  public class Options : Element
  {
    private readonly RegexOptions optionsSet;
    private readonly RegexOptions optionsClear;

    public Options(RegexOptions optionsSet, RegexOptions optionsClear)
    {
      this.optionsSet = optionsSet;
      this.optionsClear = optionsClear;
    }

    internal override void GenerateString(StringBuilder builder)
    {
      builder.Append("(?");
      if (optionsSet != RegexOptions.None)
      {
        builder.Append(RegexOptionsUtils.OptionsToString(optionsSet));
      }
      if (optionsClear != RegexOptions.None)
      {
        builder.Append('-');
        builder.Append(RegexOptionsUtils.OptionsToString(optionsClear));
      }

      builder.Append(")");
    }
  }




  /// <summary>
  /// Represents a group with modified regex options.
  /// </summary>
  public class OptionsGroup : Group
  {
    private readonly RegexOptions optionsSet;
    private readonly RegexOptions optionsClear;

    public OptionsGroup(Element content, RegexOptions optionsSet, RegexOptions optionsClear)
      : base(content)
    {
      this.optionsSet = optionsSet;
      this.optionsClear = optionsClear;
    }

    internal override void GenerateString(StringBuilder builder)
    {
      builder.Append("(?");
      if (optionsSet != RegexOptions.None)
      {
        builder.Append(RegexOptionsUtils.OptionsToString(optionsSet));
      }
      if (optionsClear != RegexOptions.None)
      {
        builder.Append('-');
        builder.Append(RegexOptionsUtils.OptionsToString(optionsClear));
      }
      builder.Append(':');

      Content.GenerateString(builder);

      builder.Append(")");
    }
  }
}
