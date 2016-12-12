using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class AStarComparator<A, S, T, C> : IComparer<T>
        where T : HeuristicSearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public int Compare(T x, T y)
        {
            var cost1 = x.Heuristic.Add(x.PathCost);
            var cost2 = y.Heuristic.Add(y.PathCost);

            return cost1.CompareTo(cost2);
        }
    }
}
