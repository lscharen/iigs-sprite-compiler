namespace SpriteCompiler.AI
{
    public class AStarSearch<A, S, T, C> : BestFirstSearch<A, S, T, C>
        where T : HeuristicSearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        public AStarSearch(AbstractAISearch<A, S, T, C> search)
            : base(search, new AStarComparator<A, S, T, C>())
        {
        }

        public AStarSearch(AbstractAISearch<A, S, T, C> search, IQueue<T> fringe)
            : base(search, fringe)
        {
        }
    }
}
