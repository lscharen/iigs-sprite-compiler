namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorStepCost : IStepCostFunction<CodeSequence, SpriteGeneratorState, IntegerCost>
    {
        public IntegerCost StepCost(SpriteGeneratorState fromState, CodeSequence action, SpriteGeneratorState toState)
        {
            return action.CycleCount;
        }
    }
}
