using SpriteCompiler.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpriteCompiler.Problem
{
    public sealed class IntegerPathCost : IPathCost<IntegerPathCost>
    {
        private readonly int value;

        private IntegerPathCost(int value)
        {
            this.value = value;
        }

        public static implicit operator int(IntegerPathCost obj)
        {
            return obj.value;
        }

        public static implicit operator IntegerPathCost(int value)
        {
            return new IntegerPathCost(value);
        }

        public IntegerPathCost Add(IntegerPathCost other)
        {
            return value + other.value;
        }

        public int CompareTo(IntegerPathCost other)
        {
            return value.CompareTo(other.value);
        }
    }

    public class SpriteGeneratorState
    {
        public SpriteGeneratorState()
        {
        }

        public SpriteGeneratorState(byte[][] data, byte[][] mask)
        {
        }

        public bool IsEmpty { get { return true; } }
    }

    public class SpriteGeneratorStepCost : IStepCostFunction<CodeSequence, SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost StepCost(SpriteGeneratorState fromState, CodeSequence action, SpriteGeneratorState toState)
        {
            return action.CycleCount;
        }
    }

    public class SpriteGeneratorGoalTest : IGoalTest<SpriteGeneratorState>
    {
        public bool IsGoal(SpriteGeneratorState state)
        {
            // We have reached our goal when there is no data left to display
            return state.IsEmpty;
        }
    }

    public class SpriteGeneratorSuccessorFunction : ISuccessorFunction<CodeSequence, SpriteGeneratorState>
    {
        public IDictionary<CodeSequence, SpriteGeneratorState> Successors(SpriteGeneratorState state)
        {
            return new Dictionary<CodeSequence, SpriteGeneratorState>();
        }
    }

    public class SpriteGeneratorHeuristicFunction : IHeuristicFunction<SpriteGeneratorState, IntegerPathCost>
    {
        public IntegerPathCost Eval(SpriteGeneratorState state)
        {
            return 0;
        }
    }

    public class SpriteGeneratorSearchNode : HeuristicSearchNode<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>
    {
        public SpriteGeneratorSearchNode(SpriteGeneratorSearchNode node, SpriteGeneratorState state)
            : base(node, state)
        {
        }
    }

    public class SpriteGeneratorNodeExpander : InformedNodeExpander<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>
    {
        public override SpriteGeneratorSearchNode CreateNode(SpriteGeneratorSearchNode parent, SpriteGeneratorState state)
        {
            return new SpriteGeneratorSearchNode(parent, state);
        }
    }

    public class SpriteGeneratorSearchProblem
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
            var problem = CreateSearchProblem();
            var strategy = new TreeSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(new SpriteGeneratorNodeExpander());

            return new AStarSearch<CodeSequence, SpriteGeneratorState, SpriteGeneratorSearchNode, IntegerPathCost>(strategy);
        }
    }
}
