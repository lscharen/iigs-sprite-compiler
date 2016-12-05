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
    public class HeuristicTests
    {
        private SpriteGeneratorHeuristicFunction heuristic = new SpriteGeneratorHeuristicFunction();

        [TestMethod]
        public void TestSmallGap()
        {
            // Create a test with $XX -- -- $XX with the Accumulator loaded with $XX. Optimal code is
            //                     ^
            // STA 3,s
            // PHA     = 7 cycles

            var state = new SpriteGeneratorState(new[] { new SpriteByte(0x11, 0), new SpriteByte(0x11, 3)})
            {
                A = Register.Constant(0x0011),
                S = Register.INITIAL_OFFSET,
                P = SpriteGeneratorState.LONG_I
            };

            var h = heuristic.Eval(state);

            h.Should().BeLessOrEqualTo(7);
        }
    }
}
