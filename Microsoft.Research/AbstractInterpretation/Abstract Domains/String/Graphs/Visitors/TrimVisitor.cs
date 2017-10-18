// CodeContracts
// 
// Copyright (c) Charles University
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

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    /// <summary>
    /// State of <see cref="TrimVisitor"/>.
    /// </summary>
    internal enum TrimVisitorState
    {
        /// <summary>
        /// The following characters should be trimmed if they are in the set.
        /// </summary>
        Trimmed,
        /// <summary>
        /// The following characters may be trimmed or not, if they are in the set.
        /// </summary>
        Unknown,
        /// <summary>
        /// No following characters may be trimmed.
        /// </summary>
        Preserved,
        /// <summary>
        /// Invalid trim state.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Produces a string graph with characters from a set trimmed.
    /// </summary>
    internal abstract class TrimVisitor : CopyVisitor<TrimVisitorState>
    {
        /// <summary>
        /// Set of the characters that will be trimmed.
        /// </summary>
        private readonly HashSet<char> trimmedChars;

        protected TrimVisitor(HashSet<char> trimmedChars)
        {
            this.trimmedChars = trimmedChars;
        }

        /// <summary>
        /// Trims the specified characters from a string graph.
        /// </summary>
        /// <param name="root">Root node of the string graph.</param>
        /// <returns>Root node of a string graph with the characters trimmed.</returns>
        public Node Trim(Node root)
        {
            TrimVisitorState state = TrimVisitorState.Trimmed;
            return VisitNode(root, VisitContext.Root, ref state);
        }

        protected override Node Visit(CharNode charNode, VisitContext context, ref TrimVisitorState data)
        {
            if (data == TrimVisitorState.Preserved)
            {
                // Not trimmed
                return charNode;
            }
            else if (trimmedChars.Contains(charNode.Value))
            {
                // May be trimmed
                if (data == TrimVisitorState.Trimmed)
                {
                    return NodeBuilder.CreateEmptyNode();
                }
                else
                {
                    return NodeBuilder.CreateOptionalNode(charNode);
                }
            }
            else
            {
                // Not trimmed
                data = TrimVisitorState.Preserved;
                return charNode;
            }
        }
        protected override Node Visit(MaxNode maxNode, VisitContext context, ref TrimVisitorState data)
        {
            if (data == TrimVisitorState.Preserved)
            {
                data = TrimVisitorState.Unknown;
            }
            return maxNode;
        }

        protected override Node VisitChildren(OrNode orNode, Node result, ref TrimVisitorState data)
        {
            TrimVisitorState stateBefore = data;
            TrimVisitorState stateAfter = TrimVisitorState.Trimmed;

            foreach (Node child in orNode.children)
            {
                TrimVisitorState childState = stateBefore;
                Node trimmedChild = VisitNode(child, VisitContext.Or, ref childState);
                ((OrNode)result).children.Add(trimmedChild);

                stateAfter = Join(stateAfter, childState);
            }
            data = stateAfter;

            return result;
        }
        private static TrimVisitorState Join(TrimVisitorState stateA, TrimVisitorState stateB)
        {
            if (stateA == TrimVisitorState.Unknown || stateB == TrimVisitorState.Bottom)
            {
                return stateA;
            }
            if (stateB == TrimVisitorState.Unknown || stateA == TrimVisitorState.Bottom)
            {
                return stateB;
            }
            if (stateA == stateB)
            {
                return stateA;
            }
            return TrimVisitorState.Unknown;
        }
    }

}
