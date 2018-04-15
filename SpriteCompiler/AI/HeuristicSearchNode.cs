namespace SpriteCompiler.AI
{
    using System;

    public interface IHeuristicSearchNodeWithMemory<A, S, T, C> : IHeuristicSearchNode<A, S, T, C> where C : ICost<C>
    {
        C F { get; set; }
    }
    public interface IHeuristicSearchNode<A, S, T, C> : ISearchNode<A, S, T, C> where C : ICost<C>
    {
    }

    public class HeuristicSearchNode<A, S, T, C> : AbstractSearchNode<A, S, T, C>, IHeuristicSearchNode<A, S, T, C>
        where T : IHeuristicSearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public HeuristicSearchNode(T node, S state)
            : base(node, state)
        {
            Heuristic = new C();
        }

        public C Heuristic { get; set; }

        public override C EstCost
        {
            get
            {
                return PathCost.Add(Heuristic);
            }
        }
    }
}
