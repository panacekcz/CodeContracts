// CodeContracts
// 
// Copyright (c) Microsoft Corporation
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class TokensTests : TokensTestBase
    {
        Tokens constant;
  
        public TokensTests()
        {
            constant = operations.Constant("const");
         
        }

        [TestMethod]
        public void TestToString()
        {
            Assert.AreEqual("{c{o{n{s{t{}!}.}.}.}.}.", constant.ToString());
            Assert.AreEqual("{}.", bottom.ToString());
        }

        [TestMethod]
        public void TestParse()
        {
            Tokens t = ParseTokens("{c{o{n{s{t{}!}.}.}.}.}.");
            Assert.AreEqual("{c{o{n{s{t{}!}.}.}.}.}.", t.ToString());

            t = ParseTokens("{a{}!b{}!c{}!}.");
            Assert.AreEqual("{a{}!b{}!c{}!}.", t.ToString());

            t = ParseTokens("{a*}!");
            Assert.AreEqual("{a*}!", t.ToString());
        }


        [TestMethod]
        public void TestJoin()
        {
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

            Assert.AreEqual("{a*b*}!", ParseTokens("{a*}!").Join(ParseTokens("{b*}!")).ToString());
            Assert.AreEqual("{a*}!", ParseTokens("{a*}!").Join(ParseTokens("{a{a{}!}.}.")).ToString());
            Assert.AreEqual("{a*b{c{}!}.}!", ParseTokens("{a*}!").Join(ParseTokens("{a{b{c{}!}.}.}.")).ToString());
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

            AssertAreEqual(constant, constant.Meet(top));

            Assert.AreEqual("{a{b{c{d{}!}.}.}.}.", ParseTokens("{a{b{c{d{}!}.}.}!}.").Meet(ParseTokens("{a{b{c{d{}!}.}!}.}.")).ToString());
            Assert.AreEqual("{a{}!}.", ParseTokens("{a{b{c{d{}!}.}!}!}.").Meet(ParseTokens("{a{b{c{d{}.}.}.}!}.")).ToString());
            Assert.AreEqual("{a{}!}.", ParseTokens("{a{}!b{}!}.").Meet(ParseTokens("{a{}!c{}!}.")).ToString());
            Assert.AreEqual("{a{}!}.", ParseTokens("{a{}!b{}!}.").Meet(ParseTokens("{a*}!")).ToString());
            Assert.AreEqual("{a{}!b{}!}.", ParseTokens("{a{}!b{}!}.").Meet(ParseTokens("{a*b*}!")).ToString());

            Assert.AreEqual("{b*}!", ParseTokens("{a*b*}!").Meet(ParseTokens("{b*c*}!")).ToString());
            Assert.AreEqual("{b*d{}!}.", ParseTokens("{a*b*d{}!}.").Meet(ParseTokens("{b*c*d{}!}.")).ToString());
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
            Assert.IsFalse(constant.LessThanEqual(longConstant));
            Assert.IsFalse(longConstant.LessThanEqual(constant));

            Assert.IsTrue(constant.LessThanEqual(top));

            Assert.IsTrue(bottom.LessThanEqual(bottom));
            Assert.IsTrue(bottom.LessThanEqual(constant));
            Assert.IsFalse(constant.LessThanEqual(bottom));


            Assert.IsTrue(ParseTokens("{a*}!").LessThanEqual(ParseTokens("{a{a*}!}!")));
            Assert.IsTrue(ParseTokens("{a{a*}.}!").LessThanEqual(ParseTokens("{a*}!")));

            Assert.IsTrue(ParseTokens("{a*}!").LessThanEqual(ParseTokens("{a*b*}!")));
            Assert.IsFalse(ParseTokens("{a*b*}!").LessThanEqual(ParseTokens("{a*}!")));
            Assert.IsTrue(ParseTokens("{a{a*}.}!").LessThanEqual(ParseTokens("{a*b*}!")));
            Assert.IsTrue(ParseTokens("{a{}!}.").LessThanEqual(ParseTokens("{a*}!")));
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

            Assert.AreEqual(constant, sameConstant);
            Assert.AreEqual(top, top);
            Assert.AreEqual(bottom, bottom);
        }
    }

}
