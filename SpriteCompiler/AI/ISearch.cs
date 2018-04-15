namespace SpriteCompiler.AI
{
    using System.Collections.Generic;

    public interface ISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        /// Perform a new search on the specified search problem using the given
        /// initial state as a starting point.  The method will return an empty
        /// list on failure.
        IEnumerable<T> Search(ISearchProblem<A, S, C> problem, S initialState);
    }
}
