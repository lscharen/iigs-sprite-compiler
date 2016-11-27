using System;

namespace SpriteCompiler.AI
{
    public class TreeSearch<A, S, T, C> : AbstractAISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        public TreeSearch(INodeExpander<A, S, T, C> expander)
            : base(expander)
        {
        }

        /// <summary>
        /// Generic tree search. See page 72 in Russell and Norvig
        /// </summary>
        protected override void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem)
        {
            fringe.AddRange(Expand(problem, node));
        }
    }
}
