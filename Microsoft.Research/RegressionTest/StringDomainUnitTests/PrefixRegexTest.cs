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
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class PrefixRegexTest
    {
        Prefix.Operations<TestVariable> operations = new Prefix.Operations<TestVariable>();

        [TestMethod]
        public void MatchEmpty()
        {
            Prefix p = new Prefix("prefix");

            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("\\z")).ProofOutcome);
        }


        [TestMethod]
        public void MatchConstant()
        {
            Prefix p = new Prefix("prefix");

            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^prefix")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("prefix$")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("other")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^other")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.False, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^other$")).ProofOutcome);
        }

        [TestMethod]
        public void MatchUnion()
        {
            Prefix p = new Prefix("prefix");

            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^prefix|^other")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^(?:prefix|other)")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^(?:pre|oth)")).ProofOutcome);

            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^(?:prefixA|prefixB)")).ProofOutcome);
        }

        [TestMethod]
        public void MatchSet()
        {
            Prefix p = new Prefix("prefix");

            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^[pst][ur][ef]fix")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^[p-z][e-r][a-z]fix")).ProofOutcome);

            Assert.AreEqual(ProofOutcome.False, operations.RegexIsMatch(p, null, RegexUtil.ModelForRegex("^pr[f-z]fix")).ProofOutcome);
        }
        [TestMethod]
        public void MatchQuantifiers()
        {
            Prefix prefix = new Prefix("prefix");
            Prefix pp = new Prefix("pp");

            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(pp, null, RegexUtil.ModelForRegex("z*")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.True, operations.RegexIsMatch(pp, null, RegexUtil.ModelForRegex("p+")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(pp, null, RegexUtil.ModelForRegex("^p*$")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(pp, null, RegexUtil.ModelForRegex("^p+$")).ProofOutcome);
        }
        [TestMethod]
        public void Assume()
        {
            Prefix prefix = new Prefix("prefix");
            PrefixRegex pr = new PrefixRegex(prefix);
            Assert.AreEqual(prefix, pr.AssumeMatch(RegexUtil.ModelForRegex("^p")));
            Assert.AreEqual(new Prefix("prefixlonger"), pr.AssumeMatch(RegexUtil.ModelForRegex("^prefixlonger")));
            Assert.AreEqual(new Prefix("prefixlonger"), pr.AssumeMatch(RegexUtil.ModelForRegex("^prefixlonger|^other")));
            Assert.IsTrue(pr.AssumeMatch(RegexUtil.ModelForRegex("^other")).IsBottom);
        }
    }
}
