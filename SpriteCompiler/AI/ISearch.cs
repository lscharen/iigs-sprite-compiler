namespace SpriteCompiler.AI
{
    using System.Collections.Generic;

    public interface ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        /// Perform a new search on the specified search problem using the given
        /// initial state as a starting point.  The method will return an empty
        /// list on failure.
        IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState);

        /**
         * Continues to run a search after a solution is found.  This can
         * be useful for enumerating over all the solutions to a search problem.
         *
         * @param problem
         * @return Sequence of search nodes desribing a solution
         */
        IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem);

    }
}
