﻿// CodeContracts
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

// Created by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    internal class ConcatVisitor : TokensTreeTransformer
    {
        private InnerNode append;
        private bool addedAsRoot = false;
        public ConcatVisitor(TokensTreeMerger merger, InnerNode append) :
            base(merger)
        {
            this.append = append;
        }


        public void ConcatTo(InnerNode left)
        {
            Transform(left);
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            // Concat to children first
            TokensTreeNode newInn = base.VisitInnerNode(innerNode);

            if (innerNode.Accepting)
            {
                //TODO: VD: not optimal
                InnerNode newInnNotAccepting = new InnerNode((InnerNode)newInn);
                newInnNotAccepting.accepting = false;

                return Merge(newInnNotAccepting, append);
            }
            else
                return newInn;
        }

        protected override TokensTreeNode VisitRepeatNode(RepeatNode inn)
        {
            throw new InvalidOperationException();

        }
    }

}