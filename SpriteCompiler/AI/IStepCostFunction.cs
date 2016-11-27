namespace SpriteCompiler.AI
{
    using System;

    public interface IStepCostFunction<A, S, C> where C : IPathCost<C>
    {
        C StepCost(S fromState, A action, S toState);
    }
}
