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
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class StringGraphRegexTest
    {
        private StringGraph top;
        private StringGraph.Operations<TestVariable> operations;

        public StringGraphRegexTest()
        {
            this.top = StringGraph.ForMax;
            operations = new StringGraph.Operations<TestVariable>();
        }

        private StringGraph SGForRegex(string regexString)
        {
            Element regex = RegexUtil.ModelForRegex(regexString);
            StringGraphRegex br = new StringGraphRegex(top);
            return br.StringGraphForRegex(regex);
        }

        private void AssertSGForRegex(string testRegexString, string expectedStringGraphString)
        {
            StringGraph stringGraph = SGForRegex(testRegexString);
            
            string regexStringGraph = stringGraph.ToString();
            Assert.AreEqual(expectedStringGraphString, regexStringGraph);
        }

        private void AssertSGIsMatch(ProofOutcome expectedResult, string stringGraphRegexString, string patternString)
        {
            StringGraph stringGraph = SGForRegex(stringGraphRegexString);

            Assert.AreEqual(expectedResult, operations.RegexIsMatch(stringGraph, null, RegexUtil.ModelForRegex(patternString)).ProofOutcome);
        }

        [TestMethod]
        public void TestSGForRegex()
        {
            AssertSGForRegex(@"^A\z", "[A]");
            AssertSGForRegex(@"^(?:A|B|C)\z", "{[A][B][C]}");
            AssertSGForRegex(@"^[ab][cd][ef]\z", "<{[a][b]}{[c][d]}{[e][f]}>");
            AssertSGForRegex(@"^(?:ab|cd){3,8}\z", "{ab,cd}[3,8]");
            AssertSGForRegex(@"^(?:ab|cd){3,8}(?:ef|gh){4,7}\z", "{ab,cd}[3,8]{ef,gh}[4,7]");
            AssertSGForRegex(@"^(?:ab|cd)?\z", "{ab,cd}[0,1]");
        }

        [TestMethod]
        public void TestSGIsMatch()
        {
            AssertSGIsMatch(ProofOutcome.True, @"^A\z", @"^A\z");
            AssertSGIsMatch(ProofOutcome.False, @"^A\z", @"^B\z");
            AssertSGIsMatch(ProofOutcome.Top, @"^[AB]\z", @"^B\z");
            AssertSGIsMatch(ProofOutcome.True, @"^[A]\z", @"^[AB]\z");
            AssertSGIsMatch(ProofOutcome.True, @"^[A]\z", @"");

            AssertSGIsMatch(ProofOutcome.Top, @"A", @"B");
            AssertSGIsMatch(ProofOutcome.False, @"^A", @"^B");
            AssertSGIsMatch(ProofOutcome.True, @"^A", @"^A");
            AssertSGIsMatch(ProofOutcome.True, @"^[a]+[b]+[c]+\z", @"^[a]+[b]+[c]+\z");

        }
    }
}
