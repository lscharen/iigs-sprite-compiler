using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class DepthLimitedNodeExpander<A, S, T, C> : NodeExpanderDelegator<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>, new()
    {
        private readonly INodeLimiter<T, C> limit;
        private bool cutoffOccured = false;

        public DepthLimitedNodeExpander(INodeExpander<A, S, T, C> expander, INodeLimiter<T, C> limit)
            : base(expander)
        {
            this.limit = limit;
        }

        public bool CutoffOccured { get { return cutoffOccured; } }

        public override IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            if (limit.Cutoff(node))
            {
                cutoffOccured = true;
                return Enumerable.Empty<T>();
            }

            return base.Expand(problem, node);
        }
    }
}
