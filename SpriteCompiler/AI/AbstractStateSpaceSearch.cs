namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;
    using Queue;

    public abstract class AbstractStateSpaceSearch<A, S, T, C> : ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        protected readonly ISearchStrategy<A, S, T, C> strategy;
        private readonly Func<IQueue<T>> fringe;

        public AbstractStateSpaceSearch(ISearchStrategy<A, S, T, C> strategy, Func<IQueue<T>> fringe)
        {
            this.strategy = strategy;
            this.fringe = fringe;                
        }

        public virtual IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState)
        {
            return strategy.Search(problem, fringe(), initialState);
        }
    }
}
