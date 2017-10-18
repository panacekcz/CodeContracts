// CodeContracts
// 
// Copyright (c) Charles University
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

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    /// <summary>
    /// Extracts a string graph corresponding to substrings beginning
    /// at (after) an index from an interval.
    /// </summary>
    internal class SliceAfterVisitor : SliceVisitor
    {
        public SliceAfterVisitor(LengthVisitor lengths) :
          base(lengths)
        {
        }

        protected override Node Visit(CharNode charNode, VisitContext context, ref IndexInterval data)
        {
            if (data.IsBottom || data.UpperBound == 0)
            {
                // We are at a position in the graph where for all strings we are at or after the 
                // upper bound of the index, that means the character is always after the index,
                // so it is unconditionally included.
                return charNode;
            }
            else if (data.LowerBound > 0)
            {
                // We are at a position in the graph before the lower bound of the index,
                // that means the character is never after the index, so it is excluded.
                return NodeBuilder.CreateEmptyNode();
            }
            else
            {
                // In other cases, the character may be included or not.
                return NodeBuilder.CreateOptionalNode(charNode);
            }
        }
    }
}
