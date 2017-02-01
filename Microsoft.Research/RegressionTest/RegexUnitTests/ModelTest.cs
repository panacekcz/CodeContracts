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

using Microsoft.Research.Regex.Model;
using Microsoft.Research.Regex;

namespace RegexUnitTests
{
    [TestClass]
    public class ModelTest
    {
        public void Test(string input, string output)
        {
            Element e = RegexUtil.ModelForRegex(input);
            Assert.AreEqual<string>(output, e.ToString());
        }

        [TestMethod]
        public void ParseChars()
        {
            Test("", "concat()");
            Test("abcdefgh", "concat(char(61),char(62),char(63),char(64),char(65),char(66),char(67),char(68))");
            Test("a", "char(61)");
        }

        [TestMethod]
        public void ParseNamed()
        {
            Test("(?<name>inner)\\k<name>", "unknown");
        }

        [TestMethod]
        public void ParseAnchors()
        {
            Test("^abc$", "concat(begin()char(61)char(62)char(63)end())");
            Test("^^$", "begin()begin()end())");
            Test("^abc\\z", "concat(begin()char(61)char(62)char(63)end())");
            Test("^^\\z", "begin()begin()end())");
        }
        [TestMethod]
        public void ParseQuantifiers()
        {
            Test("a*", "loop(char(61),0,inf)");
            Test("a*?", "loop(char(61),0,inf)");
            Test("a{1,3}?", "loop(char(61),1,3)");
        }
        [TestMethod]
        public void ParseAlternations()
        {
            Test("ab|cd", "union(concat(char(61)char(62))concat(char(63)char(64)))");
            Test("a*b*|c*d*", "union(concat(loop(char(61),0,inf)loop(char(62),0,inf))concat(loop(char(63),0,inf)loop(char(64),0,inf)))");
            Test("|a||", "union(concat()char(61)concat()concat())");
        }
        [TestMethod]
        public void ParseBackslashB()
        {
            Test("\\b[\\b]", "concat(unknown(),char())");
        }

        [TestMethod]
        public void ParseSetSubtraction()
        {
            Test("[a-z-[k-t]]", "char(61-6A,75-7A)");
        }

        [TestMethod]
        public void ParseAssertions()
        {
            Test("(?!)", "union()");
        }

        [TestMethod]
        public void ParseComments()
        {
            Test("(?#comment)", "concat()");
            Test("(?#)", "concat()");
            Test("(?#()", "concat()");
            Test("(?#(?#c)", "concat()");
            Test("(?#\\(\\)", "concat()");
        }

        [TestMethod]
        public void ParseEscape()
        {
            Test("\u5A6B", "char(5A6B)");
            Test("\\u5A6B", "char(5A6B)");
            Test("\\u5a6b", "char(5A6B)");
            Test("\\.\\$\\^\\{\\[\\(\\|\\)\\*\\+\\?\\\\", "concat(char(2E)char(24)char(5E)char(7B)char(5B)char(28)char(7C)char(29)char(2A)char(2B)char(3F)char(5C))");
            Test("\\u0031\\u0041\\u0061", "concat(char(31)char(41)char(61))");
            Test("\\x5A", "char(5A)");
            Test("\\x5b", "char(5B)");
            Test("\\cA\\cZ", "concat(char(1)char(1A))");
        }

        [TestMethod]
        public void ParseCharacterSet()
        {
            Test("[a]", "char(61)");
            Test("[abcd]", "char(61-64)");
            Test("[^s]", "char(0-72,74-FFFF)");
            Test("[[]", "char(5B)");
            Test("[]]", "char(5D)");
            Test("[^^]", "char(0-5D,5F-FFFF)");
            Test("[^]]", "char(0-5C,5E-FFFF)");
            Test("[-]", "char(2D)");
            Test("[^-]", "char(0-2C,2E-FFFF)");
            Test("[0-]]", "concat(char(30,2D)char(5D))");//not a range
        }
        [TestMethod]
        public void ParseRange()
        {
            Test("[a-z]", "char(61-7A)");
            Test("[a-zA-Z0-9_]", "char(61-7A)");
            Test("[a-zA-Z0-9\\u005F]", "char(61-7A)");

        }
        [TestMethod]
        public void ParseRangeEscape()
        {
            Test("[\\s]", "char()");
            Test("[\\s\\w\\d]", "char()");
        }
    }
}
