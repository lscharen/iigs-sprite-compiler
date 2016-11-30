namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost Eval(SpriteGeneratorState state)
        {
            return 0;
        }
    }
}
