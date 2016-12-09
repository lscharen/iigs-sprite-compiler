using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class IterativeDeepeningAStarSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>, new()
    {
        protected readonly AbstractAISearch<A, S, T, C> search;
        protected readonly C limit;

        public IterativeDeepeningAStarSearch(AbstractAISearch<A, S, T, C> search, C limit)
        {
            this.search = search;
            this.limit = limit;
        }

        public IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            C bound = new C();
            while (bound.CompareTo(limit) < 0)
            {
                var dls = new DepthLimitedSearch<A, S, T, C>(search, new CostNodeLimiter<T, C>(bound));
                var result = dls.Search(problem, initialState);

                if (!dls.IsCutoff(result))
                {
                    return result;
                }
            }

            // An empty list signals failure
            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem)
        {
            throw new NotImplementedException();
        }
    }
}
