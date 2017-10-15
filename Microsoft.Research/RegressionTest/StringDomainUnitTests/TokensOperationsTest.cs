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
using Microsoft.Research.AbstractDomains.Strings.TokensTree;

namespace StringDomainUnitTests
{
    public abstract class TokensTestBase : StringAbstractionTestBase<Tokens>
    {
        public TokensTestBase()
        {
            SetOperations(new Tokens.Operations<TestVariable>());
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

        /// <summary>
        /// Creates a Tokens domain element from a string description.
        /// </summary>
        /// <param name="tokensAsString">String format of the tokens tree.</param>
        /// <returns>The specified Tokens element.</returns>
        protected Tokens ParseTokens(string tokensAsString)
        {
            TokensTreeParser parser = new TokensTreeParser();
            return parser.ParseTokens(tokensAsString);
        }

    }

    [TestClass]
    public class TokensOperationsTests : TokensTestBase
    {


        /// <summary>
        /// Tests the Concat operation on Tokens.
        /// </summary>
        [TestMethod]
        public void TestConcat()
        {
            Tokens leftConstant = operations.Constant("left");
            Tokens rightConstant = operations.Constant("right");

            Tokens leftRightConstant = operations.Constant("leftright");

            AssertAreEqual(leftRightConstant, operations.Concat(Arg(leftConstant), Arg(rightConstant)));
            //TODO: non-constant strings
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

            Assert.AreEqual("{a{}!e{}!f{}!}.", operations.Replace(operations.Constant("a"), CharInterval.For('a','c'), CharInterval.For('e', 'f')).ToString());
        }

        [TestMethod]
        public void TestLength()
        {
            Assert.AreEqual(IndexInterval.For(5), operations.GetLength(operations.Constant("const")));
            Assert.AreEqual(IndexInterval.For(IndexInt.For(1), IndexInt.Infinity), operations.GetLength(ParseTokens("{a*b{}!}.")));
            Assert.AreEqual(IndexInterval.For(IndexInt.For(1), IndexInt.For(3)), operations.GetLength(ParseTokens("{a{b{c{}!}.}!}.")));
            Assert.AreEqual(IndexInterval.For(IndexInt.For(3), IndexInt.Infinity), operations.GetLength(ParseTokens("{a{b{c{}!}.d*}.}.")));
        }

        [TestMethod]
        public void TestStartsWith()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");

            Assert.AreEqual(ProofOutcome.True, operations.StartsEndsWithOrdinal(Arg(longConstant), null, Arg(constant), null, false).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(Arg(constant), null, Arg(longConstant), null, false).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(Arg(constant), null, Arg(top), null, false).ProofOutcome);

            Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a{b{}!c{}!}.}.")), null, Arg("ab"), null, false).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a{b{}!c{}!}.}.")), null, Arg("ad"), null, false).ProofOutcome);

            Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a*b{}!}.")), null, Arg("b"), null, false).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a*b{}!}.")), null, Arg("a"), null, false).ProofOutcome);
        }
        [TestMethod]
        public void TestEndsWith()
        {
            Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a*b{}!}.")), null, Arg("a"), null, true).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.StartsEndsWithOrdinal(Arg(ParseTokens("{a*b{}!}.")), null, Arg("b"), null, true).ProofOutcome);
        }

        [TestMethod]
        public void TestReplaceString()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");
            Tokens otherConstant = operations.Constant("other");
            Tokens anotherConstant = operations.Constant("another");

            Assert.AreEqual(constant, operations.Replace(Arg(constant), Arg(otherConstant), Arg(anotherConstant)));
            Assert.AreEqual("{a{n{t{}!}.}.c{o{n{s{t*}.}.}.}.o{t{h{e{r*}.}.}.}.}!", operations.Replace(Arg(longConstant), Arg(constant), Arg(otherConstant)).ToString());
            Assert.AreEqual("{a{n*}.c{o{n{s*}.}.}.t*x*}!", operations.Replace(Arg(longConstant), Arg("t"), Arg("x")).ToString());
        }
        [TestMethod]
        public void TestContains()
        {
            Tokens constant = operations.Constant("const");
            Tokens longConstant = operations.Constant("constant");

            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(longConstant), null, Arg(top), null));
            Assert.AreEqual(FlatPredicate.False, operations.Contains(Arg(ParseTokens("{a{}!d{}!}.")), null, Arg(ParseTokens("{b{}!c{}!}.")), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(ParseTokens("{a{}!d{}!}.")), null, Arg(ParseTokens("{b{}!d{}!}.")), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(ParseTokens("{a{}!d{}!}.")), null, Arg(ParseTokens("{a*}!")), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(ParseTokens("{a*}!")), null, Arg(ParseTokens("{a*}!")), null));
            Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(ParseTokens("{a*}!")), null, Arg(ParseTokens("{b*}!")), null));
            Assert.AreEqual(FlatPredicate.False, operations.Contains(Arg(ParseTokens("{a*}!")), null, Arg(ParseTokens("{b{}!}.")), null));
            Assert.AreEqual(FlatPredicate.False, operations.Contains(Arg(constant), null, Arg(longConstant), null));
            Assert.AreEqual(FlatPredicate.True, operations.Contains(Arg(longConstant), null, Arg(constant), null));
        }

        [TestMethod]
        public void TestSubstring()
        {
            Assert.AreEqual(operations.Constant("stan"), operations.Substring(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.For(4)));
            Assert.AreEqual(operations.Constant("stant"), operations.Substring(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.Infinity));
            Assert.AreEqual("{a*c*n*o*s*t*}!", operations.Substring(operations.Constant("constant"), IndexInterval.UnknownNonNegative, IndexInterval.For(4)).ToString());
            Assert.AreEqual("{a*c*n*o*s*t*}!", operations.Substring(operations.Constant("constant"), IndexInterval.UnknownNonNegative, IndexInterval.Infinity).ToString());
            Assert.AreEqual("{s{t{a{n{t{}!}!}!}!}!}!", operations.Substring(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.UnknownNonNegative).ToString());
            Assert.AreEqual("{s{t{a{}!}!}.}.", operations.Substring(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.For(2,3)).ToString());

            Assert.AreEqual("{b{c{}!}.f{g{}!}.}.", operations.Substring(ParseTokens("{a{b{c{d{}!}.}.}.e{f{g{h{}!}.}.}.}."), IndexInterval.For(1), IndexInterval.For(2)).ToString());
            Assert.AreEqual("{b{c{}!}!f{g{}!}!}.", operations.Substring(ParseTokens("{a{b{c{d{}!}.}.}.e{f{g{h{}!}.}.}.}."), IndexInterval.For(1), IndexInterval.For(1,2)).ToString());
            Assert.AreEqual("{a*b{c{d*}!}!e*f{g{h*}!}!}!", operations.Substring(ParseTokens("{a{b{c{d*}.}.}.e{f{g{h*}.}.}.}."), IndexInterval.For(5), IndexInterval.For(6)).ToString());

        }
        [TestMethod]
        public void TestRemove()
        {
            Assert.AreEqual(operations.Constant("con"), operations.Remove(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.Infinity));
            Assert.AreEqual("{a*c*n*o*s*t*}!", operations.Remove(operations.Constant("constant"), IndexInterval.UnknownNonNegative, IndexInterval.For(4)).ToString());
            Assert.AreEqual("{a*c*n*o*s*t*}!", operations.Remove(operations.Constant("constant"), IndexInterval.UnknownNonNegative, IndexInterval.UnknownNonNegative).ToString());
            Assert.AreEqual("{c{o{n{s{t{a{n{t{}!}!}!}!}!}!}!}!}!", operations.Remove(operations.Constant("constant"), IndexInterval.UnknownNonNegative, IndexInterval.Infinity).ToString());
            Assert.AreEqual("{a*c{o{n*}.}.n*s*t*}!", operations.Remove(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.UnknownNonNegative).ToString());
            Assert.AreEqual("{c{o{n*}.}.t{}!}.", operations.Remove(operations.Constant("constant"), IndexInterval.For(3), IndexInterval.For(4)).ToString());
        }

        [TestMethod]
        public void TestPadLeftRight()
        {
            Tokens constant = operations.Constant("const");
            Assert.AreEqual(constant, operations.PadLeftRight(constant, IndexInterval.For(1), CharInterval.For(' '), false));
            Assert.AreEqual(constant, operations.PadLeftRight(constant, IndexInterval.For(1), CharInterval.For(' '), true));
        }


        [TestMethod]
        public void TestCharAt()
        {
            Assert.AreEqual(CharInterval.For('a'), operations.GetCharAt(ParseTokens("{a*}!"), IndexInterval.Unknown));
            Assert.AreEqual(CharInterval.For('c','t'), operations.GetCharAt(operations.Constant("const"), IndexInterval.Unknown));
            Assert.AreEqual(CharInterval.For('n'), operations.GetCharAt(operations.Constant("const"), IndexInterval.For(2)));

            Assert.AreEqual(CharInterval.For('c', 'd'), operations.GetCharAt(ParseTokens("{a{c*}.b{d*}.}!"), IndexInterval.For(3)));
            Assert.AreEqual(CharInterval.For('a', 'd'), operations.GetCharAt(ParseTokens("{a{c*}.b{d*}.}!"), IndexInterval.For(3,4)));
        }


        [TestMethod]
        public void TestInsert()
        {
            AssertString("{O{t{h{e{r*}.}.}.}.S{t{r{i{n{g{}!}.}.}.}.}.s{o{m{e*}.}.}.}.", operations.Insert(Arg(operations.Constant("someString")), IndexInterval.For(4), Arg("Other")));
            AssertString("{a*o{t{h{e{r*}.}.}.}.}!", operations.Insert(Arg(operations.Constant("aaaa")), IndexInterval.Unknown, Arg("other")));
            AssertString("{a{b*}.c{d*}.e{f*}.}!", operations.Insert(Arg(ParseTokens("{a{b*}.c{d*}.}!")), IndexInterval.For(88), Arg("ef")));
            AssertString("{a*b*c*d*e{f*}.}!", operations.Insert(Arg(ParseTokens("{a{b*}.c{d*}.}!")), IndexInterval.For(89), Arg("ef")));

        }
    }
}
