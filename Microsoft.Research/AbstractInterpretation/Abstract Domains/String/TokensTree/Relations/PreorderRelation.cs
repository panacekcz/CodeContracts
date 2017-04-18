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
    internal class PreorderRelation : TokensTreeRelation
    {
        public PreorderRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }
        public static bool LessEqual(InnerNode le, InnerNode ge)
        {
            if (le == ge)
                return true;

            PreorderRelation preorder = new PreorderRelation(le, ge);
            return preorder.Solve();

        }
        protected override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        protected override bool Next(InnerNode left, InnerNode right)
        {

            if (left.Accepting && !right.Accepting)
                return false;

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (!right.children.TryGetValue(child.Key, out rightChild))
                    return false;

                Request(child.Value, rightChild);
            }

            return true;

        }
    }

}
