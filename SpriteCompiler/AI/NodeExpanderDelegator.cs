namespace SpriteCompiler.AI
{
    using System.Collections.Generic;

    public class NodeExpanderDelegator<A, S, T, C> : INodeExpander<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        private readonly INodeExpander<A, S, T, C> expander;

        public NodeExpanderDelegator(INodeExpander<A, S, T, C> expander)
        {
            this.expander = expander;
        }

        public virtual IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            return expander.Expand(problem, node);
        }

        public virtual T CreateNode(T parent, S state)
        {
            return expander.CreateNode(parent, state);
        }

        public virtual T CreateNode(S state)
        {
            return expander.CreateNode(state);
        }
    }
}
