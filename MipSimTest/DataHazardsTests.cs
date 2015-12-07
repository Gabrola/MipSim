using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MipSim;
using MipSim.Instructions;

namespace MipSimTest
{
    [TestClass]
    public class DataHazardsTests
    {
        [TestMethod]
        public void TestLoadUseHazard()
        {
            var cpu = new CPU();
            cpu.Store(0, 100);

            const string instr0 = "lw $2, 0($0)";
            const string instr1 = "add $3, $2, $0";

            cpu.AddInstruction(new LW(instr0, 1, 2, 0, 1));
            cpu.AddInstruction(new Add(instr1, 2, 3, 2, 0));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr0, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "LW: rs = $1, rt = $2, imm = 0", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr1, 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "LW Address = 0 + 0 = 0", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Add: rd = $3, rs = $2, rt = $0", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "Memory access result = 100", 0, null));
            clockCycle++;
            //STALL LOCATION
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $2 <= 100", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Add 100 + 0 = 100", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "Register $3 <= 100", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }

        [TestMethod]
        public void TestBranchPrediction()
        {
            var cpu = new CPU();
            cpu.Store(0, 100);

            const string instr0 = "ble $0, $0, 3";
            const string instr5 = "j 0";

            cpu.AddInstruction(new Ble(instr0, 0, 0, 0, 4));
            cpu.AddInstruction(new Nop("nop", 1));
            cpu.AddInstruction(new Nop("nop", 2));
            cpu.AddInstruction(new Nop("nop", 3));
            cpu.AddInstruction(new Nop("nop", 4));
            cpu.AddInstruction(new J(instr5, 5, 0));

            var expectedRecords = new List<ExecutionRecordList>();
            int clockCycle = 0;

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr0, 0, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Ble: rs = $0, rt = $0, imm = 4", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 1, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "Ble 0 <= 0 = True", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Nop: ", 1, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, "nop", 2, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr5, 3, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Writeback, "None", 0, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "J: imm = 0", 3, null));
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Execute, "None", 3, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr0, 4, null));
            //Should branch directly after this fetch
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            cpu.RunClock();
            expectedRecords.Add(new ExecutionRecordList());
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Memory, "None", 3, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Decode, "Ble: rs = $0, rt = $0, imm = 4", 4, null));
            expectedRecords[clockCycle].Add(new ExecutionRecord(ExecutionType.Fetch, instr5, 5, null));
            //Branch predicted
            clockCycle++;
            Assert.IsTrue(expectedRecords.SequenceEqual(cpu.ExecutionRecords));

            Assert.AreEqual(clockCycle, cpu.ClockCycle);
        }
    }
}
