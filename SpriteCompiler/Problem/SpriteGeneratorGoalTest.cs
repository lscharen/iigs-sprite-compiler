namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorGoalTest : IGoalTest<SpriteGeneratorState>
    {
        public bool IsGoal(SpriteGeneratorState state)
        {
            // We have reached our goal when there is no data left to display and we are back in 
            // 16-bit mode
            return state.IsEmpty && state.LongA && state.LongI;
        }
    }
}
