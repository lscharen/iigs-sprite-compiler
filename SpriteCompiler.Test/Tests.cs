using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpriteCompiler.Problem;
using SpriteCompiler.AI;
using System.Diagnostics;
using System.Collections.Generic;

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
            var solution = search.Search(problem, new SpriteGeneratorState());

            // Assert : The initial state IS the goal state
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
            var solution = search.Search(problem, new SpriteGeneratorState(new byte[] { 0xAA }));

            // Assert
            //
            // The fastest way to draw a single byte at the current location should be
            //
            // TCS          
            // SHORT A
            // LDA #$AA
            // STA 0,s
            // LONG A     = 14 cycles

            Assert.AreEqual(5, solution.Count());
            Assert.AreEqual(14, (int)solution.Last().PathCost);

            // Write out the solution
            WriteOutSolution(solution);
        }

        [TestMethod]
        public void TestSingleWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var solution = search.Search(problem, new SpriteGeneratorState(new byte[] { 0xAA, 0x55 }));

            // Assert
            //
            // The fastest way to draw a single word at the current location should be
            //
            // TCS          
            // LDA #$55AA
            // STA 0,s     = 10 cycles

            Assert.AreEqual(3, solution.Count());
            Assert.AreEqual(10, (int)solution.Last().PathCost);

            // Write out the solution
            WriteOutSolution(solution);
        }

        private void WriteOutSolution(IEnumerable<SpriteGeneratorSearchNode> solution)
        {
            foreach (var step in solution.Skip(1))
            {
                Trace.WriteLine(step.Action.ToString());
            }

            Trace.WriteLine(string.Format("; Total Cost = {0} cycles", (int)solution.Last().PathCost));
        }
    }
}
