namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
   
    /// <summary>
    /// An abstract description of a state-space search.  Specific algorthims are determined by 
    /// how the nodes are expanded, eveanluated and enqueued.
    /// 
    /// The description of the AI problem is delegated to the ISearchProblem interface.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public abstract class AbstractAISearch<A, S, T, C> : ISearch<A, S, T, C>
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
        private readonly IQueue<T> fringe;

        public AbstractAISearch(INodeExpander<A, S, T, C> expander, IQueue<T> fringe)
        {
            this.expander = expander;
            this.fringe = fringe;
        }

        public INodeExpander<A, S, T, C> Expander { get { return expander; } set { expander = value; } }

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

        public IEnumerable<T> ExtendSearch(ISearchProblem<A, S, C> problem)
        {
            while (!fringe.Empty)
            {
                var step = SearchStep(problem, fringe);
                if (step.IsGoal)
                {
                    return Solution(step.Node);
                }
            }

            return Enumerable.Empty<T>();
        }

        public ISearchStepInfo<T> SearchStep(ISearchProblem<A, S, C> problem)
        {
            var node = fringe.Remove();

            #if DEBUG
                Console.WriteLine(string.Format("Removed {0} from the queue with g = {1}, c(n, n') = {2}", node.State, node.PathCost, node.StepCost));
            #endif

            if (problem.IsGoal(node.State))
            {
                return new SearchStepInfo<T>(node, Solution(node));
            }

            AddNodes(fringe, node, problem);

            return new SearchStepInfo<T>(node, null);
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
            InitializeSearch(fringe, initialState);
            return ExtendSearch(problem, fringe);
        }

        public void InitializeSearch(IQueue<T> fringe, S initialState)
        {
            fringe.Clear();
            fringe.Enqueue(expander.CreateNode(default(T), initialState));
        }

        protected abstract void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem);
    }
}
