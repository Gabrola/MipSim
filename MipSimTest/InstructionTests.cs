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
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 10);
            cpu.RegWrite(3, 255);

            var instructionAdd = new Add("add $1, $2, $3", 0, 1, 2, 3);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("add $1, $2, $3", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("Add: rd = $1, rs = $2, rt = $3", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("Add 10 + 255 = 265", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $1 <= 265", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(265, cpu.RegRead(1));
        }

        [TestMethod]
        public void TestAddi()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 123456);

            var instructionAdd = new Addi("addi $1, $2, 123456", 0, 1, 2, 123456);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("addi $1, $2, 123456", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("Addi: rd = $1, rs = $2, imm = 123456", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("Add 123456 + 123456 = 246912", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $1 <= 246912", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(246912, cpu.RegRead(1));
        }

        [TestMethod]
        public void TestJump()
        {
            var cpu = new CPU();

            var instructionAdd = new J("j 10", 0, 10);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("j 10", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("J: imm = 10", instructionAdd.GetDecode(), false);

            //Jump should have been taken
            Assert.AreEqual(10 << 2, cpu.GetPC());
        }

        [TestMethod]
        public void TestJal()
        {
            var cpu = new CPU();

            var instructionAdd = new Jal("jal 4", 0, 4);

            cpu.AddInstruction(instructionAdd);
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0)); //Should jump to this instruction

            cpu.RunClock();
            Assert.AreEqual("jal 4", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("Jal: imm = 4", instructionAdd.GetDecode(), false);

            //Jump should have been taken
            Assert.AreEqual(4 << 2, cpu.GetPC());

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $15 <= 4", instructionAdd.GetWriteback(), false); //4 = 1 << 2

            //Test value inside register file
            Assert.AreEqual(1 << 2, cpu.RegRead(15));
        }
    }
}
