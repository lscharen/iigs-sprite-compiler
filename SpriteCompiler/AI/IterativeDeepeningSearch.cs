using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class IterativeDeepeningSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        private readonly ISearchStrategy<A, S, T, C> search;
        private readonly int limit;

        public IterativeDeepeningSearch(ISearchStrategy<A, S, T, C> search, int limit)
        {
            this.search = search;
            this.limit = limit;
        }

        public IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            for (int depth = 1; depth <= limit; depth++)
            {
                var dls = new DepthLimitedSearch<A, S, T, C>(search, new DepthNodeLimiter<T, C>(depth));
                var result = dls.Search(problem, initialState);

                if (!dls.IsCutoff(result))
                {
                    return result;
                }
            }

            return Enumerable.Empty<T>();
        }        
    }
}
