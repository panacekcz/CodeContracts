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
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;


namespace StringDomainUnitTests
{
    [TestClass]
    public class CharacterInclusionTest : CharacterInclusionTestBase
    {
        [TestMethod]
        public void TestEqual()
        {
            Assert.AreEqual(Build("bc", "a"), Build("bbbbcccc", "cbacba"));
            Assert.AreEqual(Build("", ""), Build("", ""));
            Assert.AreNotEqual(Build("b", "ac"), Build("bc", "cbacba"));
            Assert.AreNotEqual(Build("b", "a"), Build("b", "abc"));
            Assert.AreNotEqual(top, Build("b", "abc"));
            Assert.AreNotEqual(top, top.Bottom);
        }

        [TestMethod]
        public void TestJoin()
        {
            Assert.AreEqual(Build("def", "abcghij"), Build("defgh", "ab").Join(Build("defij", "bc")));
            Assert.AreEqual(Build("def", "abcghij"), Build("defgh", "abij").Join(Build("defij", "bcgh")));
        }

        [TestMethod]
        public void TestMeet()
        {
            Assert.AreEqual(Build("defghij", "b"), Build("defgh", "abij").Meet(Build("defij", "bcgh")));
        }

        [TestMethod]
        public void TestContainsValue()
        {
            Assert.IsTrue(Build("def", "abc").ContainsValue("ddffee"));
            Assert.IsTrue(Build("def", "abc").ContainsValue("daebf"));
            Assert.IsFalse(Build("def", "abc").ContainsValue("abcde"));
            Assert.IsFalse(Build("def", "abc").ContainsValue("defg"));
            Assert.IsFalse(Build("def", "abc").ContainsValue(""));
        }



        [TestMethod]
        public void TestCategory()
        {
            CategoryClassification categoryClassification = new CategoryClassification();

            Assert.AreEqual(new CharacterInclusion<BitArrayCharacterSet>("a", categoryClassification, setFactory), new CharacterInclusion<BitArrayCharacterSet>("b", categoryClassification, setFactory));
            Assert.AreNotEqual(new CharacterInclusion<BitArrayCharacterSet>("a", categoryClassification, setFactory), new CharacterInclusion<BitArrayCharacterSet>("4", categoryClassification, setFactory));
        }
    }
}
