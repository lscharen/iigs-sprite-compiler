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
            var search = SpriteGeneratorSearchProblem.Create(100); // max budget of 100 cycles
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

            //    TCS		; 2 cycles
            //    LDA	04,s	; 5 cycles
            //    AND	#$0F00	; 3 cycles
            //    ORA	#$1011	; 3 cycles
            //    STA	04,s	; 5 cycles
            //    LDA	#$1111	; 3 cycles
            //    STA	03,s	; 5 cycles
            //    STA	A2,s	; 5 cycles
            //    LDA	A4,s	; 5 cycles
            //    AND	#$0F00	; 3 cycles
            //    ORA	#$2011	; 3 cycles
            //    STA	A4,s	; 5 cycles
            //    TSC		; 2 cycles
            //    ADC	#321	; 3 cycles
            //    TCS		; 2 cycles
            //    LDA	00,s	; 5 cycles
            //    AND	#$00F0	; 3 cycles
            //    ORA	#$1101	; 3 cycles
            //    STA	00,s	; 5 cycles
            //    LDA	03,s	; 5 cycles
            //    AND	#$0F00	; 3 cycles
            //    ORA	#$2012	; 3 cycles
            //    STA	03,s	; 5 cycles
            //    LDA	#$1211	; 3 cycles
            //    STA	02,s	; 5 cycles
            //; Total Cost = 94 cycles

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
        }

        [TestMethod]
        public void TestLines_1_To_4()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create(200); // max budget of 200 cycles
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
                new SpriteByte(0x20, 0x0F, 325),

                new SpriteByte(0x01, 0xF0, 481),
                new SpriteByte(0x11, 0x00, 482),
                new SpriteByte(0x11, 0x00, 483),
                new SpriteByte(0x11, 0x00, 484),
                new SpriteByte(0x11, 0x00, 485),
                new SpriteByte(0x11, 0x00, 486)
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
