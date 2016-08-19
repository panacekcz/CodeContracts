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

namespace Microsoft.Research.Regex.AST
{
  public class Loop : Element
  {
    internal const int UNBOUNDED = -1;

    private readonly bool lazy;
    private readonly int min, max;
    private readonly Element content;

    public Element Content { get { return content; } }
    public int Min { get { return min; } }
    public int Max { get { return max; } }
    public bool IsUnbounded { get { return max == UNBOUNDED; } }
    public bool Lazy { get { return lazy; } }

    public Loop(int min, int max, Element content, bool lazy)
    {
      this.min = min;
      this.max = max;
      this.content = content;
      this.lazy = lazy;
    }

    internal override void GenerateString(StringBuilder builder)
    {
      content.GenerateString(builder);
      if (min == 0 && max == 1)
        builder.Append('?');
      else if (min == 0 && max == UNBOUNDED)
        builder.Append('*');
      else if (min == 1 && max == UNBOUNDED)
        builder.Append('+');
      else
      {
        builder.Append('{');
        builder.Append(min);
        if (min != max)
        {
          builder.Append(',');
          if (max != -1)
          {
            builder.Append(max);
          }
        }
        builder.Append('}');
      }

      if (lazy)
      {
        builder.Append('?');
      }
    }

  }
}
