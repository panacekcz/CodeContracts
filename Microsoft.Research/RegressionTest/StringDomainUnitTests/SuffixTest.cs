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
using Microsoft.Research.AbstractDomains.Strings;

using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
  [TestClass]
  public class SuffixTests
  {
    Suffix someSuffix = new Suffix("someSuffix");
    Suffix suffix = new Suffix("Suffix");
    Suffix otherSuffix = new Suffix("otherSuffix");
    Suffix bottom = new Suffix((string)null);
    Suffix top = new Suffix("");

    [TestMethod]
    public void TestJoin()
    {
      Assert.AreEqual(suffix, otherSuffix.Join(someSuffix));
      Assert.AreEqual(suffix, suffix.Join(someSuffix));
      Assert.AreEqual(suffix, otherSuffix.Join(suffix));

      Assert.AreEqual(someSuffix, bottom.Join(someSuffix));
      Assert.AreEqual(someSuffix, someSuffix.Join(bottom));

      Assert.AreEqual(top, top.Join(someSuffix));
      Assert.AreEqual(top, someSuffix.Join(top));
    }

    [TestMethod]
    public void TestMeet()
    {
      Assert.AreEqual(bottom, otherSuffix.Meet(someSuffix));
      Assert.AreEqual(someSuffix, suffix.Meet(someSuffix));
      Assert.AreEqual(otherSuffix, otherSuffix.Meet(suffix));

      Assert.AreEqual(bottom, bottom.Meet(someSuffix));
      Assert.AreEqual(bottom, someSuffix.Meet(bottom));

      Assert.AreEqual(someSuffix, top.Meet(someSuffix));
      Assert.AreEqual(someSuffix, someSuffix.Meet(top));
    }

    [TestMethod]
    public void TestTop()
    {
      Assert.IsTrue(top.IsTop);
      Assert.IsFalse(someSuffix.IsTop);
      Assert.IsFalse(bottom.IsTop);
    }

    [TestMethod]
    public void TestBottom()
    {
      Assert.IsFalse(top.IsBottom);
      Assert.IsFalse(someSuffix.IsBottom);
      Assert.IsTrue(bottom.IsBottom);
    }

    [TestMethod]
    public void TestCompare()
    {
      Assert.IsTrue(someSuffix.LessThanEqual(someSuffix));
      Assert.IsTrue(someSuffix.LessThanEqual(suffix));
      Assert.IsFalse(suffix.LessThanEqual(someSuffix));
      Assert.IsFalse(someSuffix.LessThanEqual(otherSuffix));

      Assert.IsTrue(suffix.LessThanEqual(top));

      Assert.IsTrue(bottom.LessThanEqual(bottom));
      Assert.IsTrue(bottom.LessThanEqual(someSuffix));
      Assert.IsFalse(someSuffix.LessThanEqual(bottom));
    }

    [TestMethod]
    public void TestEqual()
    {
      Assert.AreNotEqual(top, bottom);
      Assert.AreNotEqual(suffix, someSuffix);
      Assert.AreNotEqual(suffix, top);
      Assert.AreNotEqual(suffix, bottom);
    }
  }

}
