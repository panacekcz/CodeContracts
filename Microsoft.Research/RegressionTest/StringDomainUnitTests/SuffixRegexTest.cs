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
using Microsoft.Research.Regex;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.AbstractDomains.Strings;

namespace StringDomainUnitTests
{
    [TestClass]
    public class SuffixRegexTest : SuffixTestBase
    {
        [TestMethod]
        public void Match()
        {
            Suffix p = new Suffix("suffix");

            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^suffix")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("suffix\\z")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("other")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("other\\z")).ProofOutcome);
        }
        [TestMethod]
        public void Assume()
        {
            Suffix suffix = new Suffix("suffix");
            SuffixRegex pr = new SuffixRegex(suffix);
            Assert.AreEqual(suffix, pr.AssumeMatch(RegexUtil.ModelForRegex("x\\z")));
            Assert.AreEqual(new Suffix("longersuffix"), pr.AssumeMatch(RegexUtil.ModelForRegex("longersuffix\\z")));
            Assert.AreEqual(new Suffix("longersuffix"), pr.AssumeMatch(RegexUtil.ModelForRegex("longersuffix\\z|other\\z")));
            Assert.IsTrue(pr.AssumeMatch(RegexUtil.ModelForRegex("other\\z")).IsBottom);
        }
    }
}
