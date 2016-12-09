using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    /// <summary>
    /// Depth-first search with a cutoff
    /// </summary>
    public class DepthLimitedSearch<A, S, T, C> : DepthFirstSearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>, new()
    {
        protected readonly INodeLimiter<T, C> limit;

        public DepthLimitedSearch(AbstractAISearch<A, S, T, C> search, INodeLimiter<T, C> limit)
            : base(search)
        {
            this.limit = limit;
        }

        public override IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            // Save the old node expander
            var oldExpander = search.Expander;

            // Wrap the expander with a depth-limied expanded in order to
            // terminate the search
            var expander = new DepthLimitedNodeExpander<A, S, T, C>(oldExpander, limit);
            search.Expander = expander;

            // Run the search
            var solution = base.Search(problem, initialState);

            // Restore the old expander
            search.Expander = oldExpander;
            
            // Check to see we failed and if the reason for failing was not reaching the cutoff depth.
            if (search.IsFailure(solution) && expander.CutoffOccured)
            {
                return null;
            }

            return solution;

        }

        public override IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem)
        {
            return base.ExtendSearch(problem);
        }

        public bool IsCutoff(IEnumerable<T> result)
        {
            return result == null;
        }
    }
}
