namespace SpriteCompiler.AI
{
    using Adapters;
    using System.Collections.Generic;

    public class BestFirstSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        protected readonly AbstractAISearch<A, S, T, C> search;
        protected readonly IQueue<T> fringe;

        public BestFirstSearch(AbstractAISearch<A, S, T, C> search, IQueue<T> fringe)
        {
            this.search = search;
            this.fringe = fringe;
        }

        public BestFirstSearch(AbstractAISearch<A, S, T, C> search)
        {
            this.search = search;
            this.fringe = new QueueAdapter<T, C>();
        }

        public IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            fringe.Clear();
            return search.Search(problem, fringe, initialState);
        }

        public IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem)
        {
            return search.ExtendSearch(problem, fringe);
        }

        public void InitializeSearch(S initialState)
        {
            search.InitializeSearch(fringe, initialState);
        }

        public ISearchStepInfo<T> SearchStep(ISearchProblem<A,S,C> problem)
        {
            return search.SearchStep(problem, fringe);
        }
    }
}
