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
    /// <summary>
    /// Base class for test for the <see cref="Bricks"/> abstract domain.
    /// </summary>
    public abstract class BricksTestBase : StringAbstractionTestBase<Bricks>
    {
        protected readonly IBricksPolicy policy;

        protected Bricks MakeBricks(string value)
        {
            return new Bricks(value, policy);
        }

        //TODO: VD: use or remove
    /*    protected Bricks MakeBricks(int min, int max, string value)
        {
            return new Bricks(new[] { new Brick(value, IndexInt.For(min), IndexInt.For(max)) }, policy);
        }*/

        protected Bricks MakeBricks(params string[] values)
        {
            Bricks bricks = new Bricks(false, policy);
            foreach (string value in values)
                bricks = bricks.Join(new Bricks(value, policy));
            return bricks;
        }
        protected WithConstants<Bricks> MakeBricksArg(params string[] values)
        {
            return Arg(MakeBricks(values));
        }

        protected virtual IBricksPolicy CreateBricksPolicy()
        {
            return new DefaultBricksPolicy();
        }

        protected BricksTestBase()
        {
            policy = CreateBricksPolicy();
            SetOperations(new Bricks.Operations<TestVariable>(policy));
        }
    }
}
