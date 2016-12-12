namespace SpriteCompiler.AI
{
    using Queue;

    public class TreeSearch<A, S, T, C> : AbstractSearchStrategy<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        public TreeSearch(INodeExpander<A, S, T, C> expander)
            : base(expander)
        {
        }

        /// <summary>
        /// Generic tree search. See page 72 in Russell and Norvig
        /// </summary>
        protected override void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem)
        {
            fringe.AddRange(Expander.Expand(problem, node));
        }
    }
}
