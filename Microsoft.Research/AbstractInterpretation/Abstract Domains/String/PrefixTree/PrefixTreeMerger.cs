using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public class PrefixTreeMerger
    {
        private readonly List<InnerNode> merges = new List<InnerNode>();

        public RepeatNode Cutoff(PrefixTreeNode tn)
        {
            if (tn is RepeatNode)
                return (RepeatNode)tn;

            merges.Add((InnerNode)tn);
            return RepeatNode.Repeat;
        }

        private InnerNode MergeInnerNodes(InnerNode oldNode, InnerNode newNode)
        {
            InnerNode merged = new InnerNode(oldNode.Accepting || newNode.Accepting);


            foreach (var kv in oldNode.children)
            {
                PrefixTreeNode otherChild;
                if (newNode.children.TryGetValue(kv.Key, out otherChild))
                    merged.children.Add(kv.Key, Merge(kv.Value, otherChild));
                else
                    merged.children.Add(kv.Key, kv.Value);
            }

            foreach (var kv in newNode.children)
            {
                if (!oldNode.children.ContainsKey(kv.Key))
                {
                    merged.children.Add(kv.Key, kv.Value);
                }
            }

            return merged;
        }

        internal PrefixTreeNode Merge(PrefixTreeNode oldNode, PrefixTreeNode newNode)
        {
            // Trie merging
            // - if any of the two nodes is accepting, cut them off and make root accepting
            //   - if we are in the root (oldNode is root), merge them and make root accepting
            // - if any of the nodes is repeating, cut the other off
            // - if both nodes are not accepting and not repeating, merge them (defined for inner nodes, but may work for dead-end nodes too).

            if (oldNode is RepeatNode)
            {
                if (newNode is RepeatNode)
                    // Both are repeat nodes, return repeat node
                    return oldNode;
                else
                    // One of them is inner, cut it off
                    return Cutoff(newNode);
            }
            else if (newNode is RepeatNode)
            {
                // One of them is inner, cut it off
                return Cutoff(oldNode);
            }
            else
            {
                // Both are inner nodes
                return MergeInnerNodes((InnerNode)oldNode, (InnerNode)newNode);
            }
        }

        public InnerNode Build()
        {
            if (merges.Count == 0)
                throw new InvalidOperationException("No nodes");

            InnerNode root = merges[merges.Count - 1];
            merges.RemoveAt(merges.Count - 1);

            while (merges.Count > 0)
            {
                InnerNode merged = merges[merges.Count - 1];
                merges.RemoveAt(merges.Count - 1);
                root = MergeInnerNodes(root, merged);
            }

            return root;
        }

    }
}
