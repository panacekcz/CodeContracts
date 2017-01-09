using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Visits a prefix tree from the root so that all
    /// predecessors are visited before a node is visited.
    /// </summary>
    /// <typeparam name="T">A value that will be associated with each node.</typeparam>
    abstract class ForwardVisitor<T>
    {
        private readonly Dictionary<InnerNode, int> inputDegree;
        private readonly Dictionary<InnerNode, T> data;
        private readonly List<InnerNode> accessible;

        private void Collect(InnerNode node)
        {
            int i;
            if(!inputDegree.TryGetValue(node, out i))
            {
                //children
                foreach(var c in node.children)
                {
                    if (c.Value is InnerNode)
                        Collect((InnerNode)c.Value);
                }
            }

            inputDegree[node] = i + 1;
        }
        private void FindRoots()
        {
            foreach(var c in inputDegree)
            {
                if (c.Value == 1)
                    accessible.Add(c.Key);
            }
        }
        private void Discount(InnerNode node)
        {
            foreach (var c in node.children)
            {
                var cn = c.Value;
                if (cn is InnerNode)
                {
                    if(--inputDegree[(InnerNode)cn] == 1)
                    {
                        accessible.Add((InnerNode)cn);
                    }
                }
            }
        }

        protected abstract void VisitInnerNode(InnerNode node);
        protected abstract T Merge(T oldData, T newData);
        protected abstract T Default();

        /// <summary>
        /// Gets the value associated with a node.
        /// </summary>
        /// <param name="node">A node of the tree.</param>
        /// <returns>The value associated with <paramref name="node"/>.</returns>
        protected T Get(InnerNode node)
        {
            T value;
            if (!data.TryGetValue(node, out value))
                value = Default();
            return value;
        }

        protected void Push(PrefixTreeNode node, T nextData)
        {
            if (!(node is InnerNode))
                return;

            InnerNode innerNode = (InnerNode)node;

            T old;
            if(data.TryGetValue(innerNode,out old))
            {
                data[innerNode] = Merge(old, nextData);
            }
            else
            {
                data[innerNode] = nextData;
            }
        }

        /// <summary>
        /// Traverses the graph in a forward direction.
        /// </summary>
        /// <param name="root">Root of the graph.</param>
        protected void Traverse(InnerNode root)
        {
            Collect(root);
            FindRoots();

            while(accessible.Count > 0){
                InnerNode next = accessible[accessible.Count - 1];
                accessible.RemoveAt(accessible.Count - 1);

                VisitInnerNode(next);
                Discount(next);
            }
        }
    }
}
