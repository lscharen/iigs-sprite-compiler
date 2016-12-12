namespace SpriteCompiler.AI
{
    using System.Collections.Generic;
    using Queue;

    /// <summary>
    /// A search strategy defines how a state space is explored by determining which
    /// nodes in the fringe are expanded next.  Two common search strategies are
    /// Tree Search and Graph Search.
    /// </summary>
    public interface ISearchStrategy<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        IEnumerable<T> Search(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState);
        INodeExpander<A, S, T, C> Expander { get; set; }
    }
}
