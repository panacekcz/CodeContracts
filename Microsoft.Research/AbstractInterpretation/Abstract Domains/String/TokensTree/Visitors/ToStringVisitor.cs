// CodeContracts
// 
// Copyright 2016-2017 Charles University
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

// Created by Vlastimil Dort (2016-2017)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Generates a string representation of a tokens tree.
    /// </summary>
    /// <remarks>
    /// Repeat node is *, inner node is {cX...}. (if the nodes is not accepting) or {cX...}! if accepting.
    /// For str it is {s{t{r{}!}.}.}.
    /// </remarks>
    internal class ToStringVisitor : TokensTreeVisitor<string>
    {
        public string ToString(TokensTreeNode node)
        {
            return VisitNode(node);
        }

        #region TokensTreeVisitor<string> overrides

        protected override string VisitInnerNode(InnerNode inn)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('{');

            foreach (var child in inn.children.OrderBy(child => child.Key))
            {
                sb.Append(child.Key);
                sb.Append(VisitNode(child.Value));
            }
           
            sb.Append('}');
            sb.Append(inn.Accepting ? '!' : '.');

            return sb.ToString();
        }

        protected override string VisitRepeatNode(RepeatNode inn)
        {
            return "*";
        }
        #endregion

    }

 
}
