namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The NodeExpander class encapsulates a strategy of how the expand one state
    /// into its successor states.In the simplest case, a node's state is expanded
    /// using a problem-specific sucessor function, new search nodes are created and
    /// the list is returned.
    /// </summary>
    public interface INodeExpander<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node);

        T CreateNode(T parent, S state);
        T CreateNode(S state);
    }
}
