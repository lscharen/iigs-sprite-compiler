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
    public class StateTests
    {
        [TestMethod]
        public void TestStateEquivalence()
        {
            // States are used in HashSet, so we need to
            // test that two instances of the same state
            // are recognized as equivalent
            var state1 = new SpriteGeneratorState();
            var state2 = new SpriteGeneratorState();

            Assert.AreEqual(state1, state2);

            var set = new HashSet<SpriteGeneratorState>();
            set.Add(state1);

            Assert.IsTrue(set.Contains(state2));
        }
    }
}
