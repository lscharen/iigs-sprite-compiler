﻿using System;
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
            var solution = search.Search(problem, SpriteGeneratorState.Init(Enumerable.Empty<SpriteByte>()));

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
            var solution = search.Search(problem, SpriteGeneratorState.Init(new byte[] { 0xAA }));

            // Assert
            //
            // The fastest way to draw a single byte at the current location should be
            //
            // TCS          
            // SHORT A
            // LDA #$AA
            // STA 0,s
            // LONG A     = 14 cycles

            // Write out the solution
            WriteOutSolution(solution);

            Assert.AreEqual(5, solution.Count());
            Assert.AreEqual(14, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestSingleWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var solution = search.Search(problem, SpriteGeneratorState.Init(new byte[] { 0xAA, 0x55 }));

            // Assert
            //
            // The fastest way to draw a single word at the current location should be
            //
            // TCS          
            // LDA #$55AA
            // STA 0,s     = 10 cycles

            // Write out the solution
            WriteOutSolution(solution);

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
            var solution = search.Search(problem, SpriteGeneratorState.Init(new byte[] { 0x11, 0x22, 0x22 }));

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
            WriteOutSolution(solution);

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

            var solution = search.Search(problem, SpriteGeneratorState.Init(data, mask));

            // Assert
            //
            // Solution should be same as regular single-byte sprite, except
            // the store happens at offset 1, instead of 0.

            // Write out the solution
            WriteOutSolution(solution);

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

            var solution = search.Search(problem, SpriteGeneratorState.Init(data, mask));

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
            WriteOutSolution(solution);

            Assert.AreEqual(18, (int)solution.Last().PathCost);
        }

        [TestMethod]
        public void TestConsecutiveWordSprite()
        {
            // Arrange
            var problem = SpriteGeneratorSearchProblem.CreateSearchProblem();
            var search = SpriteGeneratorSearchProblem.Create();

            // Act : solve the problem
            var solution = search.Search(problem, SpriteGeneratorState.Init(new byte[] { 0xAA, 0x55, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 }));

            // Assert
            //
            // The fastest way to draw a consecutive workds hould be
            //
            // ADC #7
            // TCS          
            // PEA $6655
            // PEA $4433
            // PEA $2211
            // PEA $55AA   = 25 cycles

            // Write out the solution
            WriteOutSolution(solution);

            Assert.AreEqual(6, solution.Count());
            Assert.AreEqual(25, (int)solution.Last().PathCost);
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
