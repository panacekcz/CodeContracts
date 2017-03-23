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
