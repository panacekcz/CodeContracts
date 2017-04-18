// CodeContracts
// 
// Copyright 2016-2017 Charles University
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

// Created by Vlastimil Dort (2016-2017)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Parses string representations of tokens trees.
    /// </summary>
    public class TokensTreeParser
    {
        private enum State
        {
            OUTSIDE, INSIDE, END
        }

        private readonly Stack<InnerNode> openedNodes = new Stack<InnerNode>();
        private State state;
        private char current;

        public Tokens ParseTokens(string s)
        {
            return new Tokens((InnerNode)Parse(s));
        }

        public TokensTreeNode Parse(string s)
        {
            this.state = State.OUTSIDE;

            InnerNode preRoot = new InnerNode(false);
            this.current = '\0';
            this.openedNodes.Push(preRoot);

            foreach (char c in s)
                Next(c);

            if (state != State.INSIDE || openedNodes.Count != 1)
                throw new FormatException();

            openedNodes.Clear();

            return preRoot.children['\0'];
        }

        private void Next(char c)
        {
            if (state == State.OUTSIDE)
            {
                if (c == '{')
                {
                    if (openedNodes.Count < 1)
                        throw new FormatException();

                    state = State.INSIDE;
                    InnerNode node = new InnerNode(false);
                    openedNodes.Peek().children.Add(current, node);
                    openedNodes.Push(node);
                }
                else if (c == '*')
                {
                    if (openedNodes.Count < 1)
                        throw new FormatException();

                    openedNodes.Peek().children.Add(current, RepeatNode.Repeat);
                    state = State.INSIDE;
                }
                else
                    throw new FormatException();
            }
            else if (state == State.INSIDE)
            {
                if (c == '}')
                {
                    state = State.END;
                }
                else
                {
                    current = c;
                    state = State.OUTSIDE;
                }

            }
            else
            {
                if (openedNodes.Count < 1)
                    throw new FormatException();
                InnerNode node = openedNodes.Pop();
                if (c == '.')
                    node.accepting = false;
                else if (c == '!')
                    node.accepting = true;
                else
                    throw new FormatException();
                state = State.INSIDE;
            }

        }


    }
}
