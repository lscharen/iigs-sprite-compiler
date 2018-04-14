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
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestEmptySprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var initialState = SpriteGeneratorState.Init(Enumerable.Empty<SpriteByte>());
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert : The initial state IS the goal state
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(1, solution.Count());
        }

        [TestMethod]
        public void TestSingleByteSprite()
        {
            Trace.WriteLine("Testing a sprite with just one byte");

            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0xAA };

            var initialState = SpriteGeneratorState.Init(data);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // The fastest way to draw a single byte at the current location should be
            //
            // TCS          
            // SHORT A
            // LDA #$AA
            // PHA
            // LONG A     = 13 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(5, solution.Count());
            Assert.AreEqual(13, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestSingleWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0xAA, 0x55 };

            var initialState = SpriteGeneratorState.Init(data);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // The fastest way to draw a single word at the current location should be
            //
            // TCS          
            // LDA #$55AA
            // STA 0,s     = 10 cycles
            //
            // Alternate
            //
            // ADC #1
            // TCS
            // PEA #$55AA  = 10 cycles
            
            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(3, solution.Count());
            Assert.AreEqual(10, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestOverlappingWrite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0x11, 0x22, 0x22 };

            var initialState = SpriteGeneratorState.Init(data);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // Solution should be 18 cycles
            //
            // TCS          
            // LDA #$2211
            // STA 0,s   
            // LDA #$2222
            // STA 1,s    = 18 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(18, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestSingleByteWithMask()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0x00, 0x11 };
            var mask = new byte[] { 0xFF, 0x00 };

            var initialState = SpriteGeneratorState.Init(data, mask);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // Solution should be same as regular single-byte sprite, except
            // the store happens at offset 1, instead of 0.

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(5, solution.Count());
            Assert.AreEqual(14, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestSinglePixelMask()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0x01, 0x11 };
            var mask = new byte[] { 0xF0, 0x00 };

            var initialState = SpriteGeneratorState.Init(data, mask);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // Solution should be a single 16-bit RMW
            //
            // TCS
            // LDA 0,s
            // AND #$00F0
            // ORA #$1101
            // STA 0,s     = 18 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(18, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestConsecutiveWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0xAA, 0x55, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };

            var initialState = SpriteGeneratorState.Init(data);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // The fastest way to draw a consecutive words should be
            //
            // ADC #7
            // TCS          
            // PEA $6655
            // PEA $4433
            // PEA $2211
            // PEA $55AA   = 25 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(6, solution.Count());
            Assert.AreEqual(25, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestConsecutiveWordSpriteWithMask()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new byte[] { 0x01, 0x11, 0x22, 0x11, 0x33 };
            var mask = new byte[] { 0xF0, 0x00, 0x00, 0x00, 0x00 };

            var initialState = SpriteGeneratorState.Init(data, mask);
            var initialHeuristic = problem.Heuristic(initialState);

            var solution = search.Search(problem, initialState);

            // Assert
            //
            // The fastest way to render this data should be
            //
            // ADC #4
            // TCS
            // PEA $3311
            // PEA $2211
            // LDA 0,s
            // AND #$00F0
            // ORA #$1101
            // STA 0,s   = 31 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(31, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestThreeLineSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var data = new[]
            {
                new SpriteByte(0x11, 0),
                new SpriteByte(0x11, 160),
                new SpriteByte(0x11, 320)
            };

            var initialState = SpriteGeneratorState.Init(data);
            var initialHeuristic = problem.Heuristic(initialState);
            
            var solution = search.Search(problem, initialState);

            // Current best solution
            //
            //	TCS		; 2 cycles
            //	SEP	#$10	; 3 cycles
            //	LDA	#$11	; 2 cycles
            //	PHA	        ; 3 cycles
            //	STA	A1,s	; 4 cycles
            //	REP	#$10	; 3 cycles
            //	TSC		    ; 2 cycles
            //	ADC	#321	; 3 cycles
            //	TCS		    ; 2 cycles
            //	SEP	#$10	; 3 cycles
            //	LDA	#$11	; 2 cycles
            //	PHA      	; 3 cycles
            //	REP	#$10	; 3 cycles
            //; Total Cost = 35 cycles
            //
            // Once other register caching becomes available, this should be able to be improved to
            //
            //	TCS		    ; 2 cycles
            //	SEP	#$20	; 3 cycles
            //	LDX	#$11	; 2 cycles
            //	PHX	        ; 3 cycles
            //  ADC #160    ; 3 cycles
            //  TCS         ; 2 cycles
            //  PHX         ; 3 cycles
            //  ADC #161    ; 3 cycles
            //  TCS         ; 2 cycles
            //  PHX         ; 3 cycles
            //  REP #$20    ; 3 cycles
            //; Total Cost = 29 cycles

            // Write out the solution
            WriteOutSolution(solution, initialHeuristic);

            initialHeuristic.Should().BeLessOrEqualTo(solution.Last().PathCost);
            Assert.AreEqual(35, (int)solution.Last().PathCost);
        }

        private void WriteOutSolution(IEnumerable<SpriteGeneratorSearchNode> solution, IntegerCost h0 = null)
        {
            if (!solution.Any())
            {
                Trace.WriteLine("No solution found");
                return;
            }

            foreach (var step in solution.ToList().Skip(1))
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
