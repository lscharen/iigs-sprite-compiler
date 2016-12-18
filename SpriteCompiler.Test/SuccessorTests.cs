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
    public class SuccessorTests
    {
        private SpriteGeneratorSuccessorFunction successor = new SpriteGeneratorSuccessorFunction();

        [TestMethod]
        public void TestInitialState()
        {
            var data = new[]
            {
                new SpriteByte(0x11, 0),
                new SpriteByte(0x11, 160),
                new SpriteByte(0x11, 320)
            };

            var state = SpriteGeneratorState.Init(data);
            var successors = successor.Successors(state);

            // Should pick only the first data item
            Assert.AreEqual(1, successors.Count());
            Assert.AreEqual(successors.First().Item1.GetType(), typeof(MOVE_STACK));
        }
    }
}