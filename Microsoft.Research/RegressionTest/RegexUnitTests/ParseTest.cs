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

using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex;

namespace RegexUnitTests
{
    [TestClass]
    public class ParseTest
    {
        public void Test(string input, string output = null)
        {
            Element e = RegexParser.Parse(input);
            Assert.AreEqual<string>(output ?? input, e.ToString());
        }

        [TestMethod]
        public void ParseChars()
        {
            Test("");
            Test("abcdefgh");
            Test("a");
        }

        [TestMethod]
        public void ParseNamed()
        {
            Test("(?<name>inner)\\k<name>");
            Test("(?<4>inner)\\k<4>");
        }

        [TestMethod]
        public void ParseCaptures()
        {
            Test("(a)(b)(c)\\1\\2\\3", "(?<1>a)(?<2>b)(?<3>c)\\k<1>\\k<2>\\k<3>");
            Test("(((a)))", "(?<1>(?<2>(?<3>a)))");
        }

        [TestMethod]
        public void ParseAnchors()
        {
            Test("^abc$");
            Test("^^$");
            Test("\\z");
        }


        [TestMethod]
        public void ParseQuantifiers()
        {
            Test("a*");
            Test("a+");
            Test("a?");
            Test("a{3}");
            Test("a{2,}");

            Test("a{2,3}");

            Test("a*?");
            Test("a+?");
            Test("a??");
            Test("a{3}?");
            Test("a{2,}?");
            Test("a{2,3}?");
        }

        [TestMethod]
        public void ParseFakeQuantifiers()
        {
            Test("{,3}?");
            Test("a{,3}");
            Test("a{,3}?");
            Test("a*{,3}");
            Test("a*{,3}?");
            Test("a{x");
            Test("a{1x");
            Test("a{2,x");
            Test("a{2,3x");
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseNestedQuantifiersStarStar()
        {
            RegexParser.Parse("a**");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseNestedQuantifiersStarPlus()
        {
            RegexParser.Parse("a*+");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseNestedQuantifiersStarBrace()
        {
            RegexParser.Parse("a*{2,3}");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseEmptyQuantifierStar()
        {
            RegexParser.Parse("*");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseEmptyQuantifierPlus()
        {
            RegexParser.Parse("+");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseEmptyQuantifierQuestion()
        {
            RegexParser.Parse("?");
        }
        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void ParseEmptyQuantifierBrace()
        {
            RegexParser.Parse("{1}");
        }


        [TestMethod]
        public void ParseAlternations()
        {
            Test("ab|cd");
            Test("a*b*|c*d*");
            Test("|a||");
        }
        [TestMethod]
        public void ParseBackslashB()
        {
            Test("\\b[\\b]");
        }

        [TestMethod]
        public void ParseSetSubtraction()
        {
            Test("[a-z-[k-t]]");
        }

        [TestMethod]
        public void ParseAssertions()
        {
            Test("(?!)");
            // Lookarounds
            Test("(?=abc)");
            Test("(?!abc)");
            Test("(?<=abc)");
            Test("(?<!abc)");
        }

        [TestMethod]
        public void ParseComments()
        {
            Test("(?#comment)");
            Test("(?#)");
            Test("(?#()");
            Test("(?#(?#c)");
            Test("(?#\\(\\)");
        }

        [TestMethod]
        public void ParseEscape()
        {
            Test("\u5A6B", "\u5A6B");
            Test("\\u5A6B", "\\u5A6B");
            Test("\\u5a6b", "\\u5A6B");
            Test("\\.\\$\\^\\{\\[\\(\\|\\)\\*\\+\\?\\\\");
            Test("\\u0031\\u0041\\u0061");
            Test("\\x5A");
            Test("\\x5b", "\\x5B");
            Test("\\cA\\cZ");
        }

        [TestMethod]
        public void ParseCharacterSet()
        {
            Test("[a]");
            Test("[abcd]");
            Test("[^s]");
            Test("[[]");
            Test("[]]");
            Test("[^^]");
            Test("[^]]");
            Test("[-]");
            Test("[^-]");
            Test("[0-]]");//not a range
        }
        [TestMethod]
        public void ParseRange()
        {
            Test("[a-z]");
            Test("[a-zA-Z0-9_]");
            Test("[a-zA-Z0-9\\u005F]");

        }
        [TestMethod]
        public void ParseRangeEscape()
        {
            Test("[\\s]");
            Test("[\\s\\w\\d]");
        }
    }
}
