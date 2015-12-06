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

            cpu.RunClock();
            Assert.AreNotEqual(10 << 2, cpu.GetPC());
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

        [TestMethod]
        public void TestJr()
        {
            var cpu = new CPU();
            cpu.RegWrite(2, 10 << 2); //$2 = 40

            var instructionAdd = new JR("jr $2", 0, 2);

            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("jr $2", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("JR: rs = $2", instructionAdd.GetDecode(), false);

            //Jump should have been taken
            Assert.AreEqual(10 << 2, cpu.GetPC());
        }

        [TestMethod]
        public void TestJumpProcedure()
        {
            var cpu = new CPU();

            var instructionAdd = new JumpProcedure("jp 10", 0, 10);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("jp 10", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("JP: imm = 10", instructionAdd.GetDecode(), false);

            //Jump should have been taken
            Assert.AreEqual(10 << 2, cpu.GetPC());

            //Check stack contents
            Assert.AreEqual(1 << 2, cpu.StackPeek());
        }

        [TestMethod]
        public void TestReturnProcedure()
        {
            var cpu = new CPU();
            cpu.StackPush(10 << 2);

            var instructionAdd = new ReturnProcedure("rp", 0);

            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("rp", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("RP: ", instructionAdd.GetDecode(), false);

            //Jump should have been taken
            Assert.AreEqual(10 << 2, cpu.GetPC());
        }

        [TestMethod]
        public void TestLW()
        {
            var cpu = new CPU();
            cpu.Store(5 << 2, 123456789);
            cpu.RegWrite(6, 4 << 2);

            var instructionAdd = new LW("lw $5, 4($6)", 0, 5, 4, 6);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("lw $5, 4($6)", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("LW: rs = $6, rt = $5, imm = 4", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("LW Address = 16 + 4 = 20", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("Memory access result = 123456789", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $5 <= 123456789", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(123456789, cpu.RegRead(5));
        }

        [TestMethod]
        public void TestSW()
        {
            var cpu = new CPU();
            cpu.RegWrite(5, 123456789);
            cpu.RegWrite(6, 4 << 2);

            var instructionAdd = new SW("sw $5, 4($6)", 0, 5, 4, 6);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("sw $5, 4($6)", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("SW: rs = $6, rt = $5, imm = 4", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("SW Address = 16 + 4 = 20", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("Value written in memory = 123456789", instructionAdd.GetMem(), false);

            //Test value inside memory file
            Assert.AreEqual(123456789, cpu.Load((4 << 2) + 4));
        }

        [TestMethod]
        public void TestXor()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 1565861035);
            cpu.RegWrite(3, 882150058);

            var instructionAdd = new Xor("xor $1, $2, $3", 0, 1, 2, 3);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("xor $1, $2, $3", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("Xor: rd = $1, rs = $2, rt = $3", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("Xor 1565861035 ^ 882150058 = 1774300673", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $1 <= 1774300673", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(1774300673, cpu.RegRead(1));
        }

        [TestMethod]
        public void TestSlt()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 100);
            cpu.RegWrite(3, 200);

            var instructionAdd = new Slt("slt $1, $2, $3", 0, 1, 2, 3);
            cpu.AddInstruction(instructionAdd);

            cpu.RunClock();
            Assert.AreEqual("slt $1, $2, $3", instructionAdd.GetFetch(), false);

            cpu.RunClock();
            Assert.AreEqual("Slt: rd = $1, rs = $2, rt = $3", instructionAdd.GetDecode(), false);

            cpu.RunClock();
            Assert.AreEqual("Slt 100 < 200 = 1", instructionAdd.GetExecute(), false);

            cpu.RunClock();
            Assert.AreEqual("None", instructionAdd.GetMem(), false);

            cpu.RunClock();
            Assert.AreEqual("Register $1 <= 1", instructionAdd.GetWriteback(), false);

            //Test value inside register file
            Assert.AreEqual(1, cpu.RegRead(1));
        }
    }
}
