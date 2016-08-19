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
  public class StringGraphOperationsTest : StringAbstractionTestBase<StringGraph>
  {
    protected StringGraph.Operations<TestVariable> operations = new StringGraph.Operations<TestVariable>();

    [TestMethod]
    public void Contains()
    {
      StringGraph abc = StringGraph.ForString("abc");
      StringGraph d = StringGraph.ForString("d");

      Assert.AreEqual(ProofOutcome.True, operations.Contains(Arg(abc), null, Arg("c"), null).ProofOutcome);
    }

    [TestMethod]
    public void StartsEndsWith()
    {
      StringGraph abc = StringGraph.ForString("abcdefgh");
      StringGraph d = StringGraph.ForString("d");

      Assert.AreEqual(ProofOutcome.True, operations.StartsWithOrdinal(Arg(abc), null, Arg("abc"), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.True, operations.EndsWithOrdinal(Arg(abc), null, Arg("fgh"), null).ProofOutcome);

      Assert.AreEqual(ProofOutcome.False, operations.StartsWithOrdinal(Arg(abc), null, Arg("fgh"), null).ProofOutcome);
      Assert.AreEqual(ProofOutcome.False, operations.EndsWithOrdinal(Arg(abc), null, Arg("abc"), null).ProofOutcome);
    }

    [TestMethod]
    public void TestConcat()
    {
      StringGraph graph = StringGraph.ForString("one");
      StringGraph other = StringGraph.ForString("two");

      Assert.AreEqual("<[o][n][e][c]>", operations.Concat(Arg(graph), Arg("c")).ToString());
      Assert.AreEqual("<[c][o][n][e]>", operations.Concat(Arg("c"), Arg(graph)).ToString());
      Assert.AreEqual("<[o][n][e][t][w][o]>", operations.Concat(Arg(graph), Arg(other)).ToString());

    }

    [TestMethod]
    public void TestSubstring()
    {
      StringGraph graph = StringGraph.ForString("constant");

      Assert.AreEqual("<[s][t][a][n][t]>", operations.Substring(graph, IndexInterval.For(3), IndexInterval.Infinity).ToString());
      Assert.AreEqual("<[s][t]>", operations.Substring(graph, IndexInterval.For(3), IndexInterval.For(2)).ToString());

    }

    [TestMethod]
    public void Pad()
    {
      StringGraph graph = StringGraph.ForString("abc");

      // Not padded
      Assert.AreEqual("<[a][b][c]>", operations.PadLeft(graph, IndexInterval.For(0, 3), CharInterval.For('x')).ToString());
      Assert.AreEqual("<[a][b][c]>", operations.PadRight(graph, IndexInterval.For(0, 3), CharInterval.For('x')).ToString());

      // Padded
      Assert.AreEqual("<a:{<><[x]a>}[a][b][c]>", operations.PadLeft(graph, IndexInterval.For(0, 1000), CharInterval.For('x')).ToString());
      Assert.AreEqual("<[a][b][c]a:{<><[x]a>}>", operations.PadRight(graph, IndexInterval.For(0, 1000), CharInterval.For('x')).ToString());
    }


    [TestMethod]
    public void Trim()
    {
      StringGraph trimmed = StringGraph.ForString("x");
      StringGraph constant = StringGraph.ForString("xxxabcxxx");

      Assert.AreEqual("<[a][b][c][x][x][x]>", operations.TrimStart(Arg(constant), Arg(trimmed)).ToString());
      Assert.AreEqual("<[x][x][x][a][b][c]>", operations.TrimEnd(Arg(constant), Arg(trimmed)).ToString());
      Assert.AreEqual("<[a][b][c]>", operations.Trim(Arg(constant), Arg(trimmed)).ToString());

      Assert.AreEqual("<[b]>", operations.Trim(Arg(constant), Arg("xac")).ToString());

    }

    [TestMethod]
    public void CharAt()
    {
      StringGraph constant = StringGraph.ForString("abcdefg");

      Assert.AreEqual(CharInterval.For('a'), operations.GetCharAt(constant, IndexInterval.For(0)));
      Assert.AreEqual(CharInterval.For('g'), operations.GetCharAt(constant, IndexInterval.For(6)));

      Assert.AreEqual(CharInterval.For('g'), operations.GetCharAt(constant, IndexInterval.For(6, 100)));

      Assert.AreEqual(CharInterval.For('b', 'f'), operations.GetCharAt(constant, IndexInterval.For(1, 5)));
    }
  }
}
