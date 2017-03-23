using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Visits a prefix tree from the root so that all
    /// predecessors are visited before a node is visited.
    /// </summary>
    /// <typeparam name="TData">A value that will be associated with each node.</typeparam>
    abstract class ForwardTokensTreeVisitor<TData>
    {
        private readonly Dictionary<InnerNode, int> inputDegree = new Dictionary<InnerNode, int>();
        private readonly Dictionary<InnerNode, TData> data = new Dictionary<InnerNode, TData>();
        private readonly List<InnerNode> ready = new List<InnerNode>();

        private void ComputeInputDegrees(InnerNode node)
        {
            int degree;
            if(!inputDegree.TryGetValue(node, out degree))
            {
                // The node is visited for the first time, 
                // traverse the subtree
                foreach(var c in node.children)
                {
                    if (c.Value is InnerNode)
                        ComputeInputDegrees((InnerNode)c.Value);
                }
            }

            // Increase the degree of this node
            inputDegree[node] = degree + 1;
        }

        private void DecreaseChildDegrees(InnerNode node)
        {
            foreach (var c in node.children)
            {
                var childNode = c.Value;
                if (childNode is InnerNode)
                {
                    // Decrease the input degree
                    if(--inputDegree[(InnerNode)childNode] == 0)
                    {
                        // All edges to the node were processed, add the 
                        ready.Add((InnerNode)childNode);
                    }
                }
            }
        }

        /// <summary>
        /// Performs a specific action on a node, which has its final value.
        /// </summary>
        /// <param name="node">A node of the tree.</param>
        protected abstract void VisitInnerNode(InnerNode node);
        /// <summary>
        /// Merges the value associated with a node with a new value.
        /// </summary>
        /// <param name="oldData">Old data associated with a node.</param>
        /// <param name="newData">New data added to the value.</param>
        /// <returns>The result of combining <paramref name="oldData"/> with <paramref name="newData"/>.</returns>
        protected abstract TData Merge(TData oldData, TData newData);
        /// <summary>
        /// Gets the default value for nodes not accessed.
        /// </summary>
        /// <returns>A result value for not accessed nodes.</returns>
        protected abstract TData Default();

        /// <summary>
        /// Gets the value associated with a node.
        /// </summary>
        /// <param name="node">A node of the tree.</param>
        /// <returns>The value associated with <paramref name="node"/>.</returns>
        protected TData Get(InnerNode node)
        {
            TData value;
            if (!data.TryGetValue(node, out value))
                value = Default();
            return value;
        }

        /// <summary>
        /// Adds a value to the data associated with a node.
        /// </summary>
        /// <param name="node">Node of the tree.</param>
        /// <param name="nextData">Value added to the node data.</param>
        protected void Push(TokensTreeNode node, TData nextData)
        {
            // Repeat nodes not considered
            if (!(node is InnerNode))
                return;

            InnerNode innerNode = (InnerNode)node;

            TData oldData;
            if(data.TryGetValue(innerNode, out oldData))
            {
                data[innerNode] = Merge(oldData, nextData);
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
            ComputeInputDegrees(root);
            ready.Add(root);

            while(ready.Count > 0){
                InnerNode next = ready[ready.Count - 1];
                ready.RemoveAt(ready.Count - 1);

                VisitInnerNode(next);
                DecreaseChildDegrees(next);
            }
        }
    }
}
