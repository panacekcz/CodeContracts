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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;


namespace StringDomainUnitTests
{
    public class CharacterInclusionTestBase : StringAbstractionTestBase<CharacterInclusion<BitArrayCharacterSet>>
    {
        protected ICharacterClassification classification = new CompleteClassification();
        protected ICharacterSetFactory<BitArrayCharacterSet> setFactory = new BitArrayCharacterSetFactory();

        public CharacterInclusionTestBase()
        {
            SetOperations(new CharacterInclusion<BitArrayCharacterSet>.Operations<TestVariable>(classification, setFactory));
        }

        protected CharacterInclusion<BitArrayCharacterSet> Build(string mandatory, string additionalAllowed)
        {
            return new CharacterInclusion<BitArrayCharacterSet>(mandatory, mandatory + additionalAllowed, classification, setFactory);
        }

        protected WithConstants<CharacterInclusion<BitArrayCharacterSet>> BuildArg(string mandatory, string additionalAllowed)
        {
            return Arg(Build(mandatory, additionalAllowed));
        }

    }


    [TestClass]
    public class CharacterInclusionOperationsTest : CharacterInclusionTestBase
    {
        [TestMethod]
        public void TestReplaceChar()
        {
            CharInterval charD = CharInterval.For('d');
            CharInterval charE = CharInterval.For('e');

            Assert.AreEqual(Build("", "abce"), operations.Replace(Build("", "abcd"), charD, charE));
            Assert.AreEqual(Build("", "abcde"), operations.Replace(Build("", "abcd"), CharInterval.For('a', 'c'), charE));
            Assert.AreEqual(Build("", "abcd"), operations.Replace(Build("", "abcd"), CharInterval.For('x', 'z'), charE));
            Assert.AreEqual(Build("", "abcde"), operations.Replace(Build("", "abcd"), CharInterval.For('a', 'z'), charE));
            Assert.AreEqual(Build("ce", "ab"), operations.Replace(Build("cd", "ab"), charD, charE));
        }
        private void TestPadLeftRight(bool right)
        {
            CharInterval charX = CharInterval.For('x');
            Assert.AreEqual(Build("x", ""), operations.PadLeftRight(Build("", ""), IndexInterval.For(1), charX, right));
            Assert.AreEqual(Build("x", ""), operations.PadLeftRight(Build("", "x"), IndexInterval.For(1), charX, right));
            Assert.AreEqual(Build("", "yx"), operations.PadLeftRight(Build("", "y"), IndexInterval.For(10), charX, right));

            Assert.AreEqual(Build("y", ""), operations.PadLeftRight(Build("y", ""), IndexInterval.For(1), charX, right));
        }

        [TestMethod]
        public void TestPadLeftPadRight()
        {
            TestPadLeftRight(false);
            TestPadLeftRight(true);
        }

        [TestMethod]
        public void TestCombine()
        {
            Assert.AreEqual(Build("defghij", "abc"), operations.Concat(BuildArg("defgh", "ab"), BuildArg("defij", "bc")));
            Assert.AreEqual(Build("defghij", "abc"), operations.Insert(BuildArg("defgh", "ab"), IndexInterval.For(10), BuildArg("defij", "bc")));
        }

        [TestMethod]
        public void TestSubstring()
        {
            Assert.AreEqual(Build("bc", "a"), operations.Substring(Build("bc", "a"), IndexInterval.For(0), IndexInterval.Infinity));
            Assert.AreEqual(Build("", "abc"), operations.Substring(Build("bc", "a"), IndexInterval.For(1), IndexInterval.Infinity));
            Assert.AreEqual(Build("", ""), operations.Substring(Build("", ""), IndexInterval.For(0), IndexInterval.Infinity));
            Assert.IsTrue(operations.Substring(Build("", ""), IndexInterval.For(1), IndexInterval.Infinity).IsBottom);

            Assert.AreEqual(Build("", ""), operations.Substring(Build("bc", "a"), IndexInterval.For(10), IndexInterval.For(0)));
            Assert.AreEqual(Build("", "abc"), operations.Substring(Build("bc", "a"), IndexInterval.For(10), IndexInterval.For(1)));

            Assert.AreEqual(Build("a", ""), operations.Substring(Build("", "a"), IndexInterval.For(10), IndexInterval.For(1)));
        }


        [TestMethod]
        public void TestContains()
        {
            Assert.AreEqual(ProofOutcome.True, operations.Contains(BuildArg("bc", "a"), null, Arg(""), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.Contains(BuildArg("bc", "a"), null, Arg("d"), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.Contains(BuildArg("bc", "a"), null, Arg("a"), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.Contains(BuildArg("bc", "a"), null, Arg("b"), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.Contains(BuildArg("bc", "a"), null, Arg("bc"), null).ProofOutcome);

            Assert.AreEqual(ProofOutcome.True, operations.Contains(BuildArg("b", ""), null, Arg("b"), null).ProofOutcome);

            Assert.AreEqual(ProofOutcome.True, operations.Contains(BuildArg("bc", "a"), null, BuildArg("", ""), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.Contains(BuildArg("bc", "a"), null, BuildArg("", "b"), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.Contains(BuildArg("bc", "a"), null, BuildArg("d", "b"), null).ProofOutcome);
        }

        [TestMethod]
        public void TestStartEndsWith()
        {
            foreach (bool ends in new[] { true, false })
            {
                Assert.AreEqual(ProofOutcome.True, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, Arg(""), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, Arg("d"), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, Arg("a"), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, Arg("b"), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.True, operations.StartsEndsWithOrdinal(BuildArg("b", ""), null, Arg("b"), null, ends).ProofOutcome);

                Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, Arg("bc"), null, ends).ProofOutcome);

                Assert.AreEqual(ProofOutcome.True, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, BuildArg("", ""), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, BuildArg("", "b"), null, ends).ProofOutcome);
                Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(BuildArg("bc", "a"), null, BuildArg("d", "b"), null, ends).ProofOutcome);
            }
        }

        [TestMethod]
        public void TestIsNullOrEmpty()
        {
            Assert.AreEqual(ProofOutcome.True, operations.IsEmpty(Build("", ""), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.IsEmpty(Build("", "a"), null).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.IsEmpty(Build("a", ""), null).ProofOutcome);
        }


        [TestMethod]
        public void TestCompare()
        {
            Assert.AreEqual(CompareResult.Equal, operations.CompareOrdinal(BuildArg("", ""), BuildArg("", "")));

            Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(BuildArg("", "x"), BuildArg("", "x")));
            Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(BuildArg("y", "x"), BuildArg("y", "x"))); // y=y, yy>y, y<yy

            Assert.AreEqual(CompareResult.Less, operations.CompareOrdinal(BuildArg("a", ""), BuildArg("b", ""))); // a < b
            Assert.AreEqual(CompareResult.Less, operations.CompareOrdinal(BuildArg("a", "e"), BuildArg("g", "e"))); // eeeea < eeeeg
            Assert.AreEqual(CompareResult.NotEqual, operations.CompareOrdinal(BuildArg("a", "e"), BuildArg("e", "g"))); //  eeeea > eee, ea < eee


        }

    }
}
