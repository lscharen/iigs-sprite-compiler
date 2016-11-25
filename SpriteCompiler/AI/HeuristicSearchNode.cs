namespace SpriteCompiler.AI
{
    using System;

    public class HeuristicSearchNode<A, S, T, C> : AbstractSearchNode<A, S, T, C>, ISearchNode<A, S, T, C>
        where T : HeuristicSearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        public HeuristicSearchNode(T node, S state)
            : base(node, state)
        {
        }

        public C Heuristic { get; set; }
    }
}
