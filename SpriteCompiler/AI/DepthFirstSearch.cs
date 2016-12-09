using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class DepthFirstSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>, new()
    {
        protected readonly AbstractAISearch<A, S, T, C> search;
        protected readonly IQueue<T> fringe = new Lifo<T>();

        public DepthFirstSearch(AbstractAISearch<A, S, T, C> search)
        {
            this.search = search;
        }

        public virtual IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            fringe.Clear();
            return search.Search(problem, fringe, initialState);
        }

        public virtual IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem)
        {
            return search.ExtendSearch(problem, fringe);
        }
    }
}
