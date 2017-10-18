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
    /// Trims characters from the end of a string graph.
    /// </summary>
    class TrimEndVisitor : TrimVisitor
    {
        public TrimEndVisitor(HashSet<char> trimmedChars) :
          base(trimmedChars)
        {
        }


        protected override Node VisitChildren(ConcatNode concatNode, Node result, ref TrimVisitorState data)
        {
            ConcatNode concatResult = ((ConcatNode)result);

            foreach (Node child in Enumerable.Reverse(concatNode.children))
            {
                Node trimmedChild = VisitNode(child, VisitContext.Concat, ref data);
                if (trimmedChild != null)
                {
                    concatResult.children.Add(trimmedChild);
                }
            }

            concatResult.children.Reverse();

            return concatResult;
        }
    }
}
