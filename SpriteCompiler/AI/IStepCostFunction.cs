namespace SpriteCompiler.AI
{
    using System;

    public interface IStepCostFunction<A, S, C> where C : ICost<C>
    {
        C StepCost(S fromState, A action, S toState);
    }
}
