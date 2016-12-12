namespace SpriteCompiler.AI
{
    using System.Collections.Generic;
    using System.Linq;
    using Queue;
   
    /// <summary>
    /// An abstract description of a state-space search.  Specific algorthims are determined by 
    /// how the nodes are expanded, evaluated and enqueued.
    /// 
    /// The description of the AI problem is delegated to the ISearchProblem interface.
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public abstract class AbstractSearchStrategy<A, S, T, C> : ISearchStrategy<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : ICost<C>
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

        public AbstractSearchStrategy(INodeExpander<A, S, T, C> expander)
        {
            this.expander = expander;
        }

        public INodeExpander<A, S, T, C> Expander { get { return expander; } set { expander = value; } }

        /// <summary>
        /// Helper method to walk a solution node to the root and then reverse the list
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public IEnumerable<T> Solution(T node)
        {
            var sequence = new List<T>();
            
            for (var curr = node; node != null; node = node.Parent)
            {
                sequence.Add(curr);
            }

            sequence.Reverse();

            return sequence;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="problem"></param>
        /// <param name="fringe">Must be initialize -- usually that means being empty</param>
        /// <param name="initialState"></param>
        /// <returns></returns>
        public virtual IEnumerable<T> Search(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState)
        {
            // Add the initial state to the fringe
            fringe.Enqueue(Expander.CreateNode(initialState));

            // Search until success, or the search space is exhausted
            while (!fringe.Empty)
            {
                var node = fringe.Remove();

                if (problem.IsGoal(node.State))
                {
                    return Solution(node);
                }

                AddNodes(fringe, node, problem);
            }

            return Enumerable.Empty<T>();            
        }        

        /// <summary>
        /// When it's time to actually expand a node and add the new states to the fringe, different
        /// algorhtms can make different choices
        /// </summary>
        /// <param name="fringe"></param>
        /// <param name="node"></param>
        /// <param name="problem"></param>
        protected abstract void AddNodes(IQueue<T> fringe, T node, ISearchProblem<A, S, C> problem);
    }
}
