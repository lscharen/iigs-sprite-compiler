namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorSearchProblem
    {
        public static ISearchProblem<CodeSequence, SpriteGeneratorState, IntegerCost> CreateSearchProblem()
        {
            var goalTest = new SpriteGeneratorGoalTest();
            var stepCost = new SpriteGeneratorStepCost();
            var successors = new SpriteGeneratorSuccessorFunction();
            var heuristic = new SpriteGeneratorHeuristicFunction();

            return new SearchProblem<CodeSequence, SpriteGeneratorState, IntegerCost>(goalTest, stepCost, successors, heuristic);
        }

        public static ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost> Create()
        {
            var expander = new SpriteGeneratorNodeExpander();
            var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>(expander);
            var queue = new Adapters.QueueAdapter<SpriteGeneratorSearchNode, IntegerCost>();

            return new AStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>(strategy, queue);
        }

        public static ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost> Create(int maxCycles)
        {
            var expander = new SpriteGeneratorNodeExpander();
            var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>(expander);
            var queue = new Adapters.QueueAdapter<SpriteGeneratorSearchNode, IntegerCost>();

            var maxCost = (IntegerCost)maxCycles;
            return new IterativeDeepeningAStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerCost>(strategy, maxCost);
        }
    }
}
