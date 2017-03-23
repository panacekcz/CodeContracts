﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Changes accepting nodes to repeat nodes in a tokens tree.
    /// </summary>
    internal class RepeatVisitor : TokensTreeTransformer
    {
        private InnerNode root;

        public RepeatVisitor(TokensTreeMerger merger)
            : base(merger)
        {
        }

        /// <summary>
        /// Repeats the specified tree.
        /// </summary>
        /// <param name="root">Root node of the tree.</param>
        public void Repeat(InnerNode root)
        {
            this.root = root;
            Transform(root);
        }

        #region TokensTreeVisitor<InnerNode> overrides
        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (innerNode.Accepting)
            {
                InnerNode notAccepting = new InnerNode(newNode);
                notAccepting.accepting = false;
                if (innerNode == root)
                {
                    return notAccepting;
                }
                else
                {
                    return Cutoff(notAccepting);
                }
            }
            else
                return newNode;
        }
        protected override TokensTreeNode VisitRepeatNode(RepeatNode repeatNode)
        {
            return repeatNode;
        }
        #endregion
    }
} 
    