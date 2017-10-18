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
using Microsoft.Research.Regex;


namespace StringDomainUnitTests
{

    [TestClass]
    public class CharacterInclusionRegexTest : CharacterInclusionTestBase
    {
        [TestMethod]
        public void TestIsMatch()
        {
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("a", ""), null, RegexUtil.ModelForRegex("a")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("a", ""), null, RegexUtil.ModelForRegex("a|b|c")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("abcdef", ""), null, RegexUtil.ModelForRegex("[a-f]")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("", ""), null, RegexUtil.ModelForRegex("")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("", ""), null, RegexUtil.ModelForRegex("^\\z")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("", "a"), null, RegexUtil.ModelForRegex("")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("", "a"), null, RegexUtil.ModelForRegex("^a*\\z")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("", "abcdef"), null, RegexUtil.ModelForRegex("^[a-f]*\\z")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("a", ""), null, RegexUtil.ModelForRegex("^a+\\z")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("a", ""), null, RegexUtil.ModelForRegex("^a")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(Build("a", ""), null, RegexUtil.ModelForRegex("a\\z")));
        }
        [TestMethod]
        public void TestIsNotMatch()
        {
            Assert.AreEqual(FlatPredicate.False, operations.RegexIsMatch(Build("", "a"), null, RegexUtil.ModelForRegex("b")));
            Assert.AreEqual(FlatPredicate.False, operations.RegexIsMatch(Build("", "abc"), null, RegexUtil.ModelForRegex("d|e|f")));
            Assert.AreEqual(FlatPredicate.False, operations.RegexIsMatch(Build("b", ""), null, RegexUtil.ModelForRegex("^a*\\z")));
            Assert.AreEqual(FlatPredicate.False, operations.RegexIsMatch(Build("b", ""), null, RegexUtil.ModelForRegex("^a\\z")));
        }
        [TestMethod]
        public void TestUnknownMatch()
        {
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("", "ab"), null, RegexUtil.ModelForRegex("^(?:b*|a*)\\z")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("ab", ""), null, RegexUtil.ModelForRegex("ab")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("a", "b"), null, RegexUtil.ModelForRegex("^a")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("a", "b"), null, RegexUtil.ModelForRegex("a\\z")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("", "a"), null, RegexUtil.ModelForRegex("a")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("", "a"), null, RegexUtil.ModelForRegex("[a-z]")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("x", "ab"), null, RegexUtil.ModelForRegex("a|b")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("x", "ab"), null, RegexUtil.ModelForRegex("a|y")));
            Assert.AreEqual(FlatPredicate.Top, operations.RegexIsMatch(Build("@", ".abcdefgh_"), null, RegexUtil.ModelForRegex("^[a-z0-9_]+(?:.[a-z0-9_]+)*@[a-z0-9_]+(?:.[a-z0-9_]+)+\\z")));

        }


        [TestMethod]
        public void Assume()
        {
            CharacterInclusionRegex<BitArrayCharacterSet> cir = new CharacterInclusionRegex<BitArrayCharacterSet>(top);
            Assert.AreEqual(Build("c", "").Combine(top), cir.Assume(RegexUtil.ModelForRegex("c"), true));
            Assert.AreEqual(Build("cd", "").Combine(top), cir.Assume(RegexUtil.ModelForRegex("cd"), true));
            Assert.AreEqual(top, cir.Assume(RegexUtil.ModelForRegex("c|d"), true));
            Assert.AreEqual(Build("c", "").Combine(top), cir.Assume(RegexUtil.ModelForRegex("c+"), true));
            Assert.AreEqual(Build("", "c"), cir.Assume(RegexUtil.ModelForRegex("^c*\\z"), true));
            Assert.AreEqual(Build("", "abcd"), cir.Assume(RegexUtil.ModelForRegex("^[a-d]*\\z"), true));
            Assert.AreEqual(Build("", "c"), cir.Assume(RegexUtil.ModelForRegex("[^c]"), false));
        }

        private void AssertCIForRegex(string regex, CharacterInclusion<BitArrayCharacterSet> input, CharacterInclusion<BitArrayCharacterSet> expected)
        {
            CharacterInclusionRegex<BitArrayCharacterSet> pr = new CharacterInclusionRegex<BitArrayCharacterSet>(input);

            CharacterInclusion<BitArrayCharacterSet> result = pr.Assume(RegexUtil.ModelForRegex(regex), true);

            Assert.AreEqual(expected, result);
        }


        [TestMethod]
        public void TestCIForRexRegex()
        {
            // Sample regexes taken from 
            // Rex: Symbolic Regular Expression Explorer
            // M. Veanes, P. de Halleux, N. Tillmann
            // ICST 2010
            AssertCIForRegex(@"^(([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*\z", top, Build(".@", " -0123456789;ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"));
            AssertCIForRegex(@"^[A-Za-z0-9](([ \.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\. ([A-Za-z][A-Za-z]+)*\z", top, Build(".@ ", "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"));
            AssertCIForRegex(@"^[+-]?([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?\z", top, Build("", "-+.0123456789Ee"));
            AssertCIForRegex(@"^[0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4}\z", top, Build("/", "0123456789") );
            AssertCIForRegex(@"^[0-9]{2}-[0-9]{2}-[0-9]{4}\z", top, Build("-", "0123456789"));
            AssertCIForRegex(@"^\z?([0-9]{1,3},?([0-9]{3},?)*[0-9]{3}(\.[0-9]{0,2})?|[0-9]{1,3}(\.[0-9]{0,2})?|\.[0-9]{1,2}?)\z", top, Build("", ",.0123456789"));
            AssertCIForRegex(@"^([A-Z]{2}|[a-z]{2} [0-9]{2} [A-Z]{1,2}|[a-z]{1,2} [0-9]{1,4})?([A-Z]{3}|[a-z]{3} [0-9]{1,4})?\z", top, Build("", " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"));
        }
    }
}
