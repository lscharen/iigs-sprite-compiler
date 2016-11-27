namespace SpriteCompiler.AI
{
    public interface IGoalTest<S>
    {
        bool IsGoal(S state);
    }
}
