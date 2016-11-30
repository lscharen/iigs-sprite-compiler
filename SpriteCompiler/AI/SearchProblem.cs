using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.AI
{
    public class SearchProblem<A, S, C> : ISearchProblem<A, S, C>
        where C : IPathCost<C>
    {
        private readonly IGoalTest<S> goalTest;
        private readonly IStepCostFunction<A, S, C> stepCost;
        private readonly ISuccessorFunction<A, S> successorFn;
        private readonly IHeuristicFunction<S, C> heuristicFn;

        public SearchProblem(IGoalTest<S> goalTest, IStepCostFunction<A, S, C> stepCost, ISuccessorFunction<A, S> successor, IHeuristicFunction<S, C> heuristicFn)
        {
            this.goalTest = goalTest;
            this.stepCost = stepCost;
            this.successorFn = successor;
            this.heuristicFn = heuristicFn;
        }
        
        public IEnumerable<Tuple<A, S>> Successors(S state)
        {
            return successorFn.Successors(state);
        }

        public bool IsGoal(S state)
        {
            return goalTest.IsGoal(state);
        }

        public C StepCost(S fromState, A action, S toState)
        {
            return stepCost.StepCost(fromState, action, toState);
        }

        public C Heuristic(S state)
        {
            return heuristicFn.Eval(state);
        }
    }
}
