using System;
using System.Linq;
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

            const string instr = "add $1, $2, $3";
            cpu.AddInstruction(new Add(instr, 0, 1, 2, 3));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Add: rd = $1, rs = $2, rt = $3", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Add 10 + 255 = 265", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $1 <= 265", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(265, cpu.RegRead(1));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestAddi()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(1, 123456);

            const string instr = "addi $2, $1, 123456";
            cpu.AddInstruction(new Addi(instr, 0, 2, 1, 123456));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Addi: rt = $2, rs = $1, imm = 123456", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Add 123456 + 123456 = 246912", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $2 <= 246912", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(246912, cpu.RegRead(2));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
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

            const string instr = "jal 4";

            cpu.AddInstruction(new Jal(instr, 0, 4));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 1)); //Should jump to this instruction

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Jal: imm = 4", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Jump should have been taken
            Assert.AreEqual(4 << 2, cpu.GetPC());

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "None", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Nop: ", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $15 <= 4", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(1 << 2, cpu.RegRead(15));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
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

            const string instr = "lw $5, 4($6)";
            var instructionAdd = new LW(instr, 0, 5, 4, 6);
            cpu.AddInstruction(instructionAdd);

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "LW: rs = $6, rt = $5, imm = 4", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "LW Address = 16 + 4 = 20", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "Memory access result = 123456789", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $5 <= 123456789", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(123456789, cpu.RegRead(5));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestSW()
        {
            var cpu = new CPU();
            cpu.RegWrite(5, 123456789);
            cpu.RegWrite(6, 4 << 2);

            const string instr = "sw $5, 4($6)";

            var instructionAdd = new SW(instr, 0, 5, 4, 6);
            cpu.AddInstruction(instructionAdd);

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "SW: rs = $6, rt = $5, imm = 4", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "SW Address = 16 + 4 = 20", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "Value written in memory = 123456789", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(123456789, cpu.Load((4 << 2) + 4));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestXor()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 1565861035);
            cpu.RegWrite(3, 882150058);

            const string instr = "xor $1, $2, $3";
            cpu.AddInstruction(new Xor(instr, 0, 1, 2, 3));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Xor: rd = $1, rs = $2, rt = $3", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Xor 1565861035 ^ 882150058 = 1774300673", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $1 <= 1774300673", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(1774300673, cpu.RegRead(1));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestSlt()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(2, 100);
            cpu.RegWrite(3, 200);

            const string instr = "slt $1, $2, $3";
            cpu.AddInstruction(new Slt(instr, 0, 1, 2, 3));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Slt: rd = $1, rs = $2, rt = $3", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Slt 100 < 200 = 1", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $1 <= 1", 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            //Test value inside register file
            Assert.AreEqual(1, cpu.RegRead(1));
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestBleTaken()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(1, 100);
            cpu.RegWrite(2, 200);

            const string instr = "ble $1, $2, 5";

            cpu.AddInstruction(new Ble(instr, 0, 1, 2, 5));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Ble: rs = $1, rt = $2, imm = 5", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Ble 100 <= 200 = True", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Nop: ", 1, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            Assert.AreEqual(6 << 2, cpu.GetPC());
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestBleNotTaken()
        {
            var cpu = new CPU();

            //Set initial values in register file
            cpu.RegWrite(1, 200);
            cpu.RegWrite(2, 100);

            const string instr = "ble $1, $2, 5";

            cpu.AddInstruction(new Ble(instr, 0, 1, 2, 5));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));
            cpu.AddInstruction(new Nop("nop", 0));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Ble: rs = $1, rt = $2, imm = 5", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Ble 200 <= 100 = False", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Nop: ", 1, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            Assert.AreEqual(3 << 2, cpu.GetPC());
            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }
    }
}
