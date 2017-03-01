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
  public class PrefixOperationsTests : StringAbstractionTestBase<Prefix>
  {
    Prefix.Operations<TestVariable> operations = new Prefix.Operations<TestVariable>();

    Prefix somePrefix = new Prefix("somePrefix");
    Prefix some = new Prefix("some");
    Prefix something = new Prefix("something");
    Prefix bottom = new Prefix((string)null);
    Prefix top = new Prefix("");

    [TestMethod]
    public void TestPrefixStartsWith()
    {
      Assert.AreEqual(ProofOutcome.False, operations.StartsEndsWithOrdinal(Arg(somePrefix), null, Arg(something), null, false).ProofOutcome);
      Assert.AreEqual(ProofOutcome.Top, operations.StartsEndsWithOrdinal(Arg(somePrefix), null, Arg(some), null, false).ProofOutcome);
    }
    [TestMethod]
    public void TestPrefixReplaceChar()
    {
      Assert.AreEqual(new Prefix("somEPrEfix"), operations.Replace(somePrefix, CharInterval.For('e'), CharInterval.For('E')));
      Assert.AreEqual(new Prefix("somePrefix"), operations.Replace(somePrefix, CharInterval.For('z'), CharInterval.For('E')));
    }
    [TestMethod]
    public void TestPrefixReplaceString()
    {
      Assert.AreEqual(new Prefix("cc"), operations.Replace(Arg(new Prefix("aaaaaaaa")), Arg("aaa"), Arg("c")));
      Assert.AreEqual(new Prefix("ccaab"), operations.Replace(Arg(new Prefix("aaaaaaaab")), Arg("aaa"), Arg("c")));
      Assert.AreEqual(new Prefix("ccc"), operations.Replace(Arg(new Prefix("aaaaaaaaa")), Arg("aaa"), Arg("c")));
      Assert.AreEqual(new Prefix("aca"), operations.Replace(Arg(new Prefix("aaaabaaaa")), Arg("aaab"), Arg("c")));
      Assert.AreEqual(new Prefix("aaaa"), operations.Replace(Arg(new Prefix("aaa")), Arg("aa"), Arg("aaa")));
      Assert.AreEqual(new Prefix("baa"), operations.Replace(Arg(new Prefix("aaa")), Arg("aa"), Arg("baa")));
      Assert.AreEqual(new Prefix("cccccc"), operations.Replace(Arg(new Prefix("cccccca")), Arg("aa"), Arg("baa")));
      Assert.AreEqual(new Prefix("cccccca"), operations.Replace(Arg(new Prefix("cccccca")), Arg("aa"), Arg("aaa")));
    }
    [TestMethod]
    public void TestPrefixContains()
    {
      Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(somePrefix), null, Arg("nothing"), null));
      Assert.AreEqual(FlatPredicate.True, operations.Contains(Arg(somePrefix), null, Arg("some"), null));
      Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg(somePrefix), null, Arg(somePrefix), null));
      Assert.AreEqual(FlatPredicate.Top, operations.Contains(Arg("some"), null, Arg(some), null));
      Assert.AreEqual(FlatPredicate.False, operations.Contains(Arg("nothing"), null, Arg(some), null));
    }
    [TestMethod]
    public void TestPrefixSubstring()
    {
      Assert.AreEqual(new Prefix("ePre"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(4)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(7)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.For(100)));
      Assert.AreEqual(new Prefix("ePrefix"), operations.Substring(somePrefix, IndexInterval.For(3), IndexInterval.Infinity));

      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(10), IndexInterval.For(1)));
      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(10), IndexInterval.Infinity));

      Assert.AreEqual(somePrefix, operations.Substring(somePrefix, IndexInterval.For(0), IndexInterval.Infinity));
      Assert.AreEqual(top, operations.Substring(somePrefix, IndexInterval.For(0), IndexInterval.For(0)));
    }
    [TestMethod]
    public void TestPrefixRemove()
    {
      Assert.AreEqual(new Prefix("somfix"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(4)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(7)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.For(100)));
      Assert.AreEqual(new Prefix("som"), operations.Remove(somePrefix, IndexInterval.For(3), IndexInterval.Infinity));

      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(10), IndexInterval.For(1)));
      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(10), IndexInterval.Infinity));

      Assert.AreEqual(top, operations.Remove(somePrefix, IndexInterval.For(0), IndexInterval.Infinity));
      Assert.AreEqual(somePrefix, operations.Remove(somePrefix, IndexInterval.For(0), IndexInterval.For(0)));
    }
    [TestMethod]
    public void TestPrefixPadLeftRight()
    {
      Assert.AreEqual(new Prefix(""), operations.PadLeftRight(somePrefix, IndexInterval.For(20), CharInterval.For(' '), false));
      Assert.AreEqual(new Prefix("s"), operations.PadLeftRight(somePrefix, IndexInterval.For(20), CharInterval.For('s'), false));
      Assert.AreEqual(new Prefix("    "), operations.PadLeftRight(new Prefix("    prefix"), IndexInterval.For(20), CharInterval.For(' '), false));
      Assert.AreEqual(new Prefix("somePrefix"), operations.PadLeftRight(somePrefix, IndexInterval.For(10), CharInterval.For('x'), false));
      Assert.AreEqual(somePrefix, operations.PadLeftRight(somePrefix, IndexInterval.For(20), CharInterval.For(' '), true));
    }
    [TestMethod]
    public void TestPrefixInsert()
    {
      Assert.AreEqual(new Prefix("someOtherPrefix"), operations.Insert(Arg(somePrefix), IndexInterval.For(4), Arg("Other")));
      Assert.AreEqual(new Prefix("somePrefixOther"), operations.Insert(Arg(somePrefix), IndexInterval.For(10), Arg("Other")));
      Assert.AreEqual(new Prefix("somePrefix"), operations.Insert(Arg(somePrefix), IndexInterval.For(11), Arg("Other")));

      Assert.AreEqual(new Prefix("something"), operations.Insert(Arg(somePrefix), IndexInterval.For(4), Arg(new Prefix("thing"))));
      Assert.AreEqual(new Prefix("somePrefixthing"), operations.Insert(Arg(somePrefix), IndexInterval.For(10), Arg(new Prefix("thing"))));
      Assert.AreEqual(new Prefix("somePrefix"), operations.Insert(Arg(somePrefix), IndexInterval.For(11), Arg(new Prefix("thing"))));

      Assert.AreEqual(new Prefix("somePrefix"), operations.Insert(Arg("other"), IndexInterval.For(0), Arg(somePrefix)));
      Assert.AreEqual(new Prefix("othersomePrefix"), operations.Insert(Arg("other"), IndexInterval.For(5), Arg(somePrefix)));
      Assert.AreEqual(somePrefix.Bottom, operations.Insert(Arg("other"), IndexInterval.For(6), Arg(somePrefix)));
    }

    [TestMethod]
    public void TestPrefixCompare()
    {
      Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(Arg(new Prefix("pre")), Arg(new Prefix("pre"))));
      Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(Arg(new Prefix("prefix")), Arg(new Prefix("pre"))));
      Assert.AreEqual(CompareResult.Less, operations.CompareOrdinal(Arg(new Prefix("a")), Arg(new Prefix("pre"))));
      Assert.AreEqual(CompareResult.Greater, operations.CompareOrdinal(Arg(new Prefix("z")), Arg(new Prefix("pre"))));

      Assert.AreEqual(CompareResult.GreaterEqual, operations.CompareOrdinal(Arg(new Prefix("pre")), Arg("pre")));
      Assert.AreEqual(CompareResult.LessEqual, operations.CompareOrdinal(Arg("pre"), Arg(new Prefix("pre"))));

      Assert.AreEqual(CompareResult.Top, operations.CompareOrdinal(Arg("prefix"), Arg(new Prefix("pre"))));
      Assert.AreEqual(CompareResult.Less, operations.CompareOrdinal(Arg("pre"), Arg(new Prefix("prefix"))));
      Assert.AreEqual(CompareResult.Greater, operations.CompareOrdinal(Arg(new Prefix("prefix")), Arg("pre")));

    }
  }

}
