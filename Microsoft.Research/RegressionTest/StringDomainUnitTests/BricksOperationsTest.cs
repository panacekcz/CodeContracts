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

namespace StringDomainUnitTests
{

  [TestClass]
  public class BricksOperationsTest : BricksTestBase
  {
    protected override IBricksPolicy CreateBricksPolicy()
    {
      return new DefaultBricksPolicy { ExpandConstantRepetitions = false, MergeConstantSets = false };
    }

    [TestMethod]
    public void Insert()
    {
      Bricks onetwo = MakeBricks("one", "two");
      Assert.AreEqual("{one,two}[1,1]{three}[1,1]", operations.Insert(Arg(onetwo), IndexInterval.For(3), MakeBricksArg("three")).ToString());
      //In case of expanding bricks, would be
      //Assert.AreEqual("{onethree,twothree}[1,1]", operations.Insert(Arg(onetwo), IndexInterval.For(3), MakeBricksArg("three")).ToString());
    }


    [TestMethod]
    public void SubstringEnd()
    {
      Bricks longer = MakeBricks("abcdefgh");
      Assert.AreEqual("{efgh}[1,1]", operations.Substring(longer, IndexInterval.For(4), IndexInterval.Infinity).ToString());
    }
    [TestMethod]
    public void Substring()
    {
      Bricks longer = MakeBricks("abcdefgh");
      Assert.AreEqual("{abcd}[1,1]", operations.Substring(longer, IndexInterval.For(0), IndexInterval.For(4)).ToString());
      Assert.AreEqual("{efgh}[1,1]", operations.Substring(longer, IndexInterval.For(4), IndexInterval.For(4)).ToString());
    }

        [TestMethod]
        public void PadLeftRight()
        {

      IndexInterval index5 = IndexInterval.For(5);
      CharInterval charX = CharInterval.For('x');

      Bricks empty = MakeBricks("");
      Assert.AreEqual("{x}[5,5]", operations.PadLeftRight(empty, index5, charX, false).ToString());
      Assert.AreEqual("{x}[5,5]", operations.PadLeftRight(empty, index5, charX, true).ToString());

      Bricks longer = MakeBricks("abcdefgh");
      Assert.AreEqual("{abcdefgh}[1,1]", operations.PadLeftRight(longer, index5, charX, false).ToString());
      Assert.AreEqual("{abcdefgh}[1,1]", operations.PadLeftRight(longer, index5, charX, true).ToString());

      Bricks shorter = MakeBricks("ij");
      Assert.AreEqual("{x}[3,3]{ij}[1,1]", operations.PadLeftRight(shorter, index5, charX, false).ToString());
      Assert.AreEqual("{ij}[1,1]{x}[3,3]", operations.PadLeftRight(shorter, index5, charX, true).ToString());

      Bricks emptyOrLonger = empty.Join(longer);
      Assert.AreEqual("{x}[0,5]{abcdefgh}[0,1]", operations.PadLeftRight(emptyOrLonger, index5, charX, false).ToString());
      Assert.AreEqual("{abcdefgh}[0,1]{x}[0,5]", operations.PadLeftRight(emptyOrLonger, index5, charX, true).ToString());

      Bricks emptyOrShorter = empty.Join(shorter);
      Assert.AreEqual("{x}[3,3]{x}[0,2]{ij}[0,1]", operations.PadLeftRight(emptyOrShorter, index5, charX, false).ToString());
      Assert.AreEqual("{ij}[0,1]{x}[3,3]{x}[0,2]", operations.PadLeftRight(emptyOrShorter, index5, charX, true).ToString());

      Bricks longerOrShorter = longer.Join(shorter);
      Assert.AreEqual("{x}[0,3]{abcdefgh,ij}[1,1]", operations.PadLeftRight(longerOrShorter, index5, charX, false).ToString());
      Assert.AreEqual("{abcdefgh,ij}[1,1]{x}[0,3]", operations.PadLeftRight(longerOrShorter, index5, charX, true).ToString());
    }

    [TestMethod]
    public void Contains()
    {
      Bricks one = MakeBricks("one");
      Bricks onetwo = MakeBricks("one", "two");
      Bricks o = MakeBricks("o");

      Assert.AreEqual(FlatPredicate.True, operations.Contains(Arg(onetwo), null, Arg(o), null));
      Assert.AreEqual(FlatPredicate.True, operations.Contains(Arg(one), null, Arg(o), null));
    }

    [TestMethod]
    public void StartsWith()
    {
      Bricks strings = MakeBricks("string", "strong");
      Bricks prefix = MakeBricks("str");

      Assert.AreEqual(FlatPredicate.True, operations.StartsWithOrdinal(Arg(strings), null, Arg(prefix), null));
    }

    [TestMethod]
    public void EndsWith()
    {
      Bricks strings = MakeBricks("string", "strong");
      Bricks suffix = MakeBricks("ng");

      Assert.AreEqual(FlatPredicate.True, operations.EndsWithOrdinal(Arg(strings), null, Arg(suffix), null));
    }
  }
}
