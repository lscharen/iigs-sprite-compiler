using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpriteCompiler.Problem;
using SpriteCompiler.AI;

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
    }
}
