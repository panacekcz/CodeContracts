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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{

  [TestClass]
  public class IndexIntTest
  {
    [TestMethod]
    public void TestConversion()
    {
      IndexInt zero = IndexInt.For(0);
      Assert.AreEqual(0, zero.AsInt);
      Assert.IsFalse(zero.IsInfinite || zero.IsNegative);

      zero = IndexInt.ForNonNegative(0);
      Assert.AreEqual(0, zero.AsInt);
      Assert.IsFalse(zero.IsInfinite || zero.IsNegative);

      IndexInt max = IndexInt.ForNonNegative(int.MaxValue);
      Assert.AreEqual(int.MaxValue, max.AsInt);
      Assert.IsFalse(max.IsInfinite || max.IsNegative);
    }

    [TestMethod]
    public void TestNegative()
    {
      IndexInt neg = IndexInt.For(-1);
      Assert.IsTrue(neg.IsNegative);
      Assert.IsFalse(neg.IsInfinite);
    }

    [TestMethod]
    public void TestInfinity()
    {
      IndexInt inf = IndexInt.Infinity;
      Assert.IsFalse(inf.IsNegative);
      Assert.IsTrue(inf.IsInfinite);
    }

    [TestMethod]
    public void TestEquality()
    {
      IndexInt zero = IndexInt.For(0);
      IndexInt neg = IndexInt.For(-1);
      IndexInt inf = IndexInt.Infinity;
      IndexInt ten = IndexInt.For(10);

      Assert.IsTrue(zero == IndexInt.For(0));
      Assert.IsTrue(inf == IndexInt.Infinity);
      Assert.IsTrue(ten == IndexInt.For(10));

      Assert.IsTrue(zero != ten);
      Assert.IsTrue(zero != inf);
      Assert.IsTrue(ten != inf);

      Assert.IsFalse(zero != IndexInt.For(0));
      Assert.IsFalse(inf != IndexInt.Infinity);
      Assert.IsFalse(ten != IndexInt.For(10));

      Assert.IsFalse(zero == ten);
      Assert.IsFalse(zero == inf);
      Assert.IsFalse(ten == inf);

      Assert.IsFalse(neg == -2);
    }

    [TestMethod]
    public void TestComparison()
    {
      IndexInt zero = IndexInt.For(0);
      IndexInt neg = IndexInt.For(-1);
      IndexInt inf = IndexInt.Infinity;
      IndexInt ten = IndexInt.For(10);

      Assert.IsTrue(zero < ten);
      Assert.IsTrue(ten > zero);

      Assert.IsFalse(ten < zero);
      Assert.IsFalse(zero > ten);

      Assert.IsTrue(inf > ten);
      Assert.IsTrue(ten < inf);

      Assert.IsFalse(inf < ten);
      Assert.IsFalse(ten > inf);

      Assert.IsTrue(neg < zero);
      Assert.IsTrue(zero > neg);

      Assert.IsFalse(neg > zero);
      Assert.IsFalse(zero < neg);

      Assert.IsFalse(neg < -2);
    }

  }
}
