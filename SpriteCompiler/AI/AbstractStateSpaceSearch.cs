using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public abstract class AbstractStateSpaceSearch<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        private readonly INodeExpander<A, S, T, C> expander;
        private readonly Func<IQueue<T>> fringe;

        public AbstractStateSpaceSearch(INodeExpander<A, S, T, C> expander, Func<IQueue<T>> fringe)
        {
            this.expander = expander;
            this.fringe = fringe;
        }

        public ISearchProblemInstance<A, S, T, C> Create(ISearchProblem<A, S, C> problem, S initialState)
        {
            return new SearchProblemInstance<A, S, T, C>(problem, fringe(), initialState);
        }
    }

    public interface ISearchProblemInstance<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        /// <summary>
        /// Tries to find a solution to the search problem.  Returns null if no solution can be found.
        /// This method can be called repeatedly to try to find additional, sub optimal solutions if 
        /// supported.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> FindSolution();

        /// <summary>
        /// Take a single step of the searth to allow introspection of the search process
        /// </summary>
        /// <param name="problem"></param>
        /// <returns></returns>
        ISearchStepInfo<T> Step();

        /// <summary>
        /// Provide a set of callbacks to watch the execution of a search
        /// </summary>
        // void Trace();
    }

    public class SearchProblemInstance<A, S, T, C> : ISearchProblemInstance<A, S, T, C>
        where T : ISearchNode<A, S, T, C>
        where C : IPathCost<C>
    {
        private readonly ISearchProblem<A, S, C> problem;
        private readonly S initialState;
        private readonly IQueue<T> fringe;

        public SearchProblemInstance(ISearchProblem<A, S, C> problem, IQueue<T> fringe, S initialState)
        {
            this.problem = problem;
            this.fringe = fringe;
            this.initialState = initialState;
        }

        public IEnumerable<T> FindSolution()
        {
            throw new NotImplementedException();
        }

        public ISearchStepInfo<T> Step()
        {
            var node = fringe.Remove();

            if (problem.IsGoal(node.State))
            {
                return new SearchStepInfo<T>(node, Solution(node));
            }

            AddNodes(fringe, node, problem);

            return new SearchStepInfo<T>(node, null);
        }

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

    }
}
