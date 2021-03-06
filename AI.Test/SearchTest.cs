﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpriteCompiler.AI;
using System.Diagnostics;
using SpriteCompiler.Adapters;

namespace AI.Test
{
    public class EightPuzzleNode : HeuristicSearchNode<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>
    {
        public EightPuzzleNode(EightPuzzleNode node, EightPuzzleBoard state) : base(node, state)
        {
        }
    }

    public class EightPuzzleGoalTest : IGoalTest<EightPuzzleBoard>
    {
        private readonly EightPuzzleBoard goal;

        public EightPuzzleGoalTest(EightPuzzleBoard goal)
        {
            this.goal = new EightPuzzleBoard(goal);
        }

        public bool IsGoal(EightPuzzleBoard state)
        {
            return state.Equals(goal);
        }
    }

    public class EightPuzzleSuccessorFunction : ISuccessorFunction<Direction, EightPuzzleBoard>
    {
        public IEnumerable<Tuple<Direction, EightPuzzleBoard>> Successors(EightPuzzleBoard board)
        {
            foreach (var direction in Enum.GetValues(typeof(Direction)).Cast<Direction>())
            {
                if (board.CanMoveGap(direction))
                {
                    yield return Tuple.Create(direction, new EightPuzzleBoard(board).MoveGap(direction));
                }
            }
        }
    }

    public class EightPuzzleStepCost : IStepCostFunction<Direction, EightPuzzleBoard, IntegerCost>
    {
        public IntegerCost StepCost(EightPuzzleBoard fromState, Direction action, EightPuzzleBoard toState)
        {
            return IntegerCost.ONE;            
        }
    }

    public class ManhattanHeuristic : IHeuristicFunction<EightPuzzleBoard, IntegerCost>
    {
        private readonly EightPuzzleBoard goal;

        public ManhattanHeuristic(EightPuzzleBoard goal)
        {
            this.goal = new EightPuzzleBoard(goal);
        }

        public IntegerCost Eval(EightPuzzleBoard state)
        {
            int cost = 0;

            for (int i = 1; i < 9; i++)
            {
                var goalLocation = goal.GetLocationOf(i);
                var stateLocation = state.GetLocationOf(i);

                cost += Math.Abs(goalLocation[0] - stateLocation[0]);
                cost += Math.Abs(goalLocation[1] - stateLocation[1]);
            }

            return cost;
        }
    }

    public class MisplacedHeuristic : IHeuristicFunction<EightPuzzleBoard, IntegerCost>
    {
        private readonly EightPuzzleBoard goal;

        public MisplacedHeuristic(EightPuzzleBoard goal)
        {
            this.goal = new EightPuzzleBoard(goal);
        }

        public IntegerCost Eval(EightPuzzleBoard state)
        {
            return goal.CountMismatches(state);
        }
    }

    public class EightPuzzleNodeExpander : InformedNodeExpander<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>
    {
        public override EightPuzzleNode CreateNode(EightPuzzleNode parent, EightPuzzleBoard state)
        {
            return new EightPuzzleNode(parent, state);
        }

        public override EightPuzzleNode CreateNode(EightPuzzleBoard state)
        {
            return CreateNode(null, state);
        }
    }

    [TestClass]
    public class SearchTest
    {
        // These are the three search problem to run using IDS, A*(h1) and A*(h2)
        private ISearchProblem<Direction, EightPuzzleBoard, IntegerCost> problem_none;
        private ISearchProblem<Direction, EightPuzzleBoard, IntegerCost> problem_h1;
        private ISearchProblem<Direction, EightPuzzleBoard, IntegerCost> problem_h2;

        [TestInitialize]
        public void SetUp()
        {           
            // These objects define the abstract search problem
            var goalTest = new EightPuzzleGoalTest(EightPuzzleBoard.GOAL);
            var stepCost = new EightPuzzleStepCost();
            var successorFn = new EightPuzzleSuccessorFunction();
            var heuristic1 = new MisplacedHeuristic(EightPuzzleBoard.GOAL);
            var heuristic2 = new ManhattanHeuristic(EightPuzzleBoard.GOAL);

            // Create three search problem objects.  One without a heuristic and two with the different
            // heuristics
            problem_none = new SearchProblem<Direction, EightPuzzleBoard, IntegerCost>(goalTest, stepCost, successorFn);
            problem_h1 = new SearchProblem<Direction, EightPuzzleBoard, IntegerCost>(goalTest, stepCost, successorFn, heuristic1);
            problem_h2 = new SearchProblem<Direction, EightPuzzleBoard, IntegerCost>(goalTest, stepCost, successorFn, heuristic2);
        }

        private void PrintSolution(IEnumerable<EightPuzzleNode> solution)
        {
            if (solution == null)
            {
                System.Diagnostics.Trace.WriteLine("No solution");
                return;
            }

            System.Diagnostics.Trace.WriteLine(solution.First().State.ToString());
            foreach (var node in solution.Skip(1))
            {
                System.Diagnostics.Trace.WriteLine("");
                System.Diagnostics.Trace.WriteLine(node.Action.ToString());
                System.Diagnostics.Trace.WriteLine(node.State.ToString());
            }
        }

        [TestMethod]
        public void EightPuzzleTest()
        {
            // Number of trials to run
            int N = 100;

            // Maximum depth of the solution
            int dmax = 10;

            // Now we define the search algorithm and the type of node expansion.  Russell & Norvig discuss
            // two type of expansion strategies: tree search and graph search.  One will avoid cycles in the search
            // space and the other will not.
            //
            // They state that a tree search was used to generate Figure 4.8;
            var expander = new InstrumentedNodeExpander<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>(new EightPuzzleNodeExpander());
            var treeSearch = new TreeSearch<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>(expander);

            var ids = new IterativeDeepeningSearch<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>(treeSearch, dmax);
            var aStarH1 = new AStarSearch<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>(treeSearch, () => new QueueAdapter<EightPuzzleNode, IntegerCost>());
            var aStarH2 = new AStarSearch<Direction, EightPuzzleBoard, EightPuzzleNode, IntegerCost>(treeSearch, () => new QueueAdapter<EightPuzzleNode, IntegerCost>());

            // Depth runs from 0 to dmax
            int[,] d = new int[dmax + 1, 3];
            int[,] n = new int[dmax + 1, 3];

            for (int _d = 1; _d <= dmax; _d++)
            {
                for (int i = 0; i < N; i++)
                {
                    // Invoke the search on the problem with a particular starting state. Scramble creates a new instance
                    var initialState = EightPuzzleBoard.GOAL.Scramble(_d);

                    {
                        expander.ClearMetrics();
                        var solution = ids.Search(problem_none, new EightPuzzleBoard(initialState));
                        var depth = solution.Count() - 1;

                        System.Diagnostics.Trace.WriteLine("IDS Solution has depth " + depth + " and expanded " + expander[IntrumentedParameters.NODES_EXPANDED] + " nodes");
                        d[depth, 0] += 1;
                        n[depth, 0] += expander[IntrumentedParameters.NODES_EXPANDED];
                    }

                    {
                        expander.ClearMetrics();
                        var solution = aStarH1.Search(problem_h1, new EightPuzzleBoard(initialState));
                        var depth = solution.Count() - 1;

                        System.Diagnostics.Trace.WriteLine("A* (h1) Solution has depth " + depth + " nodes and expanded " + expander[IntrumentedParameters.NODES_EXPANDED] + " nodes");
                        d[depth, 1] += 1;
                        n[depth, 1] += expander[IntrumentedParameters.NODES_EXPANDED];

                        // PrintSolution(solution);
                    }

                    {
                        expander.ClearMetrics();
                        var solution = aStarH2.Search(problem_h2, new EightPuzzleBoard(initialState));
                        var depth = solution.Count() - 1;

                        System.Diagnostics.Trace.WriteLine("A* (h2) Solution has depth " + depth + " nodes and expanded " + expander[IntrumentedParameters.NODES_EXPANDED] + " nodes");
                        d[depth, 2] += 1;
                        n[depth, 2] += expander[IntrumentedParameters.NODES_EXPANDED];

                        // PrintSolution(solution);
                    }
                }
            }
            Trace.WriteLine("|         Search Cost                Branching Factor       |");
            Trace.WriteLine("+--+---------+--------+--------++---------+--------+--------+");
            Trace.WriteLine("| d|   IDS   | A*(h1) | A*(h2) ||   IDS   | A*(h1) | A*(h2) |");
            Trace.WriteLine("+--+---------+--------+--------++---------+--------+--------+");

            for (int i = 1; i <= dmax; i++)
            {
                var bf0 = ComputeBranchingFactor((float)n[i, 0] / (float)d[i, 0], i);
                var bf1 = ComputeBranchingFactor((float)n[i, 1] / (float)d[i, 1], i);
                var bf2 = ComputeBranchingFactor((float)n[i, 2] / (float)d[i, 2], i);

                Trace.WriteLine(String.Format("|{0,2}|{1,-8} |{2,7} |{3,7} ||{4,8:0.00} |{5,7:0.00} |{6,7:0.00} |", i,
                                n[i, 0] / Math.Max(d[i, 0], 1), n[i, 1] / Math.Max(d[i, 1], 1), n[i, 2] / Math.Max(d[i, 2], 1), bf0, bf1, bf2));
            }

            Trace.WriteLine("+--+---------+--------+--------++---------+--------+--------+");
        }

        /// <summary>
        /// Uses Newton iteration to solve for the effective branching factor
        /// </summary>
        /// <param name="n">number of nodes expanded</param>
        /// <param name="d">depth of the solution</param>
        /// <returns></returns>
        private float ComputeBranchingFactor(float n, float d)
        {
            float x = 3.0f;   // Initial guess

            for (int i = 0; i < 20; i++)
            {
                float f = (float)Math.Pow(x, d + 1.0f) - 1.0f - x * n + n;
                float df = (d + 1.0f) * (float)Math.Pow(x, d) - n;

                x = x - (f / df);
            }

            return x;
        }

    }
}
