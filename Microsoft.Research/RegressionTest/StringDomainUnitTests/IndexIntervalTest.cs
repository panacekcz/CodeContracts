﻿// CodeContracts
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
    [TestClass]
    public class IndexIntervalTest
    {
        [TestMethod]
        public void JoinTest()
        {
            Assert.AreEqual(IndexInterval.For(4, 43), IndexInterval.For(4, 10).Join(IndexInterval.For(30, 43)));
            Assert.AreEqual(IndexInterval.For(4, 10), IndexInterval.For(4, 10).Join(IndexInterval.Unreached));
            Assert.AreEqual(IndexInterval.Unknown, IndexInterval.For(4, 10).Join(IndexInterval.Unknown));
        }

        [TestMethod]
        public void MeetTest()
        {
            IndexInterval low = IndexInterval.For(4, 20);
            IndexInterval high = IndexInterval.For(15, 43);

            Assert.AreEqual(IndexInterval.For(15, 20), low.Meet(high));
            Assert.AreEqual(IndexInterval.Unreached, IndexInterval.For(4, 10).Meet(IndexInterval.Unreached));
            Assert.AreEqual(IndexInterval.For(4, 10), IndexInterval.For(4, 10).Meet(IndexInterval.Unknown));
        }

    }
}
