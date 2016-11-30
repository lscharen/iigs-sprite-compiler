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
    public class RegisterTests
    {
        [TestMethod]
        public void TestRegisterEquality()
        {
            var uninitialized1 = Register.UNINITIALIZED;
            var uninitialized2 = Register.UNINITIALIZED;

            Assert.AreEqual(uninitialized1, uninitialized2);

            var addr1 = Register.INITIAL_OFFSET;
            var addr2 = Register.INITIAL_OFFSET;
            var addr3 = Register.INITIAL_OFFSET.Add(1);
            var addr4 = addr3.Add(-1);

            Assert.AreEqual(addr1, addr2);
            Assert.AreNotEqual(addr2, addr3);
            Assert.AreEqual(addr1, addr4);

            var literal1 = Register.UNINITIALIZED.LoadConstant(0);
            var literal2 = Register.UNINITIALIZED.LoadConstant(1);
            var literal3 = Register.UNINITIALIZED.LoadConstant(1);

            Assert.AreNotEqual(literal1, literal2);
            Assert.AreNotEqual(literal1, addr1);
            Assert.AreNotEqual(literal1, uninitialized1);
            Assert.AreEqual(literal2, literal3);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestUninitializedAdd()
        {
            Register.UNINITIALIZED.Add(1);
        }
    }
}
