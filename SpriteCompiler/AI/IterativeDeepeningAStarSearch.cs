using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class IterativeDeepeningAStarSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : IHeuristicSearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        private static readonly C Cost = new C();

        protected readonly ISearchStrategy<A, S, T, C> search;
        protected readonly C limit;

        public IterativeDeepeningAStarSearch(ISearchStrategy<A, S, T, C> search, C limit)
        {
            this.search = search;
            this.limit = limit;
        }

        public IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            C bound = Cost.Zero();
            while (bound.CompareTo(limit) < 0)
            {
                var limiter = new CostNodeLimiter<T, C>(bound, Cost.Maximum());
                var dls = new DepthLimitedSearch<A, S, T, C>(search, limiter);
                var result = dls.Search(problem, initialState);

                // If there was no cutoff, return the solution (or lack thereof)                
                if (!dls.IsCutoff(result))
                {
                    return result;
                }

                // If the cost did not change, throw exception
                if (bound.Equals(limiter.NextCost))
                {
                    throw new ApplicationException("IDA*: Bound did not increase after depth-limited search");
                }

                // Otherwise, increase the cutoff to the next value
                bound = limiter.NextCost;
            }

            // An empty list signals failure
            return Enumerable.Empty<T>();
        }
    }
}
