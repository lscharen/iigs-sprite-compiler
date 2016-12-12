namespace SpriteCompiler.AI
{
    using Queue;

    public class DepthFirstSearch<A, S, T, C> : AbstractStateSpaceSearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public DepthFirstSearch(ISearchStrategy<A, S, T, C> strategy)
            : base(strategy, () => new LIFO<T>())
        {
        }
    }
}
