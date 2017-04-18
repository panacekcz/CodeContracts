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
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class PrefixIntervalOperationsTests : StringAbstractionTestBase<PrefixInterval>
    {
        PrefixInterval.Operations<TestVariable> operations = new PrefixInterval.Operations<TestVariable>();

        [TestMethod]
        public void TestPrefixIntervalIndexOf()
        {
            Assert.AreEqual(IndexInterval.For(0), operations.IndexOf(Arg(""), Arg(""), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(0), operations.IndexOf(Arg(""), Arg(""), IndexInterval.For(0), IndexInterval.Infinity, true));
            Assert.AreEqual(IndexInterval.For(1), operations.IndexOf(Arg("abcbd"), Arg("b"), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(3), operations.IndexOf(Arg("abcbd"), Arg("b"), IndexInterval.For(0), IndexInterval.Infinity, true));

            Assert.AreEqual(IndexInterval.For(-1), operations.IndexOf(Arg("abcbd"), Arg("e"), IndexInterval.For(0), IndexInterval.Infinity, true));

            Assert.AreEqual(IndexInterval.For(-1, 3), operations.IndexOf(Arg(new PrefixInterval("abcbd", "")), Arg("b"), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(1), operations.IndexOf(Arg(new PrefixInterval(null, "abcbd")), Arg("b"), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(IndexInt.For(-1), IndexInt.Infinity), operations.IndexOf(Arg(new PrefixInterval(null, "abcbd")), Arg(new PrefixInterval(null, "b")), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(0, 1), operations.IndexOf(Arg(new PrefixInterval(null, "abcbd")), Arg(new PrefixInterval("b", "")), IndexInterval.For(0), IndexInterval.Infinity, false));
            Assert.AreEqual(IndexInterval.For(IndexInt.For(3), IndexInt.Infinity), operations.IndexOf(Arg(new PrefixInterval(null, "abcbd")), Arg(new PrefixInterval("b", "")), IndexInterval.For(0), IndexInterval.Infinity, true));

        }

    }

}
