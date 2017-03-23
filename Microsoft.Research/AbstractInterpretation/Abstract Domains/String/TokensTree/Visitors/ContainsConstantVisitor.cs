using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// 
    /// </summary>
    class MustContainVisitor : ForwardTokensTreeVisitor<IndexInt>
    {
        private readonly KMP constantKmp;
        private readonly bool fixedEnd;
        private bool fail;

        public MustContainVisitor(string constant, bool fixedEnd)
        {
            this.constantKmp = new KMP(constant);
            this.fixedEnd = fixedEnd;
        }


        protected override IndexInt Default()
        {
            return IndexInt.Negative;
        }

        protected override IndexInt Merge(IndexInt oldData, IndexInt newData)
        {
            return IndexUtils.JoinIndices(oldData, newData);
        }

        private bool IsAcceptingState(IndexInt index)
        {
            if (index.IsInfinite)
                return false;
            if (index.IsNegative)
                return true;
            return index == constantKmp.End;
        }
        private IndexInt Next(IndexInt state, char c)
        {
            if (state.IsInfinite || state.IsNegative)
                return state;
            else
            {
                int nextIndex = constantKmp.Next(state.AsInt, c);

                if (!fixedEnd && nextIndex == constantKmp.End)
                    return IndexInt.Negative;

                return IndexInt.For(nextIndex);
            }

        }

        protected override void VisitInnerNode(InnerNode node)
        {
            if (fail)
                return;

            IndexInt index = Get(node);

            if (node.Accepting && !IsAcceptingState(index))
            {
                fail = true;
            }
            else
            {
                foreach (var c in node.children)
                {
                    Push(c.Value, Next(index, c.Key));
                }
            }
        }

        public bool MustContain(InnerNode root)
        {
            fail = false;
            Push(root, IndexInt.For(0));
            this.Traverse(root);
            return !fail;
        }
    }


}
