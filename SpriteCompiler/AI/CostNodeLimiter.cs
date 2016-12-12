namespace SpriteCompiler.AI
{
    /// <summary>
    ///  Class that taken a search node and detemines whether or not to terminate the
    /// search at the node.  This is different than a goal test and is used in the
    /// contect of depth-limited searches.
    /// </summary>
    public class CostNodeLimiter<T, C> : INodeLimiter<T, C>
        where T : ISearchNode<C>
        where C : ICost<C>, new()
    {
        private readonly C maxCost;
        private C nextCost;

        public CostNodeLimiter(C maxCost, C infinity)
        {
            this.maxCost = maxCost;
            this.nextCost = infinity;
        }

        public C NextCost { get { return nextCost; } }

        public bool Cutoff(T node)
        {
            // If we find a value that exceeds the current maximum, return false,
            // but keep track of the smallest value that is larger than the maximum
            // cost.
            if (node.PathCost.CompareTo(maxCost) > 0)
            {
                nextCost = (node.PathCost.CompareTo(nextCost) < 0) ? node.PathCost : nextCost;
                return true;
            }

            return false;
        }
    }
}
