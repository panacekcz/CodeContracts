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


using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex;

namespace RegexUnitTests
{

    [TestClass]
    public class CharactersTest
    {
        [TestMethod]
        public void MatchSubtractedRange()
        {
            var regex = RegexParser.Parse("[a-z-[c-d]]") as SingleElement;

            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsTrue(regex.MustMatch('b'));
            Assert.IsTrue(regex.MustMatch('e'));
            Assert.IsTrue(regex.MustMatch('z'));
            Assert.IsFalse(regex.CanMatch('c'));
            Assert.IsFalse(regex.CanMatch('d'));
        }

        [TestMethod]
        public void MatchCharacterRange()
        {
            var regex = RegexParser.Parse("[a-z]") as SingleElement;

            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsTrue(regex.MustMatch('z'));
            Assert.IsFalse(regex.CanMatch('A'));
        }

        [TestMethod]
        public void MatchCharacter()
        {
            var regex = RegexParser.Parse("a") as SingleElement;

            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsFalse(regex.CanMatch('b'));
            Assert.IsFalse(regex.CanMatch('A'));
        }

        [TestMethod]
        public void MatchWordSet()
        {
            var regex = RegexParser.Parse("\\w") as SingleElement;

            Assert.IsTrue(regex.MustMatch('0'));
            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsTrue(regex.MustMatch('z'));
            Assert.IsTrue(regex.MustMatch('A'));
            Assert.IsTrue(regex.MustMatch('Z'));
            Assert.IsTrue(regex.MustMatch('_'));

            Assert.IsFalse(regex.CanMatch('.'));
            Assert.IsFalse(regex.CanMatch('!'));
        }

        [TestMethod]
        public void MatchDigitSet()
        {
            var regex = RegexParser.Parse("\\d") as SingleElement;

            Assert.IsTrue(regex.MustMatch('0'));
            Assert.IsTrue(regex.MustMatch('9'));

            Assert.IsFalse(regex.CanMatch('a'));
            Assert.IsFalse(regex.CanMatch('A'));
            Assert.IsFalse(regex.CanMatch('.'));
            Assert.IsFalse(regex.CanMatch('!'));
        }


        [TestMethod]
        public void MatchWhitespaceSet()
        {
            var regex = RegexParser.Parse("\\s") as SingleElement;

            Assert.IsTrue(regex.MustMatch(' '));

            Assert.IsFalse(regex.CanMatch('0'));
            Assert.IsFalse(regex.CanMatch('a'));
            Assert.IsFalse(regex.CanMatch('A'));
            Assert.IsFalse(regex.CanMatch('.'));
            Assert.IsFalse(regex.CanMatch('!'));
        }

        [TestMethod]
        public void MatchWordSetNegated()
        {
            var regex = RegexParser.Parse("\\W") as SingleElement;

            Assert.IsFalse(regex.CanMatch('0'));
            Assert.IsFalse(regex.CanMatch('a'));
            Assert.IsFalse(regex.CanMatch('z'));
            Assert.IsFalse(regex.CanMatch('A'));
            Assert.IsFalse(regex.CanMatch('Z'));
            Assert.IsFalse(regex.CanMatch('_'));

            Assert.IsTrue(regex.MustMatch('.'));
            Assert.IsTrue(regex.MustMatch('!'));
        }

        [TestMethod]
        public void MatchDigitSetNegated()
        {
            var regex = RegexParser.Parse("\\D") as SingleElement;

            Assert.IsFalse(regex.CanMatch('0'));
            Assert.IsFalse(regex.CanMatch('9'));

            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsTrue(regex.MustMatch('A'));
            Assert.IsTrue(regex.MustMatch('.'));
            Assert.IsTrue(regex.MustMatch('!'));
        }


        [TestMethod]
        public void MatchWhitespaceSetNegated()
        {
            var regex = RegexParser.Parse("\\S") as SingleElement;

            Assert.IsFalse(regex.CanMatch(' '));

            Assert.IsTrue(regex.MustMatch('0'));
            Assert.IsTrue(regex.MustMatch('a'));
            Assert.IsTrue(regex.MustMatch('A'));
            Assert.IsTrue(regex.MustMatch('.'));
            Assert.IsTrue(regex.MustMatch('!'));
        }
    }
}
