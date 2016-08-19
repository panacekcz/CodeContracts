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
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
  [TestClass]
  public class BricksRegexTest : BricksTestBase
  {
    private Bricks top;

    public BricksRegexTest()
    {
      this.top = operations.Top;
    }

    private Bricks BricksForRegex(string regexString)
    {
      Element regex = RegexParser.Parse(regexString);
      BricksRegex br = new BricksRegex(top);
      return br.BricksForRegex(regex);
    }

    private void AssertBricksForRegex(string testRegexString, string expectedBricksString)
    {
      Bricks bricks = BricksForRegex(testRegexString);
      string regexBricksString = bricks.ToString();
      Assert.AreEqual(expectedBricksString, regexBricksString);
    }

    private void AssertBricksIsMatch(ProofOutcome expectedResult, string bricksRegexString, string patternString)
    {
      Bricks bricks = BricksForRegex(bricksRegexString);

      Assert.AreEqual(expectedResult, operations.RegexIsMatch(bricks, null, RegexParser.Parse(patternString)).ProofOutcome);
    }

    [TestMethod]
    public void TestBricksForRegex()
    {
      AssertBricksForRegex(@"^A\z", "{A}[1,1]");
      AssertBricksForRegex(@"^(?:A|B|C)\z", "{A,B,C}[1,1]");
      AssertBricksForRegex(@"^[ab][cd][ef]\z", "{a,b}[1,1]{c,d}[1,1]{e,f}[1,1]");
      AssertBricksForRegex(@"^(?:ab|cd){3,8}\z", "{ab,cd}[3,8]");
      AssertBricksForRegex(@"^(?:ab|cd)?\z", "{ab,cd}[0,1]");
    }

    [TestMethod]
    public void TestBricksIsMatch()
    {
      AssertBricksIsMatch(ProofOutcome.True, @"^A\z", @"^A\z");
      AssertBricksIsMatch(ProofOutcome.False, @"^A\z", @"^B\z");
      AssertBricksIsMatch(ProofOutcome.Top, @"^[AB]\z", @"^B\z");
      AssertBricksIsMatch(ProofOutcome.True, @"^[A]\z", @"^[AB]\z");
      AssertBricksIsMatch(ProofOutcome.True, @"^[A]\z", @"");

      AssertBricksIsMatch(ProofOutcome.Top, @"A", @"B");
      AssertBricksIsMatch(ProofOutcome.False, @"^A", @"^B");
      AssertBricksIsMatch(ProofOutcome.True, @"^A", @"^A");
      AssertBricksIsMatch(ProofOutcome.True, @"^[a]+[b]+[c]+\z", @"^[a]+[b]+[c]+\z");

    }
  }
}
