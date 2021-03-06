﻿namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;

    public abstract class AbstractSearchNode<A, S, T, C> : ISearchNode<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        protected readonly S state;
        protected readonly T parent;
        protected A action = default(A);

        protected C pathCost = new C();
        protected C stepCost = new C();
        protected readonly int depth;

        public AbstractSearchNode(T node, S state)
        {
            this.state = state;
            this.parent = node;
            this.depth = HasParent ? parent.Depth + 1 : 0;
        }

        private bool HasParent { get { return !EqualityComparer<T>.Default.Equals(parent, default(T));  } }

        public A Action { get { return action; } set { action = value; } }
        public T Parent { get { return parent; } }
        public C PathCost { get { return pathCost; } }
        public int Depth { get { return depth; } }
        public S State { get { return state; } }

        public virtual C EstCost { get { return PathCost; } }

        public C StepCost
        {
            get
            {
                return stepCost;
            }

            set
            {
                stepCost = value;
                pathCost = HasParent ? parent.PathCost.Add(value) : value;
            }
        }

        public string WriteSolution()
        {
            var actions = new List<string>();
            for (ISearchNode<A, S, T, C> node = this; node != null; node = node.Parent)
            {
                if (node.Action != null)
                {
                    actions.Add(node.Action.ToString());
                }
            }
            actions.Reverse();
            return String.Join(" ", actions);
        }
    }
}
