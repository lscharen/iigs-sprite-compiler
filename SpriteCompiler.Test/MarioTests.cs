using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpriteCompiler.Problem;
using SpriteCompiler.AI;
using System.Diagnostics;
using System.Collections.Generic;
using FluentAssertions;

namespace SpriteCompiler.Test
{
    /// <summary>
    /// Incremental tests that build a 16x32 mario sprite one line at a time
    /// </summary>
    [TestClass]
    public class MarioTests
    {
        [TestMethod]
        public void TestFirstLine()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();
            var sprite = new List<SpriteByte>
            {
                new SpriteByte(0x11, 0x00, 3),
                new SpriteByte(0x11, 0x00, 4),
                new SpriteByte(0x10, 0x0F, 5)
            };

            // Act : solve the problem
            var solution = search.Search(problem, SpriteGeneratorState.Init(sprite));

            // Assert : The initial state IS the goal state
            WriteOutSolution(solution);
        }

        [TestMethod]
        public void TestLines_1_To_2()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();
            var sprite = new List<SpriteByte>
            {
                new SpriteByte(0x11, 0x00, 3),
                new SpriteByte(0x11, 0x00, 4),
                new SpriteByte(0x10, 0x0F, 5),

                new SpriteByte(0x11, 0x00, 162),
                new SpriteByte(0x11, 0x00, 163),
                new SpriteByte(0x11, 0x00, 164),
                new SpriteByte(0x20, 0x0F, 165),
            };

            // Act : solve the problem
            var initialState = SpriteGeneratorState.Init(sprite);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert : The initial state IS the goal state (47 cycles is the currrent best solution)
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
        }

        [TestMethod]
        public void TestLines_1_To_3()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create(80); // max budget of 80 cycles
            var sprite = new List<SpriteByte>
            {
                new SpriteByte(0x11, 0x00, 3),
                new SpriteByte(0x11, 0x00, 4),
                new SpriteByte(0x10, 0x0F, 5),

                new SpriteByte(0x11, 0x00, 162),
                new SpriteByte(0x11, 0x00, 163),
                new SpriteByte(0x11, 0x00, 164),
                new SpriteByte(0x20, 0x0F, 165),

                new SpriteByte(0x01, 0xF0, 321),
                new SpriteByte(0x11, 0x00, 322),
                new SpriteByte(0x11, 0x00, 323),
                new SpriteByte(0x12, 0x00, 324),
                new SpriteByte(0x20, 0x0F, 325)
            };

            // Act : solve the problem
            var initialState = SpriteGeneratorState.Init(sprite);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert : The initial state IS the goal state
            WriteOutSolution(solution);
        }

        private void WriteOutSolution(IEnumerable<SpriteGeneratorSearchNode> solution, IntegerCost h0 = null)
        {
            foreach (var step in solution.Skip(1))
            {
                Trace.WriteLine(step.Action.Emit());
            }

            Trace.WriteLine(string.Format("; Total Cost = {0} cycles", (int)solution.Last().PathCost));

            if (h0 != null)
            {
                Trace.WriteLine(string.Format("; h(0) = {0} cycles", (int)h0));
            }
        }
    }
}
