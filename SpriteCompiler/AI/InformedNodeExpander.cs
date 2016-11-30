using System;
using System.Collections.Generic;

namespace SpriteCompiler.AI
{
    public abstract class InformedNodeExpander<A, S, T, C> : INodeExpander<A, S, T, C>
        where T : HeuristicSearchNode<A, S, T, C>
        where C : IPathCost<C>, new()
    {
        public abstract T CreateNode(T parent, S state);

        public IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            foreach (var successor in problem.Successors(node.State))
            {
                var action = successor.Item1;
                var state = successor.Item2;
                var next = CreateNode(node, state);

                next.Action = action;
                next.StepCost = problem.StepCost(node.State, action, state);
                next.Heuristic = problem.Heuristic(state);

                yield return next;
            }
        }
    }
}
