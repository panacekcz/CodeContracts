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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class BricksTest : BricksTestBase
    {
        protected override IBricksPolicy CreateBricksPolicy()
        {
            return new DefaultBricksPolicy { ExpandConstantRepetitions = false, MergeConstantSets = false };
        }

        /// <summary>
        /// Tests converting string constants to bricks.
        /// </summary>
        [TestMethod]
        public void Constant()
        {
            Bricks bricks = MakeBricks("const");
            Bricks empty = MakeBricks("");

            AssertString("{const}[1,1]", bricks);
            AssertString("", empty);
        }


        /// <summary>
        /// Tests extending brick lists.
        /// </summary>
        [TestMethod]
        public void Extend()
        {
            Bricks one = MakeBricks("one");
            Bricks two = MakeBricks("two");
            Bricks longBricks = operations.Concat(Arg(operations.Concat(Arg(one), Arg(two))), Arg(one));


            Assert.AreEqual("{one}[1,1]{}[0,0]{}[0,0]", one.Policy.Extend(one, longBricks).ToString());
            Assert.AreEqual("{}[0,0]{two}[1,1]{}[0,0]", two.Policy.Extend(two, longBricks).ToString());
            Assert.AreEqual("{one}[1,1]{two}[1,1]{one}[1,1]", longBricks.Policy.Extend(longBricks, longBricks).ToString());

            Assert.AreEqual("{one}[1,1]", one.Policy.Extend(one, two).ToString());
        }

        [TestMethod]
        public void Join()
        {
            Bricks one = MakeBricks("one");
            Bricks two = MakeBricks("two");
            Bricks join = one.Join(two);

            Assert.AreEqual("{one,two}[1,1]", join.ToString());

            Assert.AreEqual("{one}[1,1]", one.Join(one).ToString());
        }

        [TestMethod]
        public void Meet()
        {
            Bricks one = MakeBricks("one");
            Bricks two = MakeBricks("two");

            Assert.IsTrue(one.Meet(two).IsBottom);
            Assert.AreEqual("{one}[1,1]", one.Meet(one).ToString());
            Assert.AreEqual("{one}[1,1]", one.Meet(one.Top).ToString());
        }

    }
}
