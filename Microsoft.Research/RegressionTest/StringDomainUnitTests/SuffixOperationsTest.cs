﻿// CodeContracts
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
  public class SuffixOperationsTests : StringAbstractionTestBase<Suffix>
  {
    private readonly Suffix.Operations<TestVariable> operations = new Suffix.Operations<TestVariable>();

    private Suffix someSuffix = new Suffix("someSuffix");
    private Suffix suffix = new Suffix("Suffix");
    private Suffix otherSuffix = new Suffix("otherSuffix");
    private Suffix bottom = new Suffix((string)null);
    private Suffix top = new Suffix("");


    [TestMethod]
    public void TestEndsWith()
    {
      Assert.AreEqual(ProofOutcome.True, operations.EndsWithOrdinal(Arg(someSuffix), null, Arg("Suffix"), null).ProofOutcome);

      Assert.AreEqual(ProofOutcome.False, operations.EndsWithOrdinal(Arg(someSuffix), null, Arg(otherSuffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.False, operations.EndsWithOrdinal(Arg(someSuffix), null, Arg("other"), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.False, operations.EndsWithOrdinal(Arg("other"), null, Arg(someSuffix), null).ProofOutcome);

      Assert.AreEqual(ProofOutcome.Top, operations.EndsWithOrdinal(Arg(someSuffix), null, Arg(suffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.False, operations.EndsWithOrdinal(Arg("Suffix"), null, Arg(someSuffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.Top, operations.EndsWithOrdinal(Arg(suffix), null, Arg(someSuffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.Top, operations.EndsWithOrdinal(Arg(suffix), null, Arg("someSuffix"), null).ProofOutcome);
    }

    [TestMethod]
    public void TestConcat()
    {
      Assert.AreEqual(new Suffix("SuffixAndOther"), operations.Concat(Arg(suffix), Arg("AndOther")));
      Assert.AreEqual(suffix, operations.Concat(Arg("otherAnd"), Arg(suffix)));
    }

    [TestMethod]
    public void TestReplaceChar()
    {
      Assert.AreEqual(new Suffix("somESuffix"), operations.Replace(someSuffix, CharInterval.For('e'), CharInterval.For('E')));
      Assert.AreEqual(new Suffix("someSuffix"), operations.Replace(someSuffix, CharInterval.For('z'), CharInterval.For('E')));
      Assert.AreEqual(new Suffix("uffix"), operations.Replace(someSuffix, CharInterval.For('A', 'Z'), CharInterval.For('E')));
      Assert.AreEqual(new Suffix("Suffix"), operations.Replace(someSuffix, CharInterval.For('e'), CharInterval.For('A', 'Z')));
      Assert.AreEqual(new Suffix(""), operations.Replace(someSuffix, CharInterval.Unknown, CharInterval.Unknown));
    }


    [TestMethod]
    public void TestContains()
    {
      Assert.AreEqual(ProofOutcome.Top, operations.Contains(Arg(someSuffix), null, Arg("nothing"), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.True, operations.Contains(Arg(someSuffix), null, Arg("some"), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.Top, operations.Contains(Arg(someSuffix), null, Arg(someSuffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.Top, operations.Contains(Arg("Suffix"), null, Arg(suffix), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.False, operations.Contains(Arg("nothing"), null, Arg(suffix), null).ProofOutcome);
    }
    [TestMethod]
    public void TestSubstring()
    {
      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(3), IndexInterval.For(4)));
      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(3), IndexInterval.For(7)));
      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(3), IndexInterval.For(100)));
      Assert.AreEqual(new Suffix("eSuffix"), operations.Substring(someSuffix, IndexInterval.For(3), IndexInterval.Infinity));

      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(10), IndexInterval.For(1)));
      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(10), IndexInterval.Infinity));

      Assert.AreEqual(someSuffix, operations.Substring(someSuffix, IndexInterval.For(0), IndexInterval.Infinity));
      Assert.AreEqual(top, operations.Substring(someSuffix, IndexInterval.For(0), IndexInterval.For(0)));
    }


    [TestMethod]
    public void TestRemove()
    {
      // Removing until end is always top
      Assert.AreEqual(new Suffix(""), operations.Remove(new Suffix("aaaaa"), IndexInterval.For(3), IndexInterval.Infinity));
      // Removing known length 
      Assert.AreEqual(new Suffix("ababacdefg"), operations.Remove(new Suffix("abababacdefg"), IndexInterval.For(5), IndexInterval.For(2)));
      Assert.AreEqual(new Suffix("abacdefg"), operations.Remove(new Suffix("cdababacdefg"), IndexInterval.For(5), IndexInterval.For(2)));
      Assert.AreEqual(new Suffix("cdefg"), operations.Remove(new Suffix("ijklmnocdefg"), IndexInterval.For(5), IndexInterval.For(2)));
      Assert.AreEqual(new Suffix("abab"), operations.Remove(new Suffix("cdababab"), IndexInterval.For(100), IndexInterval.For(2)));
      Assert.AreEqual(new Suffix(""), operations.Remove(new Suffix("abcdabcd"), IndexInterval.For(5), IndexInterval.For(8)));
      Assert.AreEqual(new Suffix("ijklmnocdefg"), operations.Remove(new Suffix("ijklmnocdefg"), IndexInterval.For(5), IndexInterval.For(0)));
      Assert.AreEqual(new Suffix("ijklmnocdefg"), operations.Remove(new Suffix("ijklmnocdefg"), IndexInterval.For(100), IndexInterval.For(0)));
    }
    [TestMethod]
    public void TestPadLeft()
    {
      Assert.AreEqual(someSuffix, operations.PadLeft(someSuffix, IndexInterval.For(20), CharInterval.For(' ')));
    }
    [TestMethod]
    public void TestPadRight()
    {
      Assert.AreEqual(new Suffix(""), operations.PadRight(someSuffix, IndexInterval.For(20), CharInterval.For(' ')));
      Assert.AreEqual(new Suffix("x"), operations.PadRight(someSuffix, IndexInterval.For(20), CharInterval.For('x')));
      Assert.AreEqual(new Suffix("    "), operations.PadRight(new Suffix("Suffix    "), IndexInterval.For(20), CharInterval.For(' ')));
      Assert.AreEqual(new Suffix("someSuffix"), operations.PadRight(someSuffix, IndexInterval.For(10), CharInterval.For('z')));
    }

    [TestMethod]
    public void TestInsert()
    {
      Assert.AreEqual(someSuffix.Bottom, operations.Insert(Arg("other"), IndexInterval.For(6), Arg(someSuffix)));

      Assert.AreEqual(new Suffix("aaaaab"), operations.Insert(Arg(new Suffix("aaaaab")), IndexInterval.For(0, 5), Arg("aa")));
      Assert.AreEqual(new Suffix("aaaaab"), operations.Insert(Arg(new Suffix("aaaaab")), IndexInterval.For(0, 5), Arg("aaaaaaaaa")));
      Assert.AreEqual(new Suffix("aab"), operations.Insert(Arg(new Suffix("aaaaab")), IndexInterval.For(0, 5), Arg("aaaaaaacaa")));
      Assert.AreEqual(new Suffix("aab"), operations.Insert(Arg(new Suffix("aaacaab")), IndexInterval.For(0, 5), Arg("aaaaaaacaa")));
      Assert.AreEqual(new Suffix("aab"), operations.Insert(Arg(new Suffix("aaacaab")), IndexInterval.For(0, 5), Arg("aaaaaaaaa")));
      Assert.AreEqual(new Suffix("aab"), operations.Insert(Arg(new Suffix("aaaaab")), IndexInterval.For(0, 5), Arg(new Suffix("aa"))));

      Assert.AreEqual(new Suffix("abcdefgh"), operations.Insert(Arg(new Suffix("abcdefgh")), IndexInterval.For(0, 5), Arg("")));
      Assert.AreEqual(new Suffix("abcdefgh"), operations.Insert(Arg(new Suffix("abcdefgh")), IndexInterval.For(0), Arg(new Suffix("ijklmnop"))));

      Assert.AreEqual(new Suffix(""), operations.Insert(Arg(new Suffix("aaaaab")), IndexInterval.For(IndexInt.For(0), IndexInt.Infinity), Arg(new Suffix("aa"))));
      Assert.AreEqual(new Suffix("aa"), operations.Insert(Arg(new Suffix("aaaaaa")), IndexInterval.For(IndexInt.For(0), IndexInt.Infinity), Arg(new Suffix("aa"))));
    }

  }

}