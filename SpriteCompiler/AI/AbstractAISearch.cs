namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public abstract class AbstractAISearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        // Conceptually the expander is responsible for two things:
        //
        //   1. Creating a node's successors via the Successor Function
        //      and wrapping them in SearchNodes (computing path costs
        //      and heuristics, as well).
        //
        //   2. Creating a search node from a state.  This lets us
        //      decouple the search algorithm from the state expansion

        private INodeExpander<A, S, T, C> expander;

        public AbstractAISearch(INodeExpander<A, S, T, C> expander)
        {
            this.expander = expander;
        }

        public INodeExpander<A, S, T, C> Expander { get { return expander; } }

        public IEnumerable<T> Solution(T node)
        {
            var sequence = new List<T>();

            while (node != null)
            {
                sequence.Add(node);
                node = node.Parent;
            }

            sequence.Reverse();

            return sequence;
        }

        public IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem, IQueue<T> fringe)
        {
            while (!fringe.Empty)
            {
                var node = fringe.Remove();
#if DEBUG
                Console.WriteLine(string.Format("Removed {0} from the queue with g = {1}, c(n, n') = {2}", node.State, node.PathCost, node.StepCost));
#endif
                if (problem.IsGoal(node.State))
                {
                    return Solution(node);
                }

                AddNodes(fringe, node, problem);
            }

            return Enumerable.Empty<T>();
        }

        public IEnumerable<T> Expand(ISearchProblem<A, S, C> problem, T node)
        {
            return expander.Expand(problem, node);
        }

        public bool IsFailure(IEnumerable<T> solution)
        {
            return !solution.Any();
        }

        public virtual IEnumerable<T> Search(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState)
        {
            fringe.Enqueue(expander.CreateNode(default(T), initialState));
            return ExtendSearch(problem, fringe);
        }

        protected abstract void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem);
    }
}
