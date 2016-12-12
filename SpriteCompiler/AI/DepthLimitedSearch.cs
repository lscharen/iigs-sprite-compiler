namespace SpriteCompiler.AI
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Depth-first search with a cutoff
    /// </summary>
    public class DepthLimitedSearch<A, S, T, C> : DepthFirstSearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        protected readonly INodeLimiter<T, C> limit;

        public DepthLimitedSearch(ISearchStrategy<A, S, T, C> strategy, INodeLimiter<T, C> limit)
            : base(strategy)
        {
            this.limit = limit;
        }

        public override IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            // Save the old node expander
            var oldExpander = strategy.Expander;

            // Wrap the expander with a depth-limied expanded in order to
            // terminate the search
            var expander = new DepthLimitedNodeExpander<A, S, T, C>(oldExpander, limit);
            strategy.Expander = expander;

            // Run the search
            var solution = base.Search(problem, initialState);

            // Restore the old expander
            strategy.Expander = oldExpander;
            
            // Check to see we failed and if the reason for failing was not reaching the cutoff depth.
            if (!solution.Any() && expander.CutoffOccured)
            {
                return null;
            }

            return solution;
        }

        public bool IsCutoff(IEnumerable<T> result)
        {
            return result == null;
        }
    }
}
