using System;
using System.Collections.Generic;

namespace SpriteCompiler.AI
{
    using System.Linq;

    public abstract class InformedNodeExpander<A, S, T, C> : INodeExpander<A, S, T, C>
        where T : HeuristicSearchNode<A, S, T, C>
        where C : ICost<C>, new()
    {
        public abstract T CreateNode(T parent, S state);
        public abstract T CreateNode(S state);

        public IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            var successors = problem.Successors(node.State);

            // Debug
            #if VERBOSE_DEBUG
            Console.WriteLine(String.Format("There are {0} successors for {1}", successors.Count(), node));
            Console.WriteLine(String.Format("This node has a current path cost of {0}", node.PathCost));
            #endif

            foreach (var successor in successors)
            {
                var action = successor.Item1;
                var state = successor.Item2;
                var next = CreateNode(node, state);
                
                next.Action = action;                
                next.StepCost = problem.StepCost(node.State, action, state);
                next.Heuristic = problem.Heuristic(state);

#if False
                Console.WriteLine("   Action = " + next.Action + ", g(n') = " + next.PathCost + ", h(n') = " + next.Heuristic);
#endif
                yield return next;
            }
        }
    }
}
