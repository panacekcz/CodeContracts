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
using Microsoft.Research.AbstractDomains.Strings.PrefixTree;

namespace StringDomainUnitTests
{
    public abstract class TokensTestBase : StringAbstractionTestBase<Tokens>
    {
        protected Tokens.Operations<TestVariable> operations = new Tokens.Operations<TestVariable>();
        protected Tokens bottom, top;

        public TokensTestBase()
        {
            top = operations.Top;
            bottom = top.Bottom;
        }

        protected void AssertAreEqual(Tokens expected, Tokens actual)
        {
            //expected.Equals
            Assert.AreEqual(expected, actual);
        }
        protected void AssertAreNotEqual(Tokens expected, Tokens actual)
        {
            Assert.AreNotEqual(expected, actual);
        }

    }

    [TestClass]
    public class TokensOperationsTests : TokensTestBase
    {



        [TestMethod]
        public void TestConcat()
        {
            Tokens leftConstant = operations.Constant("left");
            Tokens rightConstant = operations.Constant("right");

            Tokens leftRightConstant = operations.Constant("leftright");

            AssertAreEqual(leftRightConstant, operations.Concat(Arg(leftConstant), Arg(rightConstant)));
        }

        [TestMethod]
        public void TestIsEmpty()
        {
            Tokens constant = operations.Constant("const");
            Tokens empty = operations.Constant("");

            Assert.AreEqual(FlatPredicate.False, operations.IsEmpty(constant, null));
            Assert.AreEqual(FlatPredicate.True, operations.IsEmpty(empty, null));
            Assert.AreEqual(FlatPredicate.Top, operations.IsEmpty(empty.Join(constant), null));
        }

        [TestMethod]
        public void TestEquals()
        {
            Tokens constant = operations.Constant("const");
            Tokens empty = operations.Constant("");

            Assert.AreEqual(FlatPredicate.False, operations.Equals(Arg(constant), null, Arg(empty), null));
            Assert.AreEqual(FlatPredicate.True, operations.Equals(Arg(constant), null, Arg(constant), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Equals(Arg(constant), null, Arg(empty.Join(constant)), null));
        }

        [TestMethod]
        public void TestCompare()
        {
            Tokens constant = operations.Constant("const");
            Tokens empty = operations.Constant("");

            Assert.AreEqual(CompareResult.Greater, operations.CompareOrdinal(Arg(constant), Arg(empty)));
            Assert.AreEqual(CompareResult.Equal, operations.CompareOrdinal(Arg(constant), Arg(constant)));
            Assert.AreEqual(CompareResult.GreaterEqual, operations.CompareOrdinal(Arg(constant), Arg(empty.Join(constant))));
            Assert.AreEqual(CompareResult.LessEqual, operations.CompareOrdinal(Arg(empty.Join(constant)), Arg(constant)));
            Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(Arg(empty.Join(constant)), Arg(empty.Join(constant))));
        }

        [TestMethod]
        public void TestReplaceChar()
        {
            Tokens constant = operations.Constant("const");
            AssertAreEqual(operations.Constant("coast"), operations.Replace(constant, CharInterval.For('n'), CharInterval.For('a')));
            AssertAreEqual(operations.Constant("const"), operations.Replace(constant, CharInterval.For('x'), CharInterval.For('y')));
        }



        [TestMethod]
        public void TestStartsWith()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");

            Assert.AreEqual(ProofOutcome.True, operations.StartsWithOrdinal(Arg(longConstant), null, Arg(constant), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.StartsWithOrdinal(Arg(constant), null, Arg(longConstant), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.StartsWithOrdinal(Arg(constant), null, Arg(top), null).ProofOutcome);
        }

        [TestMethod]
        public void TestReplaceString()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");
            Tokens otherConstant = operations.Constant("other");
            Tokens anotherConstant = operations.Constant("another");

            Assert.AreEqual(constant, operations.Replace(Arg(constant), Arg(otherConstant), Arg(anotherConstant)));
        }
        [TestMethod]
        public void TestContains()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");

            Assert.AreEqual(FlatPredicate.True, operations.Contains(Arg(longConstant), null, Arg(constant), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(longConstant), null, Arg(top), null));
            Assert.AreEqual(FlatPredicate.False, operations.Contains(Arg(constant), null, Arg(longConstant), null));
        }
#if false
        [TestMethod]
    public void TestPrefixSubstring()
    {

      Assert.AreEqual(new Prefix("ePre"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(4)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(7)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(100)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.Infinity));

      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(10), IndexInterval.For(1)));
      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(10), IndexInterval.Infinity));

      Assert.AreEqual(somePrefix, operations.Substring(somePrefix, IndexInterval.For(0), IndexInterval.Infinity));
      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(0), IndexInterval.For(0)));
    }
    [TestMethod]
    public void TestPrefixRemove()
    {
      Assert.AreEqual(new Prefix("somfix"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(4)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(7)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(100)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.Infinity));

      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(10), IndexInterval.For(1)));
      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(10), IndexInterval.Infinity));

      Assert.AreEqual(top, operations.Remove(somePrefix, IndexInterval.For(0), IndexInterval.Infinity));
      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(0), IndexInterval.For(0)));
    }
#endif
        [TestMethod]
        public void TestPadLeftRight()
        {
            Tokens constant = operations.Constant("const");
            Assert.AreEqual(constant, operations.PadLeftRight(constant, IndexInterval.For(1), CharInterval.For(' '), false));
            Assert.AreEqual(constant, operations.PadLeftRight(constant, IndexInterval.For(1), CharInterval.For(' '), true));
        }

        [TestMethod]
        public void TestInsert()
        {

            Assert.AreEqual(operations.Constant("someOtherString"), operations.Insert(Arg(operations.Constant("someString")), IndexInterval.For(4), Arg("Other")));

        }


    }

}
