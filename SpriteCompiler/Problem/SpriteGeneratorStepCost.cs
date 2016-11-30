namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorStepCost : IStepCostFunction<CodeSequence, SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost StepCost(SpriteGeneratorState fromState, CodeSequence action, SpriteGeneratorState toState)
        {
            return action.CycleCount;
        }
    }
}
