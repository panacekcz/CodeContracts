using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    class CharAtVisitor : IntervalVisitor
    {
        /// <summary>
        /// The interval of characters collected so far.
        /// </summary>
        private CharInterval result = CharInterval.Unreached;
        /// <summary>
        /// The interval of indices, which are relevant.
        /// </summary>
        private IndexInterval indices;

        /// <summary>
        /// Collects all characters that can be at index from interval in the tree root.
        /// </summary>
        /// <param name="interval">Interval of relevant indices.</param>
        /// <param name="root">Root of the tree</param>
        /// <returns></returns>
        public CharInterval CharAt(IndexInterval interval, InnerNode root)
        {
            indices = interval;
            Traverse(root);
            return result;
        }

     
        protected override void VisitInnerNode(InnerNode node)
        {
            IndexInterval i = Get(node);

            char min = node.children.Keys.Min();
            char max = node.children.Keys.Max();

            PushChildren(node, i);

            if (!i.Meet(indices).IsBottom)
            {
                result = result.Join(CharInterval.For(min, max));
            }
        }
    }
}
