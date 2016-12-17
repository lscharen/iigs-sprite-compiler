using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public static class IntrumentedParameters
    {
        public const string NODES_EXPANDED = "nodesExpanded";
    }

    public class InstrumentedNodeExpander<A, S, T, C> : NodeExpanderDelegator<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
    {
        private readonly IDictionary<string, int> metrics = new Dictionary<string, int>();
        private readonly INodeExpander<A, S, T, C> expander;

        public InstrumentedNodeExpander(INodeExpander<A, S, T, C> expander)
            : base(expander)
        {
            ClearMetrics();
        }

        public override IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            metrics[IntrumentedParameters.NODES_EXPANDED] += 1;
            return base.Expand(problem, node);
        }

        public int this[string key]
        {
            get
            {
                return metrics[key];
            }
        }

        public void ClearMetrics()
        {
            metrics[IntrumentedParameters.NODES_EXPANDED] = 0;
        }
    }

}
