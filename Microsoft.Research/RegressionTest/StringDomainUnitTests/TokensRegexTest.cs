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
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;

namespace StringDomainUnitTests
{

    [TestClass]
    public class TokensRegexTest : StringAbstractionTestBase<Tokens>
    {
        private readonly Tokens.Operations<TestVariable> operations;
        private readonly Tokens top;

        public TokensRegexTest()
        {
            operations = new Tokens.Operations<TestVariable>();
            top = operations.Top;

        }

        private Tokens TokensForRegex(string regexString, bool negative)
        {
            Element regex = RegexUtil.ModelForRegex(regexString);
            
            return negative ? TokensRegex.TokensForNegativeRegex(regex, false) : TokensRegex.TokensForRegex(regex, false);
        }

        private void AssertTokensForRegex(string testRegexString, string expectedTokensString)
        {
            AssertTokensForRegex(testRegexString, expectedTokensString, false);
        }
        private void AssertTokensForNegativeRegex(string testRegexString, string expectedTokensString)
        {
            AssertTokensForRegex(testRegexString, expectedTokensString, true);
        }
        private void AssertTokensForRegex(string testRegexString, string expectedTokensString, bool negative)
        {
            Tokens tokens = TokensForRegex(testRegexString, negative);

            string regexTokensString = tokens.ToString();
            Assert.AreEqual(expectedTokensString, regexTokensString);
        }

        [TestMethod]
        public void TestTokensForRegex()
        {
            AssertTokensForRegex(@"^A\z", "{A{}!}.");
            AssertTokensForRegex(@"^[a-f]\z", "{a{}!b{}!c{}!d{}!e{}!f{}!}.");
            AssertTokensForRegex(@"^[a-f]*\z", "{a*b*c*d*e*f*}!");
            AssertTokensForRegex(@"^(?:a*|b)\z", "{a*b{}!}!");
            AssertTokensForRegex(@"^(?:abc|def|ghi)*\z", "{a{b{c*}.}.d{e{f*}.}.g{h{i*}.}.}!");
            AssertTokensForRegex(@"^(?:abc|def|ghi)\z", "{a{b{c{}!}.}.d{e{f{}!}.}.g{h{i{}!}.}.}.");
            AssertTokensForRegex(@"^(?:abc|abd|abe)*\z", "{a{b{c*d*e*}.}.}!");
            AssertTokensForRegex(@"^(?:abc|abd|abe)*ghi\z", "{a{b{c*d*e*}.}.g{h{i{}!}.}.}.");

            Assert.IsTrue(TokensForRegex("$a", false).IsTop);
            Assert.IsTrue(TokensForRegex("$a+", false).IsTop);
        }

        [TestMethod]
        public void TestTokensForNegativeRegex()
        {
            AssertTokensForNegativeRegex(@"[^a]", "{a*}!");
            AssertTokensForNegativeRegex(@"[^ab]", "{a*b*}!");
            AssertTokensForNegativeRegex(@"[^ab]|a[^c]", "{a{c*}.b*}!");

            Assert.IsFalse(TokensForRegex("a", true).IsTop);

            // These do nothing
            Assert.IsTrue(TokensForRegex("^a", true).IsTop);
            Assert.IsTrue(TokensForRegex("a$", true).IsTop);
            Assert.IsTrue(TokensForRegex("^$", true).IsTop);
        }


        [TestMethod]
        public void TestIsMatch()
        {
            // Constant "a" must contain "a" and unions with other expressions
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(operations.Constant("a"), null, RegexUtil.ModelForRegex("a")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(operations.Constant("a"), null, RegexUtil.ModelForRegex("[a-f]")));
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(operations.Constant("a"), null, RegexUtil.ModelForRegex("a|bcd")));
            
            Assert.AreEqual(FlatPredicate.True, operations.RegexIsMatch(operations.Constant("const"), null, RegexUtil.ModelForRegex("^const\\z")));

            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(operations.Top, TestVariable.Var1, RegexUtil.ModelForRegex("^prefix")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(operations.Concat(Arg("prefix"), Arg(operations.Top)), TestVariable.Var1, RegexUtil.ModelForRegex("^prefix")).ProofOutcome);

            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(operations.Top, TestVariable.Var1, RegexUtil.ModelForRegex("suffix\\z")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(operations.Concat(Arg(operations.Top), Arg("suffix")), TestVariable.Var1, RegexUtil.ModelForRegex("suffix\\z")).ProofOutcome);

        }
        [TestMethod]
        public void TestUnknownMatch()
        {
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(top, TestVariable.Var1, RegexUtil.ModelForRegex("a")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(top, TestVariable.Var1, RegexUtil.ModelForRegex("[a-f]")).ProofOutcome);
            Assert.AreEqual(ProofOutcome.Top, operations.RegexIsMatch(top, TestVariable.Var1, RegexUtil.ModelForRegex("a|bcd")).ProofOutcome);
        }

        [TestMethod]
        public void TestTokensForRexRegex()
        {
            // Sample regexes taken from 
            // Rex: Symbolic Regular Expression Explorer
            // M. Veanes, P. de Halleux, N. Tillmann
            // ICST 2010
            AssertTokensForRegex(@"^(([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+([;.](([a-zA-Z0-9 \-\.]+)@([a-zA-Z0-9 \-\.]+)\.([a-zA-Z]{2,5}){1,25})+)*\z", "{ *-*.*0*1*2*3*4*5*6*7*8*9*;*@*A*B*C*D*E*F*G*H*I*J*K*L*M*N*O*P*Q*R*S*T*U*V*W*X*Y*Z*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*w*x*y*z*}!");
            AssertTokensForRegex(@"^[A-Za-z0-9](([ \.\-]?[a-zA-Z0-9]+)*)@([A-Za-z0-9]+)(([\.\-]?[a-zA-Z0-9]+)*)\. ([A-Za-z][A-Za-z]+)*\z", "{ *-*.*0*1*2*3*4*5*6*7*8*9*@{}.A*B*C*D*E*F*G*H*I*J*K*L*M*N*O*P*Q*R*S*T*U*V*W*X*Y*Z*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*w*x*y*z*}!");
            AssertTokensForRegex(@"^[+-]?([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?\z", "{+*-*.*0*1*2*3*4*5*6*7*8*9*E*e*}!");
            AssertTokensForRegex(@"^[0-9]{1,2}/[0-9]{1,2}/[0-9]{2,4}\z", "{/*0*1*2*3*4*5*6*7*8*9*}!");
            AssertTokensForRegex(@"^[0-9]{2}-[0-9]{2}-[0-9]{4}\z", "{-*0*1*2*3*4*5*6*7*8*9*}!");
            AssertTokensForRegex(@"^\z?([0-9]{1,3},?([0-9]{3},?)*[0-9]{3}(\.[0-9]{0,2})?|[0-9]{1,3}(\.[0-9]{0,2})?|\.[0-9]{1,2}?)\z", "{,*.*0*1*2*3*4*5*6*7*8*9*}!");
            AssertTokensForRegex(@"^([A-Z]{2}|[a-z]{2} [0-9]{2} [A-Z]{1,2}|[a-z]{1,2} [0-9]{1,4})?([A-Z]{3}|[a-z]{3} [0-9]{1,4})?\z", "{ *0*1*2*3*4*5*6*7*8*9*A*B*C*D*E*F*G*H*I*J*K*L*M*N*O*P*Q*R*S*T*U*V*W*X*Y*Z*a*b*c*d*e*f*g*h*i*j*k*l*m*n*o*p*q*r*s*t*u*v*w*x*y*z*}!");
        }
    }
}
