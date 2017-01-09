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
    public class TokensTests : TokensTestBase
    {
        Tokens constant;
        //Tokens constantSet;
        //Tokens repeatedConstant;
  
        public TokensTests()
        {
            constant = operations.Constant("const");
         
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("{c{o{n{s{t{}!}.}.}.}.}.", constant.ToString());
            Assert.AreEqual("{}.", bottom.ToString());
            //TODO: VD: toString should be more sensible and not be used for tests really
            //Assert.AreEqual("", top.ToString());
        }

        [TestMethod]
        public void TestJoin()
        {
            /* Assert.AreEqual(some, something.Join(somePrefix));
             Assert.AreEqual(some, some.Join(somePrefix));
             Assert.AreEqual(some, something.Join(some));*/

            AssertAreEqual(bottom, bottom.Join(bottom));

            AssertAreEqual(constant, constant.Join(constant));
            AssertAreEqual(constant, bottom.Join(constant));
            AssertAreEqual(constant, constant.Join(bottom));

            AssertAreEqual(top, top.Join(constant));
            AssertAreEqual(top, constant.Join(top));
            AssertAreEqual(top, top.Join(top));

            Tokens longConstant = operations.Constant("constant");

            Assert.AreEqual("{c{o{n{s{t{a{n{t{}!}.}.}!}.}.}.}.}.", constant.Join(longConstant).ToString());

            Tokens otherConstant = operations.Constant("other");

            Assert.AreEqual("{c{o{n{s{t{}!}.}.}.}.o{t{h{e{r{}!}.}.}.}.}.", constant.Join(otherConstant).ToString());
        }
        
        [TestMethod]
        public void TestMeet()
        {
            Tokens longConstant = operations.Constant("constant");

            AssertAreEqual(bottom, constant.Meet(longConstant));
            AssertAreEqual(bottom, longConstant.Meet(constant));
            AssertAreEqual(constant, constant.Meet(constant));
        
            AssertAreEqual(bottom, bottom.Meet(constant));
            AssertAreEqual(bottom, constant.Meet(bottom));

            AssertAreEqual(constant, top.Meet(constant));
            AssertAreEqual(constant, constant.Meet(top));
        }

        [TestMethod]
        public void TestTop()
        {
            Assert.IsTrue(top.IsTop);
            Assert.IsFalse(constant.IsTop);
            Assert.IsFalse(bottom.IsTop);
        }

        [TestMethod]
        public void TestBottom()
        {
            Assert.IsFalse(top.IsBottom);
            Assert.IsFalse(constant.IsBottom);
            Assert.IsTrue(bottom.IsBottom);
        }

        [TestMethod]
        public void TestLessEqual()
        {
            Tokens longConstant = operations.Constant("constant");

            Assert.IsTrue(constant.LessThanEqual(constant));
            /*Assert.IsTrue(somePrefix.LessThanEqual(some));*/
            Assert.IsFalse(constant.LessThanEqual(longConstant));
            Assert.IsFalse(longConstant.LessThanEqual(constant));

            Assert.IsTrue(constant.LessThanEqual(top));

            Assert.IsTrue(bottom.LessThanEqual(bottom));
            Assert.IsTrue(bottom.LessThanEqual(constant));
            Assert.IsFalse(constant.LessThanEqual(bottom));
        }
        [TestMethod]
        public void TestEqual()
        {
            Tokens longConstant = operations.Constant("constant");
            Tokens sameConstant = operations.Constant("const");

            AssertAreNotEqual(top, bottom);
            AssertAreNotEqual(constant, longConstant);
            AssertAreNotEqual(constant, top);
            AssertAreNotEqual(constant, bottom);

            AssertAreEqual(constant, sameConstant);
            Assert.AreEqual(top, top);
            Assert.AreEqual(bottom, bottom);
        }
    }

}
