using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpriteCompiler.Problem;
using SpriteCompiler.AI;
using System.Diagnostics;

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
            foreach (var step in solution.Skip(1))
            {
                Trace.WriteLine(step.Action.ToString());
            }
        }

        /*
        [TestMethod]
        public void TestSingleWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var solution = search.Search(problem, new SpriteGeneratorState());

            // Assert : The initial state IS the goal state
            Assert.AreEqual(1, solution.Count());
        }
        */
    }
}
