namespace SpriteCompiler.Problem
{
    using SpriteCompiler.AI;

    public sealed class SpriteGeneratorSearchProblem
    {
        public static ISearchProblem<CodeSequence, SpriteGeneratorState, IntegerPathCost> CreateSearchProblem()
        {
            var goalTest = new SpriteGeneratorGoalTest();
            var stepCost = new SpriteGeneratorStepCost();
            var successors = new SpriteGeneratorSuccessorFunction();
            var heuristic = new SpriteGeneratorHeuristicFunction();

            return new SearchProblem<CodeSequence, SpriteGeneratorState, IntegerPathCost>(goalTest, stepCost, successors, heuristic);
        }

        public static ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost> Create()
        {
            var expander = new SpriteGeneratorNodeExpander();
            //var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(expander);
            var strategy = new GraphSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(expander);

            return new AStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(strategy);
        }

        public static ISearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost> Create(int maxCycles)
        {
            var expander = new SpriteGeneratorNodeExpander();
            //var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(expander);
            var strategy = new GraphSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(expander);

            var maxCost = (IntegerPathCost)maxCycles;
            return new IterativeDeepeningAStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(strategy, maxCost);
        }
    }
}
