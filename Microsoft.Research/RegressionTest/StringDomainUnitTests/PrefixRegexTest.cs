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
using Microsoft.Research.Regex;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class PrefixRegexTest : PrefixTestBase
    {
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

        private void AssertPrefixForRegex(string regex, Prefix inputPrefix, Prefix expectedPrefix)
        {
            PrefixRegex pr = new PrefixRegex(inputPrefix);

            Prefix result = pr.AssumeMatch(RegexUtil.ModelForRegex(regex));
            Assert.AreEqual(expectedPrefix, result);
        }

        [TestMethod]
        public void TestPrefixForRexRegex()
        {
            // Sample regexes taken from 
            // Rex: Symbolic Regular Expression Explorer
            // M. Veanes, P. de Halleux, N. Tillmann
            // ICST 2010
            AssertPrefixForRegex(@"^(([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*\z", top, top);
            AssertPrefixForRegex(@"^[A-Za-z0-9](([ \.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\. ([A-Za-z][A-Za-z]+)*\z", top, top);
            AssertPrefixForRegex(@"^[+-]?([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?\z", top, top);
            AssertPrefixForRegex(@"^[0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4}\z", top, top);
            AssertPrefixForRegex(@"^[0-9]{2}-[0-9]{2}-[0-9]{4}\z", top, top);
            AssertPrefixForRegex(@"^\z?([0-9]{1,3},?([0-9]{3},?)*[0-9]{3}(\.[0-9]{0,2})?|[0-9]{1,3}(\.[0-9]{0,2})?|\.[0-9]{1,2}?)\z", top, top);
            AssertPrefixForRegex(@"^([A-Z]{2}|[a-z]{2} [0-9]{2} [A-Z]{1,2}|[a-z]{1,2} [0-9]{1,4})?([A-Z]{3}|[a-z]{3} [0-9]{1,4})?\z", top, top);
        }
    }
}
