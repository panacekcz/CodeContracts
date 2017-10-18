﻿// CodeContracts
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
    /// Represents a concat node in a string graph.
    /// </summary>
    internal class ConcatNode : InnerNode
    {
        internal ConcatNode()
          : base()
        {
        }

        public ConcatNode(IEnumerable<Node> nodes)
          : base(nodes)
        {
        }

        /// </inheritdoc>
        public override Label Label
        {
            get { return new Label(NodeKind.Concat, children.Count); }
        }

        public Node Compact()
        {
            if (indegree != 1)
                return this;
            if (children.Count == 1)
                return children[0];

            return this;
        }
    }
}
