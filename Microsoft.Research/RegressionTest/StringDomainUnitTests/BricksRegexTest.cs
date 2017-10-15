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
using Microsoft.Research.Regex.Model;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    [TestClass]
    public class BricksRegexTest : BricksTestBase
    {
        protected override IBricksPolicy CreateBricksPolicy()
        {
            return new DefaultBricksPolicy { MergeConstantSets = false, ExpandConstantRepetitions = false };
        }

        private Bricks BricksForRegex(string regexString)
        {
            Element regex = RegexUtil.ModelForRegex(regexString);
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

            Assert.AreEqual(expectedResult, operations.RegexIsMatch(bricks, null, RegexUtil.ModelForRegex(patternString)).ProofOutcome);
        }

        [TestMethod]
        public void TestBricksForRegex()
        {
            AssertBricksForRegex(@"^A\z", "{A}[1,1]");
            AssertBricksForRegex(@"^(?:A|B|C)\z", "{A,B,C}[1,1]");
            AssertBricksForRegex(@"^[ab][cd][ef]\z", "{a,b}[1,1]{c,d}[1,1]{e,f}[1,1]");
            AssertBricksForRegex(@"^(?:ab|cd){3,8}\z", "{ab,cd}[3,8]");
            AssertBricksForRegex(@"^(?:ab|cd){3,8}(?:ef|gh){4,7}\z", "{ab,cd}[3,8]{ef,gh}[4,7]");
            AssertBricksForRegex(@"^(?:ab|cd)?\z", "{ab,cd}[0,1]");

            AssertBricksForRegex(@"^(?:[ab][cd]|[ef][gh])\z", "{a,b,e,f}[1,1]{c,d,g,h}[1,1]");
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

            AssertBricksIsMatch(ProofOutcome.Top, @"^[AB][CD]\z", @"^AC\z");
            AssertBricksIsMatch(ProofOutcome.False, @"^[AB]C\z", @"^[AC]D\z");
            AssertBricksIsMatch(ProofOutcome.True, @"^[AB]C\z", @"^[AB][CD]\z");

        }


        [TestMethod]
        public void TestBricksForRexRegex()
        {
            // Sample regexes taken from 
            // Rex: Symbolic Regular Expression Explorer
            // M. Veanes, P. de Halleux, N. Tillmann
            // ICST 2010
            AssertBricksForRegex(@"^(([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*\z", "*[0,Inf]");
            AssertBricksForRegex(@"^[A-Za-z0-9](([ \.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\. ([A-Za-z][A-Za-z]+)*\z", "{0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[1,1]*[0,Inf]{@}[1,1]{0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[1,1]{0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[0,Inf]*[0,Inf]{. }[1,1]*[0,Inf]");
            AssertBricksForRegex(@"^[+-]?([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?\z", "{+,-}[0,1]{0,1,2,3,4,5,6,7,8,9}[0,Inf]{.}[0,1]{0,1,2,3,4,5,6,7,8,9}[0,Inf]{+,-}[0,1]{E,e}[0,1]");
            AssertBricksForRegex(@"^[0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4}\z", "{0,1,2,3,4,5,6,7,8,9}[1,1]{0,1,2,3,4,5,6,7,8,9}[0,1]{/}[1,1]{0,1,2,3,4,5,6,7,8,9}[1,1]{0,1,2,3,4,5,6,7,8,9}[0,1]{/}[1,1]{0,1,2,3,4,5,6,7,8,9}[2,2]{0,1,2,3,4,5,6,7,8,9}[0,2]");
            AssertBricksForRegex(@"^[0-9]{2}-[0-9]{2}-[0-9]{4}\z", "{0,1,2,3,4,5,6,7,8,9}[2,2]{-}[1,1]{0,1,2,3,4,5,6,7,8,9}[2,2]{-}[1,1]{0,1,2,3,4,5,6,7,8,9}[4,4]");
            AssertBricksForRegex(@"^\z?([0-9]{1,3},?([0-9]{3},?)*[0-9]{3}(\.[0-9]{0,2})?|[0-9]{1,3}(\.[0-9]{0,2})?|\.[0-9]{1,2}?)\z", @"{0,1,2,3,4,5,6,7,8,9}[0,3]{,}[0,1]*[0,Inf]{0,1,2,3,4,5,6,7,8,9,.}[0,7]");
            AssertBricksForRegex(@"^([A-Z]{2}|[a-z]{2} [0-9]{2} [A-Z]{1,2}|[a-z]{1,2} [0-9]{1,4})?([A-Z]{3}|[a-z]{3} [0-9]{1,4})?\z", @"{A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,0,1,2,3,4,5,6,7,8,9}[0,4]{A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z, }[0,1]{ ,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[0,2]{0,1,2,3,4,5,6,7,8,9}[0,2]{ }[0,1]{a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[0,2]{A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,0,1,2,3,4,5,6,7,8,9}[0,4]{ }[0,1]{a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z}[0,3]");
        }
    }
}
