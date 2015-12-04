using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipSim;
using MipSim.Instructions;

namespace MipSimTest
{
    [TestClass]
    public class InstructionTests
    {
        [TestMethod]
        public void TestAdd()
        {
            //Set initial values in register file
            CPU.RegWrite(2, 10);
            CPU.RegWrite(3, 255);

            var instructionAdd = new Add("add $1, $2, $3", 0, 1, 2, 3);
            CPU.AddInstruction(instructionAdd);

            CPU.RunClock();
            Assert.AreEqual("add $1, $2, $3", instructionAdd.GetFetch(), false);

            CPU.RunClock();
            Assert.AreEqual("Add: rd = $1, rs = $2, rt = $3", instructionAdd.GetDecode(), false);

            CPU.RunClock();
            Assert.AreEqual("Add 10 + 255 = 265", instructionAdd.GetExecute(), false);

            CPU.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            CPU.RunClock();
            Assert.AreEqual("Register $1 <= 265", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(265, CPU.RegRead(1));
        }
        [TestMethod]
        public void TestAddi()
        {
            //Set initial values in register file
            CPU.RegWrite(2, 123456);

            var instructionAdd = new ADDI("addi $1, $2, 123456", 0, 1, 2, 123456);
            CPU.AddInstruction(instructionAdd);

            CPU.RunClock();
            Assert.AreEqual("addi $1, $2, 123456", instructionAdd.GetFetch(), false);

            CPU.RunClock();
            Assert.AreEqual("Addi: rd = $1, rs = $2, imm = 123456", instructionAdd.GetDecode(), false);

            CPU.RunClock();
            Assert.AreEqual("Add 123456 + 123456 = 246912", instructionAdd.GetExecute(), false);

            CPU.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            CPU.RunClock();
            Assert.AreEqual("Register $1 <= 246912", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(246912, CPU.RegRead(1));
        }
    }
}
