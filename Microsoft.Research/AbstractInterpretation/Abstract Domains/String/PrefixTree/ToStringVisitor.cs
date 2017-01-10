﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Generates a string representation of a prefix tree.
    /// </summary>
    /// <remarks>
    /// Repeat node is *, inner node is {cX...}. or {cX...}! if accepting.
    /// For str it is {s{t{r{}!}.}.}.
    /// </remarks>
    class ToStringVisitor : PrefixTreeVisitor<string>
    {
        public string ToString(PrefixTreeNode node)
        {
            return VisitNode(node);
        }

        protected override string VisitInnerNode(InnerNode inn)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('{');

            foreach (var child in inn.children)
            {
                sb.Append(child.Key);
                sb.Append(VisitNode(child.Value));
            }
           
            sb.Append('}');
            sb.Append(inn.Accepting ? '!' : '.');

            return sb.ToString();
        }

        protected override string VisitRepeatNode(RepeatNode inn)
        {
            return "*";
        }

    }

    class PrefixTreeParser
    {
        private enum State
        {
            OUTSIDE, INSIDE, END
        }

        private readonly Dictionary<int, PrefixTreeNode> indexedNodes;
        private readonly Stack<InnerNode> openedNodes;

        string s;
        int i;

        State state;
        char current;


        public PrefixTreeNode Parse(string s)
        {
            this.s = s;
            this.i = 0;
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

        void Next(char c)
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
            else if(state == State.INSIDE)
            {
                //TODO: escaping?
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