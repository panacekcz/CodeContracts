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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Research.AbstractDomains.Strings;

namespace StringDomainUnitTests
{

    [TestClass]
    public class StringGraphTests
    {

        [TestMethod]
        public void TestConstant()
        {
            StringGraph node = StringGraph.ForString("constant");
            Assert.AreEqual("<[c][o][n][s][t][a][n][t]>", node.ToString());
        }



        [TestMethod]
        public void TestComparison()
        {
            StringGraph oneConst = StringGraph.ForString("one");
            StringGraph top = oneConst.Top;
            StringGraph onePrefix = StringGraph.ForConcat(oneConst, top);


            Assert.IsTrue(oneConst.LessThanEqual(top));
            Assert.IsTrue(onePrefix.LessThanEqual(top));

            Assert.IsTrue(top.LessThanEqual(top));
            Assert.IsTrue(oneConst.LessThanEqual(oneConst));
            Assert.IsTrue(onePrefix.LessThanEqual(onePrefix));

            Assert.IsFalse(top.LessThanEqual(oneConst));
            Assert.IsFalse(top.LessThanEqual(onePrefix));

        }

        [TestMethod]
        public void ToStringTest()
        {
            Assert.AreEqual("[c]", StringGraph.ForChar('c').ToString());
            Assert.AreEqual("_|_", StringGraph.ForBottom.ToString());
            Assert.AreEqual("T", StringGraph.ForMax.ToString());

            StringGraph[] chars = new[] { StringGraph.ForChar('a'), StringGraph.ForChar('b') };

            Assert.AreEqual("<[a][b]>", StringGraph.ForConcat(chars).ToString());
            Assert.AreEqual("{[a][b]}", StringGraph.ForUnion(chars).ToString());
        }

        [TestMethod]
        public void Intersection()
        {
            StringGraph cstC1 = StringGraph.ForChar('c'), cstC2 = StringGraph.ForChar('c');

            StringGraph c = cstC1.Meet(cstC2);
            Assert.AreEqual("[c]", c.ToString());

            StringGraph max = StringGraph.ForMax;

            c = cstC1.Meet(max);
            Assert.AreEqual("[c]", c.ToString());

            StringGraph cstD = StringGraph.ForChar('d');

            c = cstC1.Meet(cstD);
            Assert.AreEqual("_|_", c.ToString());

        }

    }
}
