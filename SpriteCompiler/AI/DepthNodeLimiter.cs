namespace SpriteCompiler.AI
{
    public class DepthNodeLimiter<T, C> : INodeLimiter<T, C>
        where T : ISearchNode<C>
        where C : ICost<C>, new()
    {
        private readonly int maxDepth;

        public DepthNodeLimiter(int maxDepth)
        {
            this.maxDepth = maxDepth;
        }

        public bool Cutoff(T node)
        {
            return node.Depth >= maxDepth;
        }
    }
}
