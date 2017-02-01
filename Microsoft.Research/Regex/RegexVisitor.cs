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

using Microsoft.Research.Regex.AST;

namespace Microsoft.Research.Regex
{
    /// <summary>
    /// Visits the AST of a regex.
    /// </summary>
    /// <typeparam name="Result">The type of result passed bottom up.</typeparam>
    /// <typeparam name="Data">The type of data passed along the traversal.</typeparam>
    public abstract class RegexVisitor<Result, Data>
    {
        /// <summary>
        /// Visits the <see cref="Alternation" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Alternation element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Anchor" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Anchor element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Assertion" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Assertion element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Boundary" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Boundary element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Capture" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Capture element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Comment" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Comment element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Concatenation" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Concatenation element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Empty" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Empty element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Loop" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Loop element, ref Data data);
        /// <summary>
        /// Visits the <see cref="NonBacktracking" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(NonBacktracking element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Options" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Options element, ref Data data);
        /// <summary>
        /// Visits the <see cref="OptionsGroup" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(OptionsGroup element, ref Data data);
        /// <summary>
        /// Visits the <see cref="Reference" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(Reference element, ref Data data);
        /// <summary>
        /// Visits the <see cref="SimpleGroup" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(SimpleGroup element, ref Data data);
        /// <summary>
        /// Visits the <see cref="SingleElement" /> regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result Visit(SingleElement element, ref Data data);

        /// <summary>
        /// Visits an unsupported regex element.
        /// </summary>
        /// <inheritdoc cref="VisitElement"/>
        protected abstract Result VisitUnsupported(Element element, ref Data data);

        /// <summary>
        /// Visits a regex element.
        /// </summary>
        /// <param name="element">The element visited.</param>
        /// <param name="data">Data passet along the traversal.</param>
        /// <returns>The result value for <paramref name="element"/>.</returns>
        protected Result VisitElement(Element element, ref Data data)
        {
            if (element is Alternation)
            {
                return Visit((Alternation)element, ref data);
            }
            else if (element is Anchor)
            {
                return Visit((Anchor)element, ref data);
            }
            else if (element is Assertion)
            {
                return Visit((Assertion)element, ref data);
            }
            else if (element is Boundary)
            {
                return Visit((Boundary)element, ref data);
            }
            else if (element is Capture)
            {
                return Visit((Capture)element, ref data);
            }
            else if (element is Comment)
            {
                return Visit((Comment)element, ref data);
            }
            else if (element is Concatenation)
            {
                return Visit((Concatenation)element, ref data);
            }
            else if (element is Empty)
            {
                return Visit((Empty)element, ref data);
            }
            else if (element is Loop)
            {
                return Visit((Loop)element, ref data);
            }
            else if (element is NonBacktracking)
            {
                return Visit((NonBacktracking)element, ref data);
            }
            else if (element is Options)
            {
                return Visit((Options)element, ref data);
            }
            else if (element is OptionsGroup)
            {
                return Visit((OptionsGroup)element, ref data);
            }
            else if (element is Reference)
            {
                return Visit((Reference)element, ref data);
            }
            else if (element is SimpleGroup)
            {
                return Visit((SimpleGroup)element, ref data);
            }
            else if (element is SingleElement)
            {
                return Visit((SingleElement)element, ref data);
            }
            else
            {
                return VisitUnsupported(element, ref data);
            }
        }

    }

}
