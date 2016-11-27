using System;
using System.Collections.Generic;

namespace SpriteCompiler.AI
{
    public class GraphSearch<A, S, T, C> : AbstractAISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        private readonly ISet<S> closed = new HashSet<S>();

        public GraphSearch(INodeExpander<A, S, T, C> expander)
            : base(expander)
        {
        }

        /// <summary>
        /// Generic graph search.  See page 83 in Russell and Norvig.  This only works in informed
        /// search if the heuristic is admissible.However, if a heuristic is not admissible and
        /// you still want to use, that means you should know enough to extend this class or write
        /// your own Search class.
        /// </summary>
        public override IEnumerable<T> Search(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState)
        {
            closed.Clear();
            return base.Search(problem, fringe, initialState);
        }

        protected override void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem)
        {
            if (!closed.Contains(node.State))
            {
                closed.Add(node.State);
                fringe.AddRange(Expand(problem, node));
            }
        }
    }
}
