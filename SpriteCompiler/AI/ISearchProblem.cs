namespace SpriteCompiler.AI
{
    using System;
    using System.Collections.Generic;

    public interface ISearchProblem<A, S, C>
        where C : ICost<C>
    {
        IEnumerable<Tuple<A, S>> Successors(S state);
        bool IsGoal(S state);
        C StepCost(S fromState, A action, S toState);
        C Heuristic(S state);
    }
}
