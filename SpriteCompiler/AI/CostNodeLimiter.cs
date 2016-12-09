namespace SpriteCompiler.AI
{
    /// <summary>
    ///  Class that taken a search node and detemines whether or not to terminate the
    /// search at the node.  This is different than a goal test and is used in the
    /// contect of depth-limited searches.
    /// </summary>
    public class CostNodeLimiter<T, C> : INodeLimiter<T, C>
        where T : ISearchNode<C>
        where C : IPathCost<C>, new()
    {
        private readonly C maxCost;

        public CostNodeLimiter(C maxCost)
        {
            this.maxCost = maxCost;
        }

        public bool Cutoff(T node)
        {
            return node.PathCost.CompareTo(maxCost) >= 0;
        }
    }
}
