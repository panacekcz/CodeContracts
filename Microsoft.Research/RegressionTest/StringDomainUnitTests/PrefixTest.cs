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
    public class PrefixTests : PrefixTestBase
    {
        Prefix somePrefix = new Prefix("somePrefix");
        Prefix some = new Prefix("some");
        Prefix something = new Prefix("something");

        [TestMethod]
        public void TestPrefixJoin()
        {
            Assert.AreEqual(some, something.Join(somePrefix));
            Assert.AreEqual(some, some.Join(somePrefix));
            Assert.AreEqual(some, something.Join(some));

            Assert.AreEqual(somePrefix, bottom.Join(somePrefix));
            Assert.AreEqual(somePrefix, somePrefix.Join(bottom));

            Assert.AreEqual(top, top.Join(somePrefix));
            Assert.AreEqual(top, somePrefix.Join(top));
        }

        [TestMethod]
        public void TestPrefixMeet()
        {
            Assert.AreEqual(bottom, something.Meet(somePrefix));
            Assert.AreEqual(somePrefix, some.Meet(somePrefix));
            Assert.AreEqual(something, something.Meet(some));

            Assert.AreEqual(bottom, bottom.Meet(somePrefix));
            Assert.AreEqual(bottom, somePrefix.Meet(bottom));

            Assert.AreEqual(somePrefix, top.Meet(somePrefix));
            Assert.AreEqual(somePrefix, somePrefix.Meet(top));
        }

        [TestMethod]
        public void TestTop()
        {
            Assert.IsTrue(top.IsTop);
            Assert.IsFalse(somePrefix.IsTop);
            Assert.IsFalse(bottom.IsTop);
        }

        [TestMethod]
        public void TestBottom()
        {
            Assert.IsFalse(top.IsBottom);
            Assert.IsFalse(somePrefix.IsBottom);
            Assert.IsTrue(bottom.IsBottom);
        }

        [TestMethod]
        public void TestPrefixCompare()
        {
            Assert.IsTrue(somePrefix.LessThanEqual(somePrefix));
            Assert.IsTrue(somePrefix.LessThanEqual(some));
            Assert.IsFalse(some.LessThanEqual(somePrefix));
            Assert.IsFalse(somePrefix.LessThanEqual(something));

            Assert.IsTrue(some.LessThanEqual(top));

            Assert.IsTrue(bottom.LessThanEqual(bottom));
            Assert.IsTrue(bottom.LessThanEqual(somePrefix));
            Assert.IsFalse(somePrefix.LessThanEqual(bottom));
        }

        [TestMethod]
        public void TestPrefixEqual()
        {
            Assert.AreNotEqual(top, bottom);
            Assert.AreNotEqual(some, somePrefix);
            Assert.AreNotEqual(some, top);
            Assert.AreNotEqual(some, bottom);
        }
    }

}
